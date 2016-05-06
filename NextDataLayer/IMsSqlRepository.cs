using System.Data;
using System.Data.SqlClient;

namespace NextDataLayer
{
    public interface IMsSqlRepository : IRepository<SqlConnection, CommandType, SqlBulkCopyOptions, SqlTransaction>
    {
        IMsSqlDbContext SqlDbContext { get; }
    }
}