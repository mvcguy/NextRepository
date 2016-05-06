using System.Data;
using System.Data.SqlClient;

namespace NextDataLayer
{
    public interface IMsSqlRepository : IRepository<SqlConnection, SqlCommand, CommandType, SqlBulkCopy, SqlBulkCopyOptions, SqlTransaction>
    { }
}