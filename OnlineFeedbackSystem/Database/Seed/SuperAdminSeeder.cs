using Microsoft.Data.SqlClient;
using OnlineFeedbackSystem.Models.Entities;
using OnlineFeedbackSystem.Services;
using OnlineFeedbackSystem.Web.Data;
using System.Data;

namespace OnlineFeedbackSystem.Database.Seed
{
    public class SuperAdminSeeder
    {
        private readonly IConfiguration _configuration;
        private readonly DbHelper _dbHelper;
        private readonly PasswordHasherService _passwordHasher;

        public SuperAdminSeeder(
            IConfiguration configuration,
            DbHelper dbHelper,
            PasswordHasherService passwordHasher)
        {
            _configuration = configuration;
            _dbHelper = dbHelper;
            _passwordHasher = passwordHasher;
        }

        public void Seed()
        {
            string fullName =
                _configuration["SuperAdmin:FullName"]!;

            string email =
                _configuration["SuperAdmin:Email"]!;

            string password =
                _configuration["SuperAdmin:Password"]!;

            bool isActive =
                bool.Parse(_configuration["SuperAdmin:IsActive"]!);

            // Create user object
            var user = new Users
            {
                FullName = fullName,
                Email = email,
                IsActive = isActive
            };

            // Hash the password before sending it to database
            string passwordHash =
                _passwordHasher.HashPassword(user, password);

            // Call CreateSuperAdmin stored procedure
            using var connection = _dbHelper.CreateOpenConnection();

            using var command = new SqlCommand(
                "CreateSuperAdmin",
                connection);

            command.CommandType = CommandType.StoredProcedure;

            command.Parameters.AddWithValue(
                "@FullName",
                fullName);

            command.Parameters.AddWithValue(
                "@Email",
                email);

            command.Parameters.AddWithValue(
                "@PasswordHash",
                passwordHash);

            command.Parameters.AddWithValue(
                "@IsActive",
                isActive);

            command.ExecuteNonQuery();
        }
    }
}
