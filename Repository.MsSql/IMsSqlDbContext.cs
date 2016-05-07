using System.Data;
using System.Data.SqlClient;
using NextRepository.Common;

namespace Repository.MsSql
{
    public interface IMsSqlDbContext: ISqlDbContext<SqlConnection, SqlCommand, CommandType, SqlBulkCopy, SqlBulkCopyOptions, SqlTransaction> { }
}