using System.Data;
using System.Data.SqlClient;
using MySql.Data.MySqlClient;
using NextRepository.Common;

namespace Repository.MySql
{
    public interface IMySqlRepository : IRepository<MySqlConnection, CommandType, SqlBulkCopyOptions, MySqlTransaction>
    {
        IMySqlDbContext SqlDbContext { get; }
    }
}