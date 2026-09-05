using Microsoft.Data.SqlClient;
using OnlineFeedbackSystem.Models.ViewModels;
using OnlineFeedbackSystem.Web.Data;
using System.Data;

namespace OnlineFeedbackSystem.Repositories
{
    public class AnalyticsRepository : IAnalyticsRepository
    {
        private readonly DbHelper _dbHelper;

        public AnalyticsRepository(DbHelper dbHelper)
        {
            _dbHelper = dbHelper;
        }

        private static double? GetNullableDouble(SqlDataReader reader, int ordinal)
        {
            if (reader.IsDBNull(ordinal)) return null;
            return Convert.ToDouble(reader.GetValue(ordinal));
        }

        public FormAnalyticsViewModel? GetFormAnalytics(int formId)
        {
            using var connection = _dbHelper.CreateOpenConnection();
            using var command = new SqlCommand("GetFormAnalyticsSummary", connection);
            command.CommandType = CommandType.StoredProcedure;
            command.Parameters.AddWithValue("@FormId", formId);

            using var reader = command.ExecuteReader();

            FormAnalyticsViewModel? analytics = null;

            // Result Set 1: Form Summary
            if (reader.Read())
            {
                analytics = new FormAnalyticsViewModel
                {
                    FormId = reader.GetInt32(reader.GetOrdinal("FormId")),
                    Title = reader.GetString(reader.GetOrdinal("Title")),
                    Category = reader.GetString(reader.GetOrdinal("Category")),
                    Status = reader.GetString(reader.GetOrdinal("Status")),
                    StartDate = reader.GetDateTime(reader.GetOrdinal("StartDate")),
                    EndDate = reader.GetDateTime(reader.GetOrdinal("EndDate")),
                    TotalResponses = reader.GetInt32(reader.GetOrdinal("TotalResponses")),

                    // Use safe conversion
                    OverallAverageRating = GetNullableDouble(reader, reader.GetOrdinal("OverallAverageRating")) ?? 0.0,

                    QuestionCount = reader.GetInt32(reader.GetOrdinal("QuestionCount"))
                };
            }

            if (analytics == null) return null;

            // Result Set 2: Question Breakdown
            var questionDict = new Dictionary<int, QuestionAnalyticsItemViewModel>();
            if (reader.NextResult())
            {
                while (reader.Read())
                {
                    var qAnalytics = new QuestionAnalyticsItemViewModel
                    {
                        QuestionId = reader.GetInt32(reader.GetOrdinal("QuestionId")),
                        QuestionText = reader.GetString(reader.GetOrdinal("QuestionText")),
                        QuestionType = reader.GetString(reader.GetOrdinal("QuestionType")),
                        TotalAnswers = reader.GetInt32(reader.GetOrdinal("TotalAnswers")),
                        AverageScore = GetNullableDouble(reader, reader.GetOrdinal("AverageScore"))
                    };
                    analytics.QuestionAnalytics.Add(qAnalytics);
                    questionDict[qAnalytics.QuestionId] = qAnalytics;
                }
            }

            // Result Set 3: Option breakdown counts
            if (reader.NextResult())
            {
                while (reader.Read())
                {
                    int qId = reader.GetInt32(reader.GetOrdinal("QuestionId"));
                    string optText = reader.GetString(reader.GetOrdinal("OptionText"));
                    int count = reader.GetInt32(reader.GetOrdinal("OptionCount"));

                    if (questionDict.TryGetValue(qId, out var q))
                    {
                        double percentage = q.TotalAnswers > 0 ? Math.Round((double)count / q.TotalAnswers * 100, 1) : 0;
                        q.OptionBreakdown.Add(new OptionBreakdownItemViewModel
                        {
                            OptionText = optText,
                            Count = count,
                            Percentage = percentage
                        });
                    }
                }
            }

            // Result Set 4: Low rating alerts
            if (reader.NextResult())
            {
                while (reader.Read())
                {
                    analytics.LowRatingAlerts.Add(new LowRatingAlertViewModel
                    {
                        QuestionText = reader.GetString(reader.GetOrdinal("QuestionText")),
                        RatingValue = GetNullableDouble(reader, reader.GetOrdinal("RatingValue")) ?? 0.0,
                        RespondentInfo = reader.GetString(reader.GetOrdinal("RespondentInfo")),
                        SubmittedAt = reader.GetDateTime(reader.GetOrdinal("SubmittedAt"))
                    });
                }
            }

            // Result Set 5: Text feedback comments
            if (reader.NextResult())
            {
                while (reader.Read())
                {
                    analytics.TextComments.Add(new CommentFeedbackViewModel
                    {
                        QuestionText = reader.GetString(reader.GetOrdinal("QuestionText")),
                        Comment = reader.GetString(reader.GetOrdinal("Comment")),
                        RespondentInfo = reader.GetString(reader.GetOrdinal("RespondentInfo")),
                        SubmittedAt = reader.GetDateTime(reader.GetOrdinal("SubmittedAt"))
                    });
                }
            }

            return analytics;
        }
    }
}
