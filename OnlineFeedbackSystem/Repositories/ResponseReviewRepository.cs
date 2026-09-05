using Microsoft.Data.SqlClient;
using OnlineFeedbackSystem.Models.ViewModels;
using OnlineFeedbackSystem.Web.Data;
using System.Data;

namespace OnlineFeedbackSystem.Repositories
{
    public class ResponseReviewRepository : IResponseReviewRepository
    {
        private readonly DbHelper _dbHelper;

        public ResponseReviewRepository(DbHelper dbHelper)
        {
            _dbHelper = dbHelper;
        }

        private static double? GetNullableDouble(SqlDataReader reader, string columnName)
        {
            int ord = reader.GetOrdinal(columnName);
            if (reader.IsDBNull(ord)) return null;
            return Convert.ToDouble(reader.GetValue(ord));
        }

        public List<ResponseListItemViewModel> GetResponses(int? formId = null)
        {
            var list = new List<ResponseListItemViewModel>();

            using var connection = _dbHelper.CreateOpenConnection();
            using var command = new SqlCommand("GetFormResponsesList", connection);
            command.CommandType = CommandType.StoredProcedure;
            command.Parameters.AddWithValue("@FormId", (object?)formId ?? DBNull.Value);

            using var reader = command.ExecuteReader();
            while (reader.Read())
            {
                list.Add(new ResponseListItemViewModel
                {
                    Id = reader.GetInt32(reader.GetOrdinal("Id")),
                    FormId = reader.GetInt32(reader.GetOrdinal("FormId")),
                    FormTitle = reader.GetString(reader.GetOrdinal("FormTitle")),
                    Category = reader.GetString(reader.GetOrdinal("Category")),
                    SubmissionToken = reader.IsDBNull(reader.GetOrdinal("SubmissionToken"))
                        ? string.Empty
                        : reader.GetGuid(reader.GetOrdinal("SubmissionToken")).ToString(),
                    SubmittedAt = reader.GetDateTime(reader.GetOrdinal("SubmittedAt")),
                    IsAnonymous = reader.GetBoolean(reader.GetOrdinal("IsAnonymous")),
                    RespondentName = reader.IsDBNull(reader.GetOrdinal("RespondentName")) ? string.Empty : reader.GetString(reader.GetOrdinal("RespondentName")),
                    AverageRating = GetNullableDouble(reader, "AverageRating")
                });
            }

            return list;
        }

        public ResponseDetailViewModel? GetResponseDetail(int responseId)
        {
            using var connection = _dbHelper.CreateOpenConnection();
            using var command = new SqlCommand("GetResponseDetailById", connection);
            command.CommandType = CommandType.StoredProcedure;
            command.Parameters.AddWithValue("@ResponseId", responseId);

            using var reader = command.ExecuteReader();

            ResponseDetailViewModel? detail = null;

            // Result Set 1: Header
            if (reader.Read())
            {
                detail = new ResponseDetailViewModel
                {
                    ResponseId = reader.GetInt32(reader.GetOrdinal("ResponseId")),
                    FormId = reader.GetInt32(reader.GetOrdinal("FormId")),
                    FormTitle = reader.GetString(reader.GetOrdinal("FormTitle")),
                    Category = reader.GetString(reader.GetOrdinal("Category")),
                    SubmissionToken = reader.IsDBNull(reader.GetOrdinal("SubmissionToken"))
                        ? string.Empty
                        : reader.GetGuid(reader.GetOrdinal("SubmissionToken")).ToString(),
                    SubmittedAt = reader.GetDateTime(reader.GetOrdinal("SubmittedAt")),
                    IsAnonymous = reader.GetBoolean(reader.GetOrdinal("IsAnonymous")),
                    RespondentName = reader.IsDBNull(reader.GetOrdinal("RespondentName")) ? string.Empty : reader.GetString(reader.GetOrdinal("RespondentName"))
                };
            }

            if (detail == null) return null;

            // Result Set 2: Answers
            if (reader.NextResult())
            {
                while (reader.Read())
                {
                    detail.Answers.Add(new ResponseAnswerDetailViewModel
                    {
                        QuestionId = reader.GetInt32(reader.GetOrdinal("QuestionId")),
                        QuestionText = reader.GetString(reader.GetOrdinal("QuestionText")),
                        QuestionType = reader.GetString(reader.GetOrdinal("QuestionType")),
                        AnswerText = reader.IsDBNull(reader.GetOrdinal("AnswerText")) ? null : reader.GetString(reader.GetOrdinal("AnswerText")),
                        NumericValue = GetNullableDouble(reader, "NumericValue")
                    });
                }
            }

            return detail;
        }
    }
}
