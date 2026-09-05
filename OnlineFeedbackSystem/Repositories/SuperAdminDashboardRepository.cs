using Microsoft.Data.SqlClient;
using OnlineFeedbackSystem.Models.ViewModels;
using OnlineFeedbackSystem.Web.Data;
using System.Data;

namespace OnlineFeedbackSystem.Repositories
{
    public class SuperAdminDashboardRepository : ISuperAdminDashboardRepository
    {
        private readonly DbHelper _dbHelper;

        public SuperAdminDashboardRepository(DbHelper dbHelper)
        {
            _dbHelper = dbHelper;
        }

        public DashboardStatsViewModel GetStats()
        {
            var stats = new DashboardStatsViewModel();

            using var connection = _dbHelper.CreateOpenConnection();

            using (var command = new SqlCommand("GetStatusDashboard", connection)
            {
                CommandType = CommandType.StoredProcedure
            })
            using (var reader = command.ExecuteReader())
            {
                reader.Read();
                stats.TotalForms = reader.GetInt32(0);

                reader.NextResult();
                reader.Read();
                stats.ActiveForms = reader.GetInt32(0);

                reader.NextResult();
                reader.Read();
                stats.TotalResponses = reader.GetInt32(0);

                reader.NextResult();
                reader.Read();
                // Use Convert on GetValue to support DECIMAL or FLOAT
                stats.AverageRating = Math.Round(Convert.ToDouble(reader.GetValue(0)), 2);

                reader.NextResult();
                reader.Read();
                stats.TotalUsers = reader.GetInt32(0);

                reader.NextResult();
                reader.Read();
                stats.TotalDepartments = reader.GetInt32(0);
            }

            stats.RecentUsers = GetRecentUsers(connection);

            return stats;
        }

        private static List<RecentUserViewModel> GetRecentUsers(SqlConnection connection)
        {
            var recentUsers = new List<RecentUserViewModel>();

            const string query = @"
                SELECT TOP (@Top)
                    u.[UserId],
                    u.[FullName],
                    u.[Email],
                    r.[RoleName],
                    d.[DepartmentName],
                    u.[IsActive],
                    u.[CreatedAt]
                FROM [dbo].[Users] u
                LEFT JOIN [dbo].[Roles] r ON u.[RoleId] = r.[RoleId]
                LEFT JOIN [dbo].[Departments] d ON u.[DepartmentId] = d.[DepartmentId]
                ORDER BY u.[CreatedAt] DESC, u.[UserId] DESC;";

            using var command = new SqlCommand(query, connection);
            command.Parameters.AddWithValue("@Top", 5);

            using var reader = command.ExecuteReader();
            int deptOrdinal = reader.GetOrdinal("DepartmentName");
            int roleOrdinal = reader.GetOrdinal("RoleName");

            while (reader.Read())
            {
                recentUsers.Add(new RecentUserViewModel
                {
                    UserId = reader.GetInt32(reader.GetOrdinal("UserId")),
                    FullName = reader.GetString(reader.GetOrdinal("FullName")),
                    Email = reader.GetString(reader.GetOrdinal("Email")),
                    RoleName = reader.IsDBNull(roleOrdinal) ? "User" : reader.GetString(roleOrdinal),
                    DepartmentName = reader.IsDBNull(deptOrdinal) ? null : reader.GetString(deptOrdinal),
                    IsActive = reader.GetBoolean(reader.GetOrdinal("IsActive")),
                    CreatedAt = reader.GetDateTime(reader.GetOrdinal("CreatedAt"))
                });
            }

            return recentUsers;
        }
    }
}
