using Microsoft.Data.SqlClient;
using OnlineFeedbackSystem.Models.Entities;
using OnlineFeedbackSystem.Web.Data;
using System.Data;

namespace OnlineFeedbackSystem.Repositories
{
    public class QuestionRepository : IQuestionRepository
    {
        private readonly DbHelper _dbHelper;

        public QuestionRepository(DbHelper dbHelper)
        {
            _dbHelper = dbHelper;
        }

        public List<Questions> GetAll(string? category = null, string? type = null, string? search = null)
        {
            var questions = new List<Questions>();
            var questionDict = new Dictionary<int, Questions>();

            using var connection = _dbHelper.CreateOpenConnection();

            // 1. Call sp_GetAllQuestions
            using (var command = new SqlCommand("GetAllQuestions", connection))
            {
                command.CommandType = CommandType.StoredProcedure;
                command.Parameters.AddWithValue("@Category", string.IsNullOrWhiteSpace(category) ? DBNull.Value : category.Trim());
                command.Parameters.AddWithValue("@Type", string.IsNullOrWhiteSpace(type) ? DBNull.Value : type.Trim());
                command.Parameters.AddWithValue("@Search", string.IsNullOrWhiteSpace(search) ? DBNull.Value : search.Trim());

                using var reader = command.ExecuteReader();
                while (reader.Read())
                {
                    var question = MapQuestion(reader);
                    questions.Add(question);
                    questionDict[question.Id] = question;
                }
            }

            // 2. Call sp_GetQuestionOptions to populate options
            if (questions.Count > 0)
            {
                using var optCmd = new SqlCommand("GetQuestionOptions", connection);
                optCmd.CommandType = CommandType.StoredProcedure;
                optCmd.Parameters.AddWithValue("@QuestionId", DBNull.Value);

                using var optReader = optCmd.ExecuteReader();
                while (optReader.Read())
                {
                    var opt = MapOption(optReader);
                    if (questionDict.TryGetValue(opt.QuestionId, out var q))
                    {
                        q.Options.Add(opt);
                    }
                }
            }

            return questions;
        }

        public Questions? GetById(int id)
        {
            using var connection = _dbHelper.CreateOpenConnection();
            Questions? question = null;

            // 1. Call sp_GetQuestionById
            using (var command = new SqlCommand("GetQuestionById", connection))
            {
                command.CommandType = CommandType.StoredProcedure;
                command.Parameters.AddWithValue("@Id", id);

                using var reader = command.ExecuteReader();
                if (reader.Read())
                {
                    question = MapQuestion(reader);
                }
            }

            // 2. Call sp_GetQuestionOptions for this specific question
            if (question != null)
            {
                using var optCmd = new SqlCommand("GetQuestionOptions", connection);
                optCmd.CommandType = CommandType.StoredProcedure;
                optCmd.Parameters.AddWithValue("@QuestionId", id);

                using var optReader = optCmd.ExecuteReader();
                while (optReader.Read())
                {
                    question.Options.Add(MapOption(optReader));
                }
            }

            return question;
        }

        public int Create(Questions question, List<QuestionOptions> options)
        {
            using var connection = _dbHelper.CreateOpenConnection();
            using var transaction = connection.BeginTransaction();

            try
            {
                int newId;

                // 1. Call sp_CreateQuestion
                using (var command = new SqlCommand("CreateQuestion", connection, transaction))
                {
                    command.CommandType = CommandType.StoredProcedure;
                    command.Parameters.AddWithValue("@QuestionText", question.QuestionText.Trim());
                    command.Parameters.AddWithValue("@QuestionType", question.QuestionType);
                    command.Parameters.AddWithValue("@Category", question.Category.Trim());
                    command.Parameters.AddWithValue("@IsActive", question.IsActive);

                    newId = Convert.ToInt32(command.ExecuteScalar());
                }

                // 2. Call sp_AddQuestionOption for each option
                if (options != null && options.Count > 0)
                {
                    for (int i = 0; i < options.Count; i++)
                    {
                        var opt = options[i];
                        if (string.IsNullOrWhiteSpace(opt.OptionText)) continue;

                        using var optCmd = new SqlCommand("AddQuestionOption", connection, transaction);
                        optCmd.CommandType = CommandType.StoredProcedure;
                        optCmd.Parameters.AddWithValue("@QuestionId", newId);
                        optCmd.Parameters.AddWithValue("@OptionText", opt.OptionText.Trim());
                        optCmd.Parameters.AddWithValue("@OptionValue", string.IsNullOrWhiteSpace(opt.OptionValue) ? opt.OptionText.Trim() : opt.OptionValue.Trim());
                        optCmd.Parameters.AddWithValue("@DisplayOrder", i + 1);
                        optCmd.ExecuteNonQuery();
                    }
                }

                transaction.Commit();
                return newId;
            }
            catch
            {
                transaction.Rollback();
                throw;
            }
        }

        public bool Update(Questions question, List<QuestionOptions> options)
        {
            using var connection = _dbHelper.CreateOpenConnection();
            using var transaction = connection.BeginTransaction();

            try
            {
                // 1. Call sp_UpdateQuestion
                using (var command = new SqlCommand("UpdateQuestion", connection, transaction))
                {
                    command.CommandType = CommandType.StoredProcedure;
                    command.Parameters.AddWithValue("@Id", question.Id);
                    command.Parameters.AddWithValue("@QuestionText", question.QuestionText.Trim());
                    command.Parameters.AddWithValue("@QuestionType", question.QuestionType);
                    command.Parameters.AddWithValue("@Category", question.Category.Trim());
                    command.Parameters.AddWithValue("@IsActive", question.IsActive);

                    command.ExecuteNonQuery();
                }

                // 2. Call sp_DeleteQuestionOptions to clear existing options
                using (var delCmd = new SqlCommand("DeleteQuestionOptions", connection, transaction))
                {
                    delCmd.CommandType = CommandType.StoredProcedure;
                    delCmd.Parameters.AddWithValue("@QuestionId", question.Id);
                    delCmd.ExecuteNonQuery();
                }

                // 3. Call sp_AddQuestionOption for each updated option
                if (options != null && options.Count > 0)
                {
                    for (int i = 0; i < options.Count; i++)
                    {
                        var opt = options[i];
                        if (string.IsNullOrWhiteSpace(opt.OptionText)) continue;

                        using var optCmd = new SqlCommand("AddQuestionOption", connection, transaction);
                        optCmd.CommandType = CommandType.StoredProcedure;
                        optCmd.Parameters.AddWithValue("@QuestionId", question.Id);
                        optCmd.Parameters.AddWithValue("@OptionText", opt.OptionText.Trim());
                        optCmd.Parameters.AddWithValue("@OptionValue", string.IsNullOrWhiteSpace(opt.OptionValue) ? opt.OptionText.Trim() : opt.OptionValue.Trim());
                        optCmd.Parameters.AddWithValue("@DisplayOrder", i + 1);
                        optCmd.ExecuteNonQuery();
                    }
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

        public bool ToggleStatus(int id)
        {
            using var connection = _dbHelper.CreateOpenConnection();
            using var command = new SqlCommand("ToggleQuestionStatus", connection);
            command.CommandType = CommandType.StoredProcedure;
            command.Parameters.AddWithValue("@Id", id);

            int rows = Convert.ToInt32(command.ExecuteScalar());
            return rows > 0;
        }

        public bool Delete(int id)
        {
            using var connection = _dbHelper.CreateOpenConnection();
            using var command = new SqlCommand("DeleteQuestion", connection);
            command.CommandType = CommandType.StoredProcedure;
            command.Parameters.AddWithValue("@Id", id);

            int result = Convert.ToInt32(command.ExecuteScalar());
            return result > 0;
        }

        public List<string> GetCategories()
        {
            var categories = new List<string>();
            using var connection = _dbHelper.CreateOpenConnection();

            try
            {
                using var command = new SqlCommand("GetDistinctQuestionCategories", connection);
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
            if (!categories.Contains("Facilities")) categories.Add("Facilities");
            if (!categories.Contains("Faculty")) categories.Add("Faculty");
            if (!categories.Contains("General")) categories.Add("General");
            if (!categories.Contains("Services")) categories.Add("Services");

            return categories.Distinct().OrderBy(c => c).ToList();
        }

        private static Questions MapQuestion(SqlDataReader reader)
        {
            return new Questions
            {
                Id = reader.GetInt32(reader.GetOrdinal("Id")),
                QuestionText = reader.GetString(reader.GetOrdinal("QuestionText")),
                QuestionType = reader.GetString(reader.GetOrdinal("QuestionType")),
                Category = reader.GetString(reader.GetOrdinal("Category")),
                IsActive = reader.GetBoolean(reader.GetOrdinal("IsActive"))
            };
        }

        private static QuestionOptions MapOption(SqlDataReader reader)
        {
            return new QuestionOptions
            {
                Id = reader.GetInt32(reader.GetOrdinal("Id")),
                QuestionId = reader.GetInt32(reader.GetOrdinal("QuestionId")),
                OptionText = reader.GetString(reader.GetOrdinal("OptionText")),
                OptionValue = reader.GetString(reader.GetOrdinal("OptionValue")),
                DisplayOrder = reader.GetInt32(reader.GetOrdinal("DisplayOrder"))
            };
        }
    }
}
