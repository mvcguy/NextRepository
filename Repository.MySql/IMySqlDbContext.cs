using System.Data;
using System.Data.SqlClient;
using MySql.Data.MySqlClient;
using NextRepository.Common;

namespace Repository.MySql
{
    public interface IMySqlDbContext: ISqlDbContext<MySqlConnection, MySqlCommand, CommandType, MySqlBulkLoader, SqlBulkCopyOptions, MySqlTransaction> { }
}