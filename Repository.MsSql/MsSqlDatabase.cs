using System.Data;
using System.Data.SqlClient;
using NextRepository.Common;

namespace Repository.MsSql
{
    public class MsSqlDatabase : Database
    {
        public override IDbConnection GetConnection(string connectionString)
        {
            return new SqlConnection(connectionString);
        }
    }
}