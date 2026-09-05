using Microsoft.Data.SqlClient;
using OnlineFeedbackSystem.Models.Entities;
using OnlineFeedbackSystem.Models.ViewModels;
using OnlineFeedbackSystem.Web.Data;
using System.Data;

namespace OnlineFeedbackSystem.Repositories
{
    public class NotificationsRepository : INotificationsRepository
    {
        private readonly DbHelper _dbHelper;

        public NotificationsRepository(DbHelper dbHelper)
        {
            _dbHelper = dbHelper;
        }

        // Your DB already has tables/SPs - do not re-create schema here.
        public void EnsureTableAndSeed(int? userId)
        {
            // no-op: schema and seeding are managed via your DB and stored procedures
        }

        public NotificationIndexViewModel GetNotifications(int? userId, string? filterType = null, bool unreadOnly = false)
        {
            var viewModel = new NotificationIndexViewModel
            {
                SelectedFilter = string.IsNullOrWhiteSpace(filterType) ? (unreadOnly ? "Unread" : "All") : filterType
            };

            using var connection = _dbHelper.CreateOpenConnection();

            // 1) Get stats via stored procedure (expects @UserId)
            using (var statsCmd = new SqlCommand("GetNotificationStats", connection))
            {
                statsCmd.CommandType = CommandType.StoredProcedure;
                statsCmd.Parameters.AddWithValue("@UserId", (object?)userId ?? DBNull.Value);

                using var reader = statsCmd.ExecuteReader();
                if (reader.Read())
                {
                    viewModel.TotalCount = reader.IsDBNull(0) ? 0 : reader.GetInt32(0);
                    viewModel.UnreadCount = reader.IsDBNull(1) ? 0 : reader.GetInt32(1);
                    viewModel.AlertCount = reader.IsDBNull(2) ? 0 : reader.GetInt32(2);
                    viewModel.SubmissionCount = reader.IsDBNull(3) ? 0 : reader.GetInt32(3);
                }
                reader.Close();
            }

            // 2) Get notification rows via stored procedure (expects @UserId, @FilterType, @UnreadOnly)
            using (var listCmd = new SqlCommand("GetNotifications", connection))
            {
                listCmd.CommandType = CommandType.StoredProcedure;
                listCmd.Parameters.AddWithValue("@UserId", (object?)userId ?? DBNull.Value);
                listCmd.Parameters.AddWithValue("@FilterType", (object?)viewModel.SelectedFilter ?? DBNull.Value);
                listCmd.Parameters.AddWithValue("@UnreadOnly", unreadOnly);

                using var reader = listCmd.ExecuteReader();
                while (reader.Read())
                {
                    viewModel.Notifications.Add(new NotificationItemViewModel
                    {
                        Id = reader.GetInt32(reader.GetOrdinal("Id")),
                        UserId = reader.IsDBNull(reader.GetOrdinal("UserId")) ? null : reader.GetInt32(reader.GetOrdinal("UserId")),
                        FormId = reader.IsDBNull(reader.GetOrdinal("FormId")) ? null : reader.GetInt32(reader.GetOrdinal("FormId")),
                        Type = reader.GetString(reader.GetOrdinal("Type")),
                        Message = reader.GetString(reader.GetOrdinal("Message")),
                        IsRead = reader.GetBoolean(reader.GetOrdinal("IsRead")),
                        CreatedAt = reader.GetDateTime(reader.GetOrdinal("CreatedAt")),
                        FormTitle = reader.IsDBNull(reader.GetOrdinal("FormTitle")) ? null : reader.GetString(reader.GetOrdinal("FormTitle"))
                    });
                }
            }

            return viewModel;
        }

        public int GetUnreadCount(int? userId)
        {
            using var connection = _dbHelper.CreateOpenConnection();
            using var cmd = new SqlCommand("GetUnreadNotificationsCount", connection)
            {
                CommandType = CommandType.StoredProcedure
            };
            cmd.Parameters.AddWithValue("@UserId", (object?)userId ?? DBNull.Value);
            return Convert.ToInt32(cmd.ExecuteScalar());
        }

        public bool MarkAsRead(int id, int? userId)
        {
            using var connection = _dbHelper.CreateOpenConnection();
            using var cmd = new SqlCommand("MarkNotificationAsRead", connection)
            {
                CommandType = CommandType.StoredProcedure
            };
            cmd.Parameters.AddWithValue("@Id", id);
            cmd.Parameters.AddWithValue("@UserId", (object?)userId ?? DBNull.Value);
            return cmd.ExecuteNonQuery() > 0;
        }

        public bool MarkAllAsRead(int? userId)
        {
            using var connection = _dbHelper.CreateOpenConnection();
            using var cmd = new SqlCommand("MarkAllNotificationsAsRead", connection)
            {
                CommandType = CommandType.StoredProcedure
            };
            cmd.Parameters.AddWithValue("@UserId", (object?)userId ?? DBNull.Value);
            return cmd.ExecuteNonQuery() > 0;
        }

        public bool Delete(int id, int? userId)
        {
            using var connection = _dbHelper.CreateOpenConnection();
            using var cmd = new SqlCommand("DeleteNotification", connection)
            {
                CommandType = CommandType.StoredProcedure
            };
            cmd.Parameters.AddWithValue("@Id", id);
            cmd.Parameters.AddWithValue("@UserId", (object?)userId ?? DBNull.Value);
            return cmd.ExecuteNonQuery() > 0;
        }

        public bool ClearAllRead(int? userId)
        {
            using var connection = _dbHelper.CreateOpenConnection();
            using var cmd = new SqlCommand("ClearAllReadNotifications", connection)
            {
                CommandType = CommandType.StoredProcedure
            };
            cmd.Parameters.AddWithValue("@UserId", (object?)userId ?? DBNull.Value);
            return cmd.ExecuteNonQuery() > 0;
        }

        public int Create(Notifications notification)
        {
            using var connection = _dbHelper.CreateOpenConnection();
            using var cmd = new SqlCommand("CreateNotification", connection)
            {
                CommandType = CommandType.StoredProcedure
            };

            cmd.Parameters.AddWithValue("@UserId", (object?)notification.UserId ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@FormId", (object?)notification.FormId ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@Type", notification.Type);
            cmd.Parameters.AddWithValue("@Message", notification.Message);
            cmd.Parameters.AddWithValue("@IsRead", notification.IsRead);

            return Convert.ToInt32(cmd.ExecuteScalar());
        }

        public void NotifyUser(int userId, string type, string message, int? formId = null)
        {
            try
            {
                Create(new Notifications
                {
                    UserId = userId,
                    FormId = formId,
                    Type = type,
                    Message = message,
                    IsRead = false,
                    CreatedAt = DateTime.Now
                });
            }
            catch
            {
                // Ignore notification error so primary operation is not blocked
            }
        }

        public void NotifySuperAdmins(string type, string message, int? formId = null)
        {
            try
            {
                var superAdminIds = new List<int>();
                using (var connection = _dbHelper.CreateOpenConnection())
                {
                    using var cmd = new SqlCommand(@"
                        SELECT u.UserId 
                        FROM Users u 
                        INNER JOIN Roles r ON u.RoleId = r.RoleId 
                        WHERE r.RoleName = 'SuperAdmin' AND u.IsActive = 1", connection);

                    using var reader = cmd.ExecuteReader();
                    while (reader.Read())
                    {
                        superAdminIds.Add(reader.GetInt32(0));
                    }
                }

                if (superAdminIds.Count > 0)
                {
                    foreach (var id in superAdminIds)
                    {
                        Create(new Notifications
                        {
                            UserId = id,
                            FormId = formId,
                            Type = type,
                            Message = message,
                            IsRead = false,
                            CreatedAt = DateTime.Now
                        });
                    }
                }
                else
                {
                    Create(new Notifications
                    {
                        UserId = null,
                        FormId = formId,
                        Type = type,
                        Message = message,
                        IsRead = false,
                        CreatedAt = DateTime.Now
                    });
                }
            }
            catch
            {
                // Ignore notification error so primary operation is not blocked
            }
        }
    }
}
