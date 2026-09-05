using Microsoft.Data.SqlClient;
using OnlineFeedbackSystem.Web.Data;
using OnlineFeedbackSystem.Models.Entities;
using System.Data;

namespace OnlineFeedbackSystem.Repositories
{
    public interface IDepartmentRepository
    {
        List<Departments> GetAll();
        Departments? GetById(int id);
        int Create(Departments department);
        bool Update(Departments department);
        bool Deactivate(int id);
        bool ToggleStatus(int id);
    }

    public class DepartmentRepository : IDepartmentRepository
    {
        private readonly DbHelper _dbHelper;

        public DepartmentRepository(DbHelper dbHelper)
        {
            _dbHelper = dbHelper;
        }

        public List<Departments> GetAll()
        {
            var departments = new List<Departments>();

            using var connection = _dbHelper.CreateOpenConnection();

            using var command = new SqlCommand(
                "GetAllDepartments",
                connection);

            command.CommandType = CommandType.StoredProcedure;

            using var reader = command.ExecuteReader();

            while (reader.Read())
            {
                departments.Add(MapDepartment(reader));
            }

            return departments;
        }

        public Departments? GetById(int id)
        {
            using var connection = _dbHelper.CreateOpenConnection();

            using var command = new SqlCommand(
                "GetDepartmentById",
                connection);

            command.CommandType = CommandType.StoredProcedure;

            command.Parameters.AddWithValue("@DepartmentId", id);

            using var reader = command.ExecuteReader();

            return reader.Read()
                ? MapDepartment(reader)
                : null;
        }

        public int Create(Departments department)
        {
            using var connection = _dbHelper.CreateOpenConnection();

            using var command = new SqlCommand(
                "CreateDepartment",
                connection);

            command.CommandType = CommandType.StoredProcedure;

            command.Parameters.AddWithValue(
                "@DepartmentName",
                department.DepartmentName);

            command.Parameters.AddWithValue(
                "@IsActive",
                department.IsActive);

            return Convert.ToInt32(command.ExecuteScalar());
        }

        public bool Update(Departments department)
        {
            using var connection = _dbHelper.CreateOpenConnection();

            using var command = new SqlCommand(
                "UpdateDepartment",
                connection);

            command.CommandType = CommandType.StoredProcedure;

            command.Parameters.AddWithValue(
                "@DepartmentId",
                department.DepartmentId);

            command.Parameters.AddWithValue(
                "@DepartmentName",
                department.DepartmentName);

            command.Parameters.AddWithValue(
                "@IsActive",
                department.IsActive);

            int rowsAffected = Convert.ToInt32(
                command.ExecuteScalar());

            return rowsAffected > 0;
        }

        public bool Deactivate(int id)
        {
            using var connection = _dbHelper.CreateOpenConnection();

            using var command = new SqlCommand(
                "DeactivateDepartment",
                connection);

            command.CommandType = CommandType.StoredProcedure;

            command.Parameters.AddWithValue(
                "@DepartmentId",
                id);

            int rowsAffected = Convert.ToInt32(
                command.ExecuteScalar());

            return rowsAffected > 0;
        }

        public bool ToggleStatus(int id)
        {
            var dept = GetById(id);
            if (dept == null) return false;

            dept.IsActive = !dept.IsActive;
            return Update(dept);
        }

        private static Departments MapDepartment(SqlDataReader reader)
        {
            return new Departments
            {
                DepartmentId = reader.GetInt32(
                    reader.GetOrdinal("DepartmentId")),

                DepartmentName = reader.GetString(
                    reader.GetOrdinal("DepartmentName")),

                IsActive = reader.GetBoolean(
                    reader.GetOrdinal("IsActive"))
            };
        }
    }
}
