using Microsoft.Data.SqlClient;
using OnlineFeedbackSystem.Web.Data;
using OnlineFeedbackSystem.Models.Entities;
using System.Data;

namespace OnlineFeedbackSystem.Repositories
{
    public interface IUserRepository
    {
        List<Users> GetAll();
        Users? GetById(int id);
        Users? GetByEmail(string email);
        int Create(Users user);
        bool Update(Users user);
        bool Deactivate(int id);
    }

    public class UserRepository : IUserRepository
    {
        private readonly DbHelper _dbHelper;

        public UserRepository(DbHelper dbHelper)
        {
            _dbHelper = dbHelper;
        }

        public List<Users> GetAll()
        {
            var users = new List<Users>();

            using var connection = _dbHelper.CreateOpenConnection();
            using var command = new SqlCommand("GetAllUsers", connection);

            command.CommandType = CommandType.StoredProcedure;

            using SqlDataReader reader = command.ExecuteReader();

            while (reader.Read())
            {
                users.Add(MapUser(reader));
            }

            return users;
        }

        public Users? GetById(int id)
        {
            using var connection = _dbHelper.CreateOpenConnection();
            using var command = new SqlCommand("GetUserById", connection);

            command.CommandType = CommandType.StoredProcedure;

            command.Parameters.AddWithValue("@UserId", id);

            using SqlDataReader reader = command.ExecuteReader();

            return reader.Read() ? MapUser(reader) : null;
        }

        public Users? GetByEmail(string email)
        {
            using var connection = _dbHelper.CreateOpenConnection();
            using var command = new SqlCommand("GetUserByEmail", connection);

            command.CommandType = CommandType.StoredProcedure;

            command.Parameters.AddWithValue("@Email", email);

            using SqlDataReader reader = command.ExecuteReader();

            return reader.Read() ? MapUser(reader) : null;
        }

        public int Create(Users user)
        {
            using var connection = _dbHelper.CreateOpenConnection();
            using var command = new SqlCommand("CreateUser", connection);

            command.CommandType = CommandType.StoredProcedure;

            command.Parameters.AddWithValue("@FullName", user.FullName);
            command.Parameters.AddWithValue("@Email", user.Email);
            command.Parameters.AddWithValue("@PasswordHash", user.PasswordHash);
            command.Parameters.AddWithValue("@RoleId", user.RoleId);

            // DBNull.Value, not user.DepartmentId directly: AddWithValue
            // can't convert a C# null (from an int? that's empty) into
            // anything SQL Server accepts on its own — it has to be handed
            // DBNull.Value explicitly, or this throws for any user with no
            // department, e.g. a Super Admin.
            command.Parameters.AddWithValue("@DepartmentId", (object?)user.DepartmentId ?? DBNull.Value);

            command.Parameters.AddWithValue("@IsActive", user.IsActive);

            return Convert.ToInt32(command.ExecuteScalar());
        }

        public bool Update(Users user)
        {
            using var connection = _dbHelper.CreateOpenConnection();
            using var command = new SqlCommand("UpdateUser", connection);

            command.CommandType = CommandType.StoredProcedure;

            command.Parameters.AddWithValue("@UserId", user.UserId);
            command.Parameters.AddWithValue("@FullName", user.FullName);
            command.Parameters.AddWithValue("@Email", user.Email);
            command.Parameters.AddWithValue("@RoleId", user.RoleId);
            command.Parameters.AddWithValue("@DepartmentId", (object?)user.DepartmentId ?? DBNull.Value);
            command.Parameters.AddWithValue("@IsActive", user.IsActive);

            int rowsAffected = Convert.ToInt32(command.ExecuteScalar());

            return rowsAffected > 0;
        }

        public bool Deactivate(int id)
        {
            using var connection = _dbHelper.CreateOpenConnection();
            using var command = new SqlCommand("DeactivateUser", connection);

            command.CommandType = CommandType.StoredProcedure;

            command.Parameters.AddWithValue("@UserId", id);

            int rowsAffected = Convert.ToInt32(command.ExecuteScalar());

            return rowsAffected > 0;
        }

        private static Users MapUser(SqlDataReader reader)
        {
            int departmentIdOrdinal = reader.GetOrdinal("DepartmentId");

            return new Users
            {
                UserId = reader.GetInt32(reader.GetOrdinal("UserId")),

                FullName = reader.GetString(
                    reader.GetOrdinal("FullName")),

                Email = reader.GetString(
                    reader.GetOrdinal("Email")),

                PasswordHash = reader.GetString(
                    reader.GetOrdinal("PasswordHash")),

                RoleId = reader.GetInt32(
                    reader.GetOrdinal("RoleId")),

                // Was reader.GetInt32(...) with no null check — throws the
                // instant a row has NULL here, which any user with no
                // department assigned will. This is the fix: check
                // IsDBNull first, same pattern as Department's nullable
                // fields elsewhere in this project.
                DepartmentId = reader.IsDBNull(departmentIdOrdinal)
                    ? null
                    : reader.GetInt32(departmentIdOrdinal),

                IsActive = reader.GetBoolean(
                    reader.GetOrdinal("IsActive")),

                CreatedAt = reader.GetDateTime(
                    reader.GetOrdinal("CreatedAt"))
            };
        }
    }
}