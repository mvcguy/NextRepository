using System.Data;
using System.Data.SqlClient;

namespace NextDataLayer
{
    public interface IMsSqlDbContext: ISqlDbContext<SqlConnection, SqlCommand, CommandType, SqlBulkCopy, SqlBulkCopyOptions, SqlTransaction> { }
}