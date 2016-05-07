using System.Data;
using System.Data.SqlClient;
using NextRepository.Common;

namespace Repository.MsSql
{
    public interface IMsSqlRepository : IRepository<SqlConnection, CommandType, SqlBulkCopyOptions, SqlTransaction>
    {
        IMsSqlDbContext SqlDbContext { get; }
    }
}