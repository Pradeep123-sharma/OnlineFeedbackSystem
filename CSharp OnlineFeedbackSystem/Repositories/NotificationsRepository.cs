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

        public void EnsureTableAndSeed(int? userId)
        {
            try
            {
                using var connection = _dbHelper.CreateOpenConnection();

                // 1. Ensure table exists (schema matches your documentation)
                string createTableSql = @"
                IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Notifications]') AND type in (N'U'))
                BEGIN
                    CREATE TABLE [dbo].[Notifications] (
                        [Id] INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
                        [UserId] INT NOT NULL,
                        [FormId] INT NULL,
                        [Type] NVARCHAR(50) NOT NULL DEFAULT 'System',
                        [Message] NVARCHAR(1000) NOT NULL,
                        [IsRead] BIT NOT NULL DEFAULT 0,
                        [CreatedAt] DATETIME2(7) NOT NULL DEFAULT SYSUTCDATETIME()
                    );
                END";

                using (var cmd = new SqlCommand(createTableSql, connection))
                {
                    cmd.ExecuteNonQuery();
                }

                // 2. Check if table is empty, seed initial sample notifications (match reduced columns)
                string checkCountSql = "SELECT COUNT(1) FROM dbo.Notifications;";
                using (var checkCmd = new SqlCommand(checkCountSql, connection))
                {
                    int count = Convert.ToInt32(checkCmd.ExecuteScalar());
                    if (count == 0)
                    {
                        string seedSql = @"
                        INSERT INTO dbo.Notifications (UserId, FormId, Type, Message, IsRead, CreatedAt)
                        VALUES 
                        (NULL, NULL, 'System', 'Welcome to Pulse Admin — your Pulse feedback management system is configured and ready.', 0, DATEADD(minute, -15, SYSUTCDATETIME())),
                        (NULL, 1, 'Submission', 'A new verified response has been submitted for \"Student Course Feedback\" with an overall score of 4.8/5.0.', 0, DATEADD(hour, -2, SYSUTCDATETIME())),
                        (NULL, 1, 'LowRating', 'Low rating received: a respondent gave 2.0/5.0 on question \"Course Material Clarity\". Please review.', 0, DATEADD(hour, -5, SYSUTCDATETIME())),
                        (NULL, 2, 'Publication', 'The form \"Campus Facilities Evaluation\" is now live and accepting responses.', 1, DATEADD(day, -1, SYSUTCDATETIME()));";

                        using var seedCmd = new SqlCommand(seedSql, connection);
                        seedCmd.ExecuteNonQuery();
                    }
                }
            }
            catch
            {
                // Fallback gracefully
            }
        }

        public NotificationIndexViewModel GetNotifications(int? userId, string? filterType = null, bool unreadOnly = false)
        {
            EnsureTableAndSeed(userId);

            var viewModel = new NotificationIndexViewModel
            {
                SelectedFilter = string.IsNullOrWhiteSpace(filterType) ? (unreadOnly ? "Unread" : "All") : filterType
            };

            using var connection = _dbHelper.CreateOpenConnection();

            // Fetch summary stats first
            string statsSql = @"
            SELECT 
                COUNT(1) AS TotalCount,
                SUM(CASE WHEN IsRead = 0 THEN 1 ELSE 0 END) AS UnreadCount,
                SUM(CASE WHEN Type = 'LowRating' THEN 1 ELSE 0 END) AS AlertCount,
                SUM(CASE WHEN Type = 'Submission' THEN 1 ELSE 0 END) AS SubmissionCount
            FROM Notifications
            WHERE (@UserId IS NULL OR UserId IS NULL OR UserId = @UserId);";

            using (var statsCmd = new SqlCommand(statsSql, connection))
            {
                statsCmd.Parameters.AddWithValue("@UserId", (object?)userId ?? DBNull.Value);
                using var statsReader = statsCmd.ExecuteReader();
                if (statsReader.Read())
                {
                    viewModel.TotalCount = statsReader.IsDBNull(0) ? 0 : statsReader.GetInt32(0);
                    viewModel.UnreadCount = statsReader.IsDBNull(1) ? 0 : statsReader.GetInt32(1);
                    viewModel.AlertCount = statsReader.IsDBNull(2) ? 0 : statsReader.GetInt32(2);
                    viewModel.SubmissionCount = statsReader.IsDBNull(3) ? 0 : statsReader.GetInt32(3);
                }
            }

            // Build query based on filter
            string querySql = @"
            SELECT 
                n.Id,
                n.UserId,
                n.FormId,
                n.Type,
                n.Message,
                n.IsRead,
                n.CreatedAt,
                f.Title AS FormTitle
            FROM Notifications n
            LEFT JOIN FeedbackForms f ON n.FormId = f.Id
            WHERE (@UserId IS NULL OR n.UserId IS NULL OR n.UserId = @UserId)";

            if (viewModel.SelectedFilter == "Unread")
            {
                querySql += " AND n.IsRead = 0";
            }
            else if (!string.IsNullOrEmpty(viewModel.SelectedFilter) && viewModel.SelectedFilter != "All")
            {
                querySql += " AND n.Type = @FilterType";
            }

            querySql += " ORDER BY n.CreatedAt DESC, n.Id DESC;";

            using (var queryCmd = new SqlCommand(querySql, connection))
            {
                queryCmd.Parameters.AddWithValue("@UserId", (object?)userId ?? DBNull.Value);
                if (!string.IsNullOrEmpty(viewModel.SelectedFilter) && viewModel.SelectedFilter != "All" && viewModel.SelectedFilter != "Unread")
                {
                    queryCmd.Parameters.AddWithValue("@FilterType", viewModel.SelectedFilter);
                }

                using var reader = queryCmd.ExecuteReader();
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
            EnsureTableAndSeed(userId);

            using var connection = _dbHelper.CreateOpenConnection();
            string sql = "SELECT COUNT(1) FROM Notifications WHERE (@UserId IS NULL OR UserId IS NULL OR UserId = @UserId) AND IsRead = 0;";
            using var cmd = new SqlCommand(sql, connection);
            cmd.Parameters.AddWithValue("@UserId", (object?)userId ?? DBNull.Value);
            return Convert.ToInt32(cmd.ExecuteScalar());
        }

        public bool MarkAsRead(int id, int? userId)
        {
            EnsureTableAndSeed(userId);

            using var connection = _dbHelper.CreateOpenConnection();
            string sql = "UPDATE Notifications SET IsRead = 1 WHERE Id = @Id AND (@UserId IS NULL OR UserId IS NULL OR UserId = @UserId);";
            using var cmd = new SqlCommand(sql, connection);
            cmd.Parameters.AddWithValue("@Id", id);
            cmd.Parameters.AddWithValue("@UserId", (object?)userId ?? DBNull.Value);
            return cmd.ExecuteNonQuery() > 0;
        }

        public bool MarkAllAsRead(int? userId)
        {
            EnsureTableAndSeed(userId);

            using var connection = _dbHelper.CreateOpenConnection();
            string sql = "UPDATE Notifications SET IsRead = 1 WHERE IsRead = 0 AND (@UserId IS NULL OR UserId IS NULL OR UserId = @UserId);";
            using var cmd = new SqlCommand(sql, connection);
            cmd.Parameters.AddWithValue("@UserId", (object?)userId ?? DBNull.Value);
            return cmd.ExecuteNonQuery() > 0;
        }

        public bool Delete(int id, int? userId)
        {
            EnsureTableAndSeed(userId);

            using var connection = _dbHelper.CreateOpenConnection();
            string sql = "DELETE FROM Notifications WHERE Id = @Id AND (@UserId IS NULL OR UserId IS NULL OR UserId = @UserId);";
            using var cmd = new SqlCommand(sql, connection);
            cmd.Parameters.AddWithValue("@Id", id);
            cmd.Parameters.AddWithValue("@UserId", (object?)userId ?? DBNull.Value);
            return cmd.ExecuteNonQuery() > 0;
        }

        public bool ClearAllRead(int? userId)
        {
            EnsureTableAndSeed(userId);

            using var connection = _dbHelper.CreateOpenConnection();
            string sql = "DELETE FROM Notifications WHERE IsRead = 1 AND (@UserId IS NULL OR UserId IS NULL OR UserId = @UserId);";
            using var cmd = new SqlCommand(sql, connection);
            cmd.Parameters.AddWithValue("@UserId", (object?)userId ?? DBNull.Value);
            return cmd.ExecuteNonQuery() > 0;
        }

        public int Create(Notifications notification)
        {
            EnsureTableAndSeed(notification.UserId);

            using var connection = _dbHelper.CreateOpenConnection();
            string sql = @"
            INSERT INTO Notifications (UserId, FormId, Type, Message, IsRead, CreatedAt)
            VALUES (@UserId, @FormId, @Type, @Message, @IsRead, SYSUTCDATETIME());
            SELECT CAST(SCOPE_IDENTITY() AS INT);";

            using var cmd = new SqlCommand(sql, connection);
            cmd.Parameters.AddWithValue("@UserId", (object?)notification.UserId ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@FormId", (object?)notification.FormId ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@Type", notification.Type);
            cmd.Parameters.AddWithValue("@Message", notification.Message);
            cmd.Parameters.AddWithValue("@IsRead", notification.IsRead);

            return Convert.ToInt32(cmd.ExecuteScalar());
        }
    }
}