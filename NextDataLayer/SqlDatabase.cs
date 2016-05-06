using System.Data;
using System.Data.SqlClient;

namespace NextDataLayer
{
    public class SqlDatabase : Database
    {
        public override IDbConnection GetConnection(string connectionString)
        {
            return new SqlConnection(connectionString);
        }
    }
}