using System.Data;
using Microsoft.Data.SqlClient;
using OnlineFeedbackSystem.Models.Entities;
using OnlineFeedbackSystem.Models.ViewModels;
using OnlineFeedbackSystem.Web.Data;

namespace OnlineFeedbackSystem.Repositories
{
    public class AuditLogRepository : IAuditLogRepository
    {
        private readonly DbHelper _dbHelper;

        public AuditLogRepository(DbHelper dbHelper)
        {
            _dbHelper = dbHelper;
        }

        public List<AuditLogs> GetLogs(string? action = null, string? entityName = null, string? search = null, DateTime? startDate = null, DateTime? endDate = null, int top = 200)
        {
            var logs = new List<AuditLogs>();

            using var connection = _dbHelper.CreateOpenConnection();
            using var command = new SqlCommand("GetAuditLogs", connection);
            command.CommandType = CommandType.StoredProcedure;

            command.Parameters.AddWithValue("@Action", string.IsNullOrWhiteSpace(action) ? DBNull.Value : action.Trim());
            command.Parameters.AddWithValue("@EntityName", string.IsNullOrWhiteSpace(entityName) ? DBNull.Value : entityName.Trim());
            command.Parameters.AddWithValue("@Search", string.IsNullOrWhiteSpace(search) ? DBNull.Value : search.Trim());
            command.Parameters.AddWithValue("@StartDate", startDate.HasValue ? startDate.Value : DBNull.Value);
            command.Parameters.AddWithValue("@EndDate", endDate.HasValue ? endDate.Value : DBNull.Value);
            command.Parameters.AddWithValue("@Top", top);

            using var reader = command.ExecuteReader();
            while (reader.Read())
            {
                int userIdOrdinal = reader.GetOrdinal("UserId");
                int entityNameOrdinal = reader.GetOrdinal("EntityName");
                int entityIdOrdinal = reader.GetOrdinal("EntityId");
                int detailsOrdinal = reader.GetOrdinal("Details");
                int userNameOrdinal = reader.GetOrdinal("UserName");
                int userEmailOrdinal = reader.GetOrdinal("UserEmail");
                int roleNameOrdinal = reader.GetOrdinal("RoleName");

                logs.Add(new AuditLogs
                {
                    Id = reader.GetInt32(reader.GetOrdinal("Id")),
                    UserId = reader.IsDBNull(userIdOrdinal) ? null : reader.GetInt32(userIdOrdinal),
                    Action = reader.GetString(reader.GetOrdinal("Action")),
                    EntityName = reader.IsDBNull(entityNameOrdinal) ? null : reader.GetString(entityNameOrdinal),
                    EntityId = reader.IsDBNull(entityIdOrdinal) ? null : reader.GetInt32(entityIdOrdinal),
                    Details = reader.IsDBNull(detailsOrdinal) ? null : reader.GetString(detailsOrdinal),
                    CreatedAt = reader.GetDateTime(reader.GetOrdinal("CreatedAt")),
                    UserName = reader.IsDBNull(userNameOrdinal) ? null : reader.GetString(userNameOrdinal),
                    UserEmail = reader.IsDBNull(userEmailOrdinal) ? null : reader.GetString(userEmailOrdinal),
                    RoleName = reader.IsDBNull(roleNameOrdinal) ? null : reader.GetString(roleNameOrdinal)
                });
            }

            return logs;
        }

        public AuditLogStatsDto GetStats()
        {
            var stats = new AuditLogStatsDto();

            using var connection = _dbHelper.CreateOpenConnection();
            using var command = new SqlCommand("GetAuditLogStats", connection);
            command.CommandType = CommandType.StoredProcedure;

            using var reader = command.ExecuteReader();

            // 1. Total Logs
            if (reader.Read())
            {
                stats.TotalLogs = reader.GetInt32(0);
            }

            // 2. Today's Logs
            if (reader.NextResult() && reader.Read())
            {
                stats.TodayLogs = reader.GetInt32(0);
            }

            // 3. Distinct Active Users
            if (reader.NextResult() && reader.Read())
            {
                stats.ActiveUsers = reader.GetInt32(0);
            }

            // 4. Form Actions
            if (reader.NextResult() && reader.Read())
            {
                stats.FormActions = reader.GetInt32(0);
            }

            // 5. Distinct Actions
            if (reader.NextResult())
            {
                while (reader.Read())
                {
                    if (!reader.IsDBNull(0))
                    {
                        stats.ActionTypes.Add(reader.GetString(0));
                    }
                }
            }

            // 6. Distinct Entities
            if (reader.NextResult())
            {
                while (reader.Read())
                {
                    if (!reader.IsDBNull(0))
                    {
                        stats.EntityTypes.Add(reader.GetString(0));
                    }
                }
            }

            return stats;
        }

        public int InsertLog(int? userId, string action, string? entityName = null, int? entityId = null, string? details = null)
        {
            try
            {
                using var connection = _dbHelper.CreateOpenConnection();
                using var command = new SqlCommand("InsertAuditLog", connection);
                command.CommandType = CommandType.StoredProcedure;

                command.Parameters.AddWithValue("@UserId", (object?)userId ?? DBNull.Value);
                command.Parameters.AddWithValue("@Action", action.Trim());
                command.Parameters.AddWithValue("@EntityName", string.IsNullOrWhiteSpace(entityName) ? DBNull.Value : entityName.Trim());
                command.Parameters.AddWithValue("@EntityId", (object?)entityId ?? DBNull.Value);
                command.Parameters.AddWithValue("@Details", string.IsNullOrWhiteSpace(details) ? DBNull.Value : details.Trim());

                return Convert.ToInt32(command.ExecuteScalar());
            }
            catch
            {
                // Audit logging should never break primary user workflows
                return 0;
            }
        }
    }
}
