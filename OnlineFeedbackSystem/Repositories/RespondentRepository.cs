using System.Data;
using Microsoft.Data.SqlClient;
using OnlineFeedbackSystem.Models.Entities;
using OnlineFeedbackSystem.Models.ViewModels;
using OnlineFeedbackSystem.Web.Data;

namespace OnlineFeedbackSystem.Repositories
{
    public class RespondentRepository : IRespondentRepository
    {
        private readonly DbHelper _dbHelper;

        public RespondentRepository(DbHelper dbHelper)
        {
            _dbHelper = dbHelper;
        }

        public List<RespondentAvailableFormItemViewModel> GetAvailableForms(int? userId = null)
        {
            var list = new List<RespondentAvailableFormItemViewModel>();

            using var connection = _dbHelper.CreateOpenConnection();
            using var command = new SqlCommand("GetAvailableFeedbackForms", connection);
            command.CommandType = CommandType.StoredProcedure;
            command.Parameters.AddWithValue("@UserId", (object?)userId ?? DBNull.Value);

            using var reader = command.ExecuteReader();
            while (reader.Read())
            {
                list.Add(new RespondentAvailableFormItemViewModel
                {
                    Id = reader.GetInt32(reader.GetOrdinal("Id")),
                    Title = reader.GetString(reader.GetOrdinal("Title")),
                    Description = reader.IsDBNull(reader.GetOrdinal("Description")) ? null : reader.GetString(reader.GetOrdinal("Description")),
                    Category = reader.GetString(reader.GetOrdinal("Category")),
                    DepartmentId = GetNullableInt(reader, "DepartmentId"),
                    DepartmentName = GetNullableString(reader, "DepartmentName"),
                    EndDate = reader.GetDateTime(reader.GetOrdinal("EndDate")),
                    IsAnonymous = reader.GetBoolean(reader.GetOrdinal("IsAnonymous")),
                    QuestionCount = reader.GetInt32(reader.GetOrdinal("QuestionCount")),
                    AlreadySubmitted = reader.GetInt32(reader.GetOrdinal("AlreadySubmitted")) == 1
                });
            }

            return list;
        }

        public RespondentDashboardViewModel GetRespondentDashboard(int? userId, string? userName = null)
        {
            var availableForms = GetAvailableForms(userId);
            var submissions = userId.HasValue ? GetUserSubmissions(userId.Value) : new List<RespondentSubmissionHistoryItemViewModel>();

            int completed = availableForms.Count(f => f.AlreadySubmitted);
            int pending = availableForms.Count(f => !f.AlreadySubmitted);

            return new RespondentDashboardViewModel
            {
                RespondentName = !string.IsNullOrWhiteSpace(userName) ? userName : "Respondent",
                IsAuthenticated = userId.HasValue,
                TotalAvailableSurveys = availableForms.Count,
                CompletedSurveys = completed,
                PendingSurveys = pending,
                AvailableForms = availableForms,
                RecentSubmissions = submissions
            };
        }

        public List<RespondentSubmissionHistoryItemViewModel> GetUserSubmissions(int userId)
        {
            var list = new List<RespondentSubmissionHistoryItemViewModel>();
            using var connection = _dbHelper.CreateOpenConnection();

            using var command = new SqlCommand("GetUserSubmissions", connection)
            {
                CommandType = CommandType.StoredProcedure
            };
            command.Parameters.AddWithValue("@UserId", userId);

            using var reader = command.ExecuteReader();
            while (reader.Read())
            {
                list.Add(new RespondentSubmissionHistoryItemViewModel
                {
                    ResponseId = reader.GetInt32(reader.GetOrdinal("Id")),
                    FormId = reader.GetInt32(reader.GetOrdinal("FormId")),
                    FormTitle = reader.GetString(reader.GetOrdinal("Title")),
                    Category = reader.GetString(reader.GetOrdinal("Category")),
                    SubmittedAt = reader.GetDateTime(reader.GetOrdinal("SubmittedAt")),
                    SubmissionToken = reader.IsDBNull(reader.GetOrdinal("SubmissionToken")) ? string.Empty : reader.GetGuid(reader.GetOrdinal("SubmissionToken")).ToString(),
                    Status = reader.IsDBNull(reader.GetOrdinal("Status")) ? "Submitted" : reader.GetString(reader.GetOrdinal("Status"))
                });
            }

            return list;
        }

        public RespondentFormFillingViewModel? GetFormForFilling(int formId)
        {
            using var connection = _dbHelper.CreateOpenConnection();

            // 1. Get Form Header via stored procedure
            RespondentFormFillingViewModel? model = null;
            using (var command = new SqlCommand("GetFeedbackFormById", connection))
            {
                command.CommandType = CommandType.StoredProcedure;
                command.Parameters.AddWithValue("@Id", formId);

                using var reader = command.ExecuteReader();
                if (reader.Read())
                {
                    model = new RespondentFormFillingViewModel
                    {
                        FormId = reader.GetInt32(reader.GetOrdinal("Id")),
                        Title = reader.GetString(reader.GetOrdinal("Title")),
                        Description = reader.IsDBNull(reader.GetOrdinal("Description")) ? null : reader.GetString(reader.GetOrdinal("Description")),
                        Category = reader.GetString(reader.GetOrdinal("Category")),
                        DepartmentId = GetNullableInt(reader, "DepartmentId"),
                        DepartmentName = GetNullableString(reader, "DepartmentName"),
                        IsAnonymous = reader.GetBoolean(reader.GetOrdinal("IsAnonymous")),
                        OneResponseOnly = reader.GetBoolean(reader.GetOrdinal("OneResponseOnly"))
                    };
                }
            }

            if (model == null) return null;

            // 2. Get Assigned Questions via stored procedure
            var questionsDict = new Dictionary<int, RespondentQuestionInputViewModel>();
            using (var qCmd = new SqlCommand("GetFormQuestions", connection))
            {
                qCmd.CommandType = CommandType.StoredProcedure;
                qCmd.Parameters.AddWithValue("@FormId", formId);

                using var qReader = qCmd.ExecuteReader();
                while (qReader.Read())
                {
                    var q = new RespondentQuestionInputViewModel
                    {
                        QuestionId = qReader.GetInt32(qReader.GetOrdinal("QuestionId")),
                        QuestionText = qReader.GetString(qReader.GetOrdinal("QuestionText")),
                        QuestionType = qReader.GetString(qReader.GetOrdinal("QuestionType")),
                        Category = qReader.GetString(qReader.GetOrdinal("Category")),
                        IsRequired = qReader.GetBoolean(qReader.GetOrdinal("IsRequired")),
                        DisplayOrder = qReader.GetInt32(qReader.GetOrdinal("DisplayOrder"))
                    };
                    model.Questions.Add(q);
                    questionsDict[q.QuestionId] = q;
                }
            }

            // 3. Get Options for choice questions via stored procedure
            if (model.Questions.Count > 0)
            {
                using var optCmd = new SqlCommand("GetQuestionOptions", connection);
                optCmd.CommandType = CommandType.StoredProcedure;
                optCmd.Parameters.AddWithValue("@QuestionId", DBNull.Value);

                using var optReader = optCmd.ExecuteReader();
                while (optReader.Read())
                {
                    int qId = optReader.GetInt32(optReader.GetOrdinal("QuestionId"));
                    string optText = optReader.GetString(optReader.GetOrdinal("OptionText"));
                    if (questionsDict.TryGetValue(qId, out var item))
                    {
                        item.Options.Add(optText);
                    }
                }
            }

            return model;
        }

        public bool HasUserSubmitted(int formId, int userId)
        {
            using var connection = _dbHelper.CreateOpenConnection();
            using var command = new SqlCommand("CheckUserFormSubmission", connection);
            command.CommandType = CommandType.StoredProcedure;
            command.Parameters.AddWithValue("@FormId", formId);
            command.Parameters.AddWithValue("@UserId", userId);

            var result = command.ExecuteScalar();
            return Convert.ToInt32(result) == 1;
        }

        public string SubmitFeedback(int formId, int? userId, List<RespondentQuestionInputViewModel> answers)
        {
            Guid submissionToken = Guid.NewGuid();

            using var connection = _dbHelper.CreateOpenConnection();
            using var transaction = connection.BeginTransaction();

            try
            {
                // 1. Call stored procedure to create response
                int responseId;
                using (var command = new SqlCommand("SubmitFeedbackResponse", connection, transaction))
                {
                    command.CommandType = CommandType.StoredProcedure;
                    command.Parameters.AddWithValue("@FormId", formId);
                    command.Parameters.AddWithValue("@UserId", (object?)userId ?? DBNull.Value);

                    var p = command.Parameters.Add("@SubmissionToken", SqlDbType.UniqueIdentifier);
                    p.Value = submissionToken;

                    responseId = Convert.ToInt32(command.ExecuteScalar());
                }

                // 2. Call InsertFeedbackAnswer stored procedure for each answer
                foreach (var ans in answers)
                {
                    string? answerText = null;
                    double? numericValue = ans.NumericAnswer;

                    if (ans.QuestionType == "5-Star Rating" || ans.QuestionType == "1-10 Rating")
                    {
                        numericValue = ans.NumericAnswer;
                        answerText = ans.NumericAnswer?.ToString();
                    }
                    else if (ans.QuestionType == "Multiple Choice" && ans.SelectedChoices != null && ans.SelectedChoices.Count > 0)
                    {
                        answerText = string.Join(", ", ans.SelectedChoices);
                    }
                    else if (!string.IsNullOrWhiteSpace(ans.TextAnswer))
                    {
                        answerText = ans.TextAnswer.Trim();
                    }

                    using var ansCmd = new SqlCommand("InsertFeedbackAnswer", connection, transaction);
                    ansCmd.CommandType = CommandType.StoredProcedure;
                    ansCmd.Parameters.AddWithValue("@ResponseId", responseId);
                    ansCmd.Parameters.AddWithValue("@QuestionId", ans.QuestionId);
                    ansCmd.Parameters.AddWithValue("@AnswerText", (object?)answerText ?? DBNull.Value);
                    ansCmd.Parameters.AddWithValue("@NumericValue", (object?)numericValue ?? DBNull.Value);
                    ansCmd.ExecuteNonQuery();
                }

                transaction.Commit();
                return submissionToken.ToString();
            }
            catch
            {
                transaction.Rollback();
                throw;
            }
        }

        private static int? GetNullableInt(SqlDataReader reader, string columnName)
        {
            try
            {
                int ordinal = reader.GetOrdinal(columnName);
                if (reader.IsDBNull(ordinal)) return null;
                return reader.GetInt32(ordinal);
            }
            catch
            {
                return null;
            }
        }

        private static string? GetNullableString(SqlDataReader reader, string columnName)
        {
            try
            {
                int ordinal = reader.GetOrdinal(columnName);
                if (reader.IsDBNull(ordinal)) return null;
                return reader.GetString(ordinal);
            }
            catch
            {
                return null;
            }
        }
    }
}