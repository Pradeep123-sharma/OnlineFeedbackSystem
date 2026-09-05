using Microsoft.Data.SqlClient;
using OnlineFeedbackSystem.Web.Data;
using OnlineFeedbackSystem.Models.Entities;
using System.Data;

namespace OnlineFeedbackSystem.Repositories
{
    // The interface is what your controllers depend on, not the concrete
    // class. Same reason you'd code to an interface in Java: it lets you
    // swap the implementation (or mock it in a unit test) without touching
    // any calling code.
    public interface IRoleRepository
    {
        List<Roles> GetAll();
        Roles? GetById(int id);
    }

    public class RoleRepository : IRoleRepository
    {
        private readonly DbHelper _dbHelper;

        public RoleRepository(DbHelper dbHelper)
        {
            _dbHelper = dbHelper;
        }

        public List<Roles> GetAll()
        {
            var roles = new List<Roles>();

            // 'using' disposes the connection automatically when this block
            // ends — equivalent to Java's try-with-resources for a JDBC
            // Connection. Forgetting this is the #1 way people leak
            // connections in ADO.NET code.
            using var connection = _dbHelper.CreateOpenConnection();
            using var command = new SqlCommand("GetAllRoles", connection);
            command.CommandType = CommandType.StoredProcedure;


            // SqlDataReader is ADO.NET's version of JDBC's ResultSet.
            // reader.Read() advances one row at a time, just like rs.next().
            using var reader = command.ExecuteReader();

            while (reader.Read())
            {
                roles.Add(MapRole(reader));
            }

            return roles;
        }

        public Roles? GetById(int id)
        {
            using var connection = _dbHelper.CreateOpenConnection();
            using var command = new SqlCommand("GetRoleById", connection);
            command.CommandType = CommandType.StoredProcedure;

            command.Parameters.AddWithValue("@RoleId", id);

            using var reader = command.ExecuteReader();

            return reader.Read() ? MapRole(reader) : null;
        }

        // Centralizing the row -> object mapping in one private method
        // means you only write this logic once, even though multiple
        // query methods above return a Role.
        private static Roles MapRole(SqlDataReader reader)
        {
            return new Roles
            {
                RoleId = reader.GetInt32(reader.GetOrdinal("RoleId")),
                RoleName = reader.GetString(reader.GetOrdinal("RoleName"))
            };
        }
    }
}
