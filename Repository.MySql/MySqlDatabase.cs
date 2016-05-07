using System.Data;
using MySql.Data.MySqlClient;
using NextRepository.Common;

namespace Repository.MySql
{
    public class MySqlDatabase : Database
    {
        public override IDbConnection GetConnection(string connectionString)
        {
            return new MySqlConnection(connectionString);
        }
    }
}