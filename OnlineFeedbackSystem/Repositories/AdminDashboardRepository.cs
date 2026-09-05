using Microsoft.Data.SqlClient;
using OnlineFeedbackSystem.Models.ViewModels;
using OnlineFeedbackSystem.Web.Data;
using System.Data;

namespace OnlineFeedbackSystem.Repositories
{
    public class AdminDashboardRepository : IAdminDashboardRepository
    {
        private readonly DbHelper _dbHelper;

        public AdminDashboardRepository(DbHelper dbHelper)
        {
            _dbHelper = dbHelper;
        }

        private static double GetNumericAsDouble(SqlDataReader reader, int ordinal, double fallback = 0.0)
        {
            if (reader.IsDBNull(ordinal)) return fallback;
            return Convert.ToDouble(reader.GetValue(ordinal));
        }

        public AdminDashboardViewModel GetDashboardStats(int? createdBy = null)
        {
            var viewModel = new AdminDashboardViewModel();

            using var connection = _dbHelper.CreateOpenConnection();
            using var command = new SqlCommand("GetAdminDashboardStats", connection);
            command.CommandType = CommandType.StoredProcedure;
            command.Parameters.AddWithValue("@CreatedBy", (object?)createdBy ?? DBNull.Value);

            using var reader = command.ExecuteReader();

            // Result Set 1: Summary Counts
            if (reader.Read())
            {
                viewModel.TotalForms = reader.GetInt32(reader.GetOrdinal("TotalForms"));
                viewModel.ActiveForms = reader.GetInt32(reader.GetOrdinal("ActiveForms"));
                viewModel.TotalResponses = reader.GetInt32(reader.GetOrdinal("TotalResponses"));

                // Use helper to safely convert any numeric DB type to double
                viewModel.AverageRating = GetNumericAsDouble(reader, reader.GetOrdinal("AverageRating"), 0.0);

                viewModel.TotalQuestionsInBank = reader.GetInt32(reader.GetOrdinal("TotalQuestionsInBank"));
            }

            // Result Set 2: Recent Forms
            if (reader.NextResult())
            {
                while (reader.Read())
                {
                    viewModel.RecentForms.Add(new AdminRecentFormItemViewModel
                    {
                        Id = reader.GetInt32(reader.GetOrdinal("Id")),
                        Title = reader.GetString(reader.GetOrdinal("Title")),
                        Category = reader.GetString(reader.GetOrdinal("Category")),
                        Status = reader.GetString(reader.GetOrdinal("Status")),
                        StartDate = reader.GetDateTime(reader.GetOrdinal("StartDate")),
                        EndDate = reader.GetDateTime(reader.GetOrdinal("EndDate")),
                        ResponseCount = reader.GetInt32(reader.GetOrdinal("ResponseCount")),
                        QuestionCount = reader.GetInt32(reader.GetOrdinal("QuestionCount"))
                    });
                }
            }

            return viewModel;
        }
    }
}
