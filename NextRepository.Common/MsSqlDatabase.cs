using System.Data;
using System.Data.SqlClient;
using MySql.Data.MySqlClient;

namespace NextRepository.Common
{
    public class MsSqlDatabase : Database
    {
        public override IDbConnection GetConnection(string connectionString)
        {
            return new SqlConnection(connectionString);
        }
    }

    public class MySqlDatabase : Database
    {
        public override IDbConnection GetConnection(string connectionString)
        {
            return new MySqlConnection(connectionString);
        }
    }

}