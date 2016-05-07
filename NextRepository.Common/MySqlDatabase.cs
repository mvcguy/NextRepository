using System.Data;
using MySql.Data.MySqlClient;

namespace NextRepository.Common
{
    public class MySqlDatabase : Database
    {
        public override IDbConnection GetConnection(string connectionString)
        {
            return new MySqlConnection(connectionString);
        }
    }
}