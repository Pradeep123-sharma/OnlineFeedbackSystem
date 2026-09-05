using Microsoft.Data.SqlClient;
using OnlineFeedbackSystem.Models.Entities;
using OnlineFeedbackSystem.Models.ViewModels;
using OnlineFeedbackSystem.Web.Data;
using System.Data;

namespace OnlineFeedbackSystem.Repositories
{
    public class FeedbackFormRepository : IFeedbackFormRepository
    {
        private readonly DbHelper _dbHelper;

        public FeedbackFormRepository(DbHelper dbHelper)
        {
            _dbHelper = dbHelper;
        }

        public List<FeedbackFormListItemViewModel> GetAll(string? status = null, string? category = null, string? search = null, int? departmentId = null, int? createdBy = null)
        {
            var list = new List<FeedbackFormListItemViewModel>();

            using var connection = _dbHelper.CreateOpenConnection();
            using var command = new SqlCommand("GetAllFeedbackForms", connection);
            command.CommandType = CommandType.StoredProcedure;

            command.Parameters.AddWithValue("@Status", string.IsNullOrWhiteSpace(status) ? DBNull.Value : status.Trim());
            command.Parameters.AddWithValue("@Category", string.IsNullOrWhiteSpace(category) ? DBNull.Value : category.Trim());
            command.Parameters.AddWithValue("@Search", string.IsNullOrWhiteSpace(search) ? DBNull.Value : search.Trim());
            command.Parameters.AddWithValue("@DepartmentId", (object?)departmentId ?? DBNull.Value);
            command.Parameters.AddWithValue("@CreatedBy", (object?)createdBy ?? DBNull.Value);

            using var reader = command.ExecuteReader();
            int createdByOrdinal = -1;
            try { createdByOrdinal = reader.GetOrdinal("CreatedBy"); } catch { }

            while (reader.Read())
            {
                list.Add(new FeedbackFormListItemViewModel
                {
                    Id = reader.GetInt32(reader.GetOrdinal("Id")),
                    Title = reader.GetString(reader.GetOrdinal("Title")),
                    Description = reader.IsDBNull(reader.GetOrdinal("Description")) ? null : reader.GetString(reader.GetOrdinal("Description")),
                    Category = reader.GetString(reader.GetOrdinal("Category")),
                    DepartmentId = GetNullableInt(reader, "DepartmentId"),
                    DepartmentName = GetNullableString(reader, "DepartmentName"),
                    StartDate = reader.GetDateTime(reader.GetOrdinal("StartDate")),
                    EndDate = reader.GetDateTime(reader.GetOrdinal("EndDate")),
                    IsAnonymous = reader.GetBoolean(reader.GetOrdinal("IsAnonymous")),
                    OneResponseOnly = reader.GetBoolean(reader.GetOrdinal("OneResponseOnly")),
                    Status = reader.GetString(reader.GetOrdinal("Status")),
                    CreatedBy = createdByOrdinal >= 0 && !reader.IsDBNull(createdByOrdinal) ? reader.GetInt32(createdByOrdinal) : 0,
                    ResponseCount = reader.GetInt32(reader.GetOrdinal("ResponseCount")),
                    QuestionCount = reader.GetInt32(reader.GetOrdinal("QuestionCount"))
                });
            }

            return list;
        }

        public FeedbackForms? GetById(int id)
        {
            using var connection = _dbHelper.CreateOpenConnection();
            using var command = new SqlCommand("GetFeedbackFormById", connection);
            command.CommandType = CommandType.StoredProcedure;
            command.Parameters.AddWithValue("@Id", id);

            using var reader = command.ExecuteReader();
            if (reader.Read())
            {
                return new FeedbackForms
                {
                    Id = reader.GetInt32(reader.GetOrdinal("Id")),
                    Title = reader.GetString(reader.GetOrdinal("Title")),
                    Description = reader.IsDBNull(reader.GetOrdinal("Description")) ? null : reader.GetString(reader.GetOrdinal("Description")),
                    Category = reader.GetString(reader.GetOrdinal("Category")),
                    DepartmentId = GetNullableInt(reader, "DepartmentId"),
                    DepartmentName = GetNullableString(reader, "DepartmentName"),
                    StartDate = reader.GetDateTime(reader.GetOrdinal("StartDate")),
                    EndDate = reader.GetDateTime(reader.GetOrdinal("EndDate")),
                    IsAnonymous = reader.GetBoolean(reader.GetOrdinal("IsAnonymous")),
                    OneResponseOnly = reader.GetBoolean(reader.GetOrdinal("OneResponseOnly")),
                    Status = reader.GetString(reader.GetOrdinal("Status")),
                    CreatedBy = reader.GetInt32(reader.GetOrdinal("CreatedBy"))
                };
            }

            return null;
        }

        public int Create(FeedbackForms form)
        {
            using var connection = _dbHelper.CreateOpenConnection();
            using var command = new SqlCommand("CreateFeedbackForm", connection);
            command.CommandType = CommandType.StoredProcedure;

            command.Parameters.AddWithValue("@Title", form.Title.Trim());
            command.Parameters.AddWithValue("@Description", string.IsNullOrWhiteSpace(form.Description) ? DBNull.Value : form.Description.Trim());
            command.Parameters.AddWithValue("@Category", form.Category.Trim());
            command.Parameters.AddWithValue("@DepartmentId", (object?)form.DepartmentId ?? DBNull.Value);
            command.Parameters.AddWithValue("@StartDate", form.StartDate.Date);
            command.Parameters.AddWithValue("@EndDate", form.EndDate.Date);
            command.Parameters.AddWithValue("@IsAnonymous", form.IsAnonymous);
            command.Parameters.AddWithValue("@OneResponseOnly", form.OneResponseOnly);
            command.Parameters.AddWithValue("@Status", string.IsNullOrWhiteSpace(form.Status) ? "Draft" : form.Status);
            command.Parameters.AddWithValue("@CreatedBy", form.CreatedBy <= 0 ? 1 : form.CreatedBy);

            return Convert.ToInt32(command.ExecuteScalar());
        }

        public bool Update(FeedbackForms form)
        {
            using var connection = _dbHelper.CreateOpenConnection();
            using var command = new SqlCommand("UpdateFeedbackForm", connection);
            command.CommandType = CommandType.StoredProcedure;

            command.Parameters.AddWithValue("@Id", form.Id);
            command.Parameters.AddWithValue("@Title", form.Title.Trim());
            command.Parameters.AddWithValue("@Description", string.IsNullOrWhiteSpace(form.Description) ? DBNull.Value : form.Description.Trim());
            command.Parameters.AddWithValue("@Category", form.Category.Trim());
            command.Parameters.AddWithValue("@DepartmentId", (object?)form.DepartmentId ?? DBNull.Value);
            command.Parameters.AddWithValue("@StartDate", form.StartDate.Date);
            command.Parameters.AddWithValue("@EndDate", form.EndDate.Date);
            command.Parameters.AddWithValue("@IsAnonymous", form.IsAnonymous);
            command.Parameters.AddWithValue("@OneResponseOnly", form.OneResponseOnly);
            command.Parameters.AddWithValue("@Status", form.Status);

            return Convert.ToInt32(command.ExecuteScalar()) > 0;
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

        public bool ChangeStatus(int id, string status)
        {
            using var connection = _dbHelper.CreateOpenConnection();
            using var command = new SqlCommand("ChangeFormStatus", connection);
            command.CommandType = CommandType.StoredProcedure;

            command.Parameters.AddWithValue("@Id", id);
            command.Parameters.AddWithValue("@Status", status);

            return Convert.ToInt32(command.ExecuteScalar()) > 0;
        }

        public bool Delete(int id)
        {
            using var connection = _dbHelper.CreateOpenConnection();
            using var command = new SqlCommand("DeleteFeedbackForm", connection);
            command.CommandType = CommandType.StoredProcedure;

            command.Parameters.AddWithValue("@Id", id);

            return Convert.ToInt32(command.ExecuteScalar()) > 0;
        }

        public List<FormQuestionItemViewModel> GetFormQuestions(int formId)
        {
            var list = new List<FormQuestionItemViewModel>();
            var questionDict = new Dictionary<int, FormQuestionItemViewModel>();

            using var connection = _dbHelper.CreateOpenConnection();

            // 1. Get questions assigned to form
            using (var command = new SqlCommand("GetFormQuestions", connection))
            {
                command.CommandType = CommandType.StoredProcedure;
                command.Parameters.AddWithValue("@FormId", formId);

                using var reader = command.ExecuteReader();
                while (reader.Read())
                {
                    var item = new FormQuestionItemViewModel
                    {
                        FormQuestionId = reader.GetInt32(reader.GetOrdinal("FormQuestionId")),
                        FormId = reader.GetInt32(reader.GetOrdinal("FormId")),
                        QuestionId = reader.GetInt32(reader.GetOrdinal("QuestionId")),
                        IsRequired = reader.GetBoolean(reader.GetOrdinal("IsRequired")),
                        DisplayOrder = reader.GetInt32(reader.GetOrdinal("DisplayOrder")),
                        QuestionText = reader.GetString(reader.GetOrdinal("QuestionText")),
                        QuestionType = reader.GetString(reader.GetOrdinal("QuestionType")),
                        Category = reader.GetString(reader.GetOrdinal("Category"))
                    };
                    list.Add(item);
                    questionDict[item.QuestionId] = item;
                }
            }

            // 2. Load options for choices if any
            if (list.Count > 0)
            {
                using var optCmd = new SqlCommand("GetQuestionOptions", connection);
                optCmd.CommandType = CommandType.StoredProcedure;
                optCmd.Parameters.AddWithValue("@QuestionId", DBNull.Value);

                using var optReader = optCmd.ExecuteReader();
                while (optReader.Read())
                {
                    int qId = optReader.GetInt32(optReader.GetOrdinal("QuestionId"));
                    string optText = optReader.GetString(optReader.GetOrdinal("OptionText"));
                    if (questionDict.TryGetValue(qId, out var item))
                    {
                        item.Options.Add(optText);
                    }
                }
            }

            return list;
        }

        public int AddFormQuestion(int formId, int questionId, bool isRequired = true, int? displayOrder = null)
        {
            using var connection = _dbHelper.CreateOpenConnection();
            using var command = new SqlCommand("AddFormQuestion", connection);
            command.CommandType = CommandType.StoredProcedure;

            command.Parameters.AddWithValue("@FormId", formId);
            command.Parameters.AddWithValue("@QuestionId", questionId);
            command.Parameters.AddWithValue("@IsRequired", isRequired);
            command.Parameters.AddWithValue("@DisplayOrder", (object?)displayOrder ?? DBNull.Value);

            return Convert.ToInt32(command.ExecuteScalar());
        }

        public bool RemoveFormQuestion(int formId, int questionId)
        {
            using var connection = _dbHelper.CreateOpenConnection();
            using var command = new SqlCommand("RemoveFormQuestion", connection);
            command.CommandType = CommandType.StoredProcedure;

            command.Parameters.AddWithValue("@FormId", formId);
            command.Parameters.AddWithValue("@QuestionId", questionId);

            return Convert.ToInt32(command.ExecuteScalar()) > 0;
        }

        public bool ReorderFormQuestion(int formQuestionId, int displayOrder, bool isRequired)
        {
            using var connection = _dbHelper.CreateOpenConnection();
            using var command = new SqlCommand("ReorderFormQuestion", connection);
            command.CommandType = CommandType.StoredProcedure;

            command.Parameters.AddWithValue("@FormQuestionId", formQuestionId);
            command.Parameters.AddWithValue("@DisplayOrder", displayOrder);
            command.Parameters.AddWithValue("@IsRequired", isRequired);

            return Convert.ToInt32(command.ExecuteScalar()) > 0;
        }

        public bool ReorderAllQuestions(int formId, List<int> orderedQuestionIds)
        {
            if (orderedQuestionIds == null || orderedQuestionIds.Count == 0) return true;

            using var connection = _dbHelper.CreateOpenConnection();
            using var transaction = connection.BeginTransaction();

            try
            {
                for (int i = 0; i < orderedQuestionIds.Count; i++)
                {
                    int qId = orderedQuestionIds[i];
                    int order = i + 1;

                    using var cmd = new SqlCommand(@"
                        UPDATE [dbo].[FormQuestions] 
                        SET [DisplayOrder] = @DisplayOrder 
                        WHERE [FormId] = @FormId AND [QuestionId] = @QuestionId", connection, transaction);
                    
                    cmd.Parameters.AddWithValue("@DisplayOrder", order);
                    cmd.Parameters.AddWithValue("@FormId", formId);
                    cmd.Parameters.AddWithValue("@QuestionId", qId);
                    cmd.ExecuteNonQuery();
                }

                transaction.Commit();
                return true;
            }
            catch
            {
                transaction.Rollback();
                throw;
            }
        }

        public bool MoveQuestion(int formId, int questionId, string direction)
        {
            var questions = GetFormQuestions(formId).OrderBy(q => q.DisplayOrder).ToList();
            int index = questions.FindIndex(q => q.QuestionId == questionId);
            if (index < 0) return false;

            if (direction.Equals("up", StringComparison.OrdinalIgnoreCase) && index > 0)
            {
                var temp = questions[index];
                questions[index] = questions[index - 1];
                questions[index - 1] = temp;
            }
            else if (direction.Equals("down", StringComparison.OrdinalIgnoreCase) && index < questions.Count - 1)
            {
                var temp = questions[index];
                questions[index] = questions[index + 1];
                questions[index + 1] = temp;
            }
            else
            {
                return false;
            }

            return ReorderAllQuestions(formId, questions.Select(q => q.QuestionId).ToList());
        }

        public bool UpdateFormQuestionRequired(int formId, int questionId, bool isRequired)
        {
            using var connection = _dbHelper.CreateOpenConnection();
            using var command = new SqlCommand("UpdateFormQuestionRequired", connection);
            command.CommandType = CommandType.StoredProcedure;

            command.Parameters.AddWithValue("@FormId", formId);
            command.Parameters.AddWithValue("@QuestionId", questionId);
            command.Parameters.AddWithValue("@IsRequired", isRequired);

            return Convert.ToInt32(command.ExecuteScalar()) > 0;
        }

        public List<string> GetCategories()
        {
            var categories = new List<string>();
            using var connection = _dbHelper.CreateOpenConnection();

            try
            {
                using var command = new SqlCommand("GetDistinctFormCategories", connection);
                command.CommandType = CommandType.StoredProcedure;
                using var reader = command.ExecuteReader();
                while (reader.Read())
                {
                    categories.Add(reader.GetString(0));
                }
            }
            catch
            {
                // Fallback default categories
            }

            if (!categories.Contains("Academic")) categories.Add("Academic");
            if (!categories.Contains("Course Evaluation")) categories.Add("Course Evaluation");
            if (!categories.Contains("Faculty Feedback")) categories.Add("Faculty Feedback");
            if (!categories.Contains("Campus Facilities")) categories.Add("Campus Facilities");
            if (!categories.Contains("General Survey")) categories.Add("General Survey");

            return categories.Distinct().OrderBy(c => c).ToList();
        }
    }
}
