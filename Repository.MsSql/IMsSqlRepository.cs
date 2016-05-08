using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using NextRepository.Common;

namespace Repository.MsSql
{
    public interface IMsSqlRepository : IRepository<SqlConnection, CommandType, SqlBulkCopyOptions, SqlTransaction>
    {
        IMsSqlDbContext SqlDbContext { get; }

        new void BulkInsert<T>(string tableName, IList<T> records, SqlBulkCopyOptions bulkCopyOptions=SqlBulkCopyOptions.Default, int batchSize = 5000, int timeOut = 500,
            Func<SqlConnection, SqlTransaction, bool> preQueryOperation = null, Func<SqlConnection, SqlTransaction, bool> postQueryOperation = null);

        new int NonQuery(string sql, CommandType commandType=CommandType.Text, object paramCollection = null, bool useTransaction = true,
            Func<SqlConnection, SqlTransaction, bool> preQueryOperation = null, Func<SqlConnection, SqlTransaction, bool> postQueryOperation = null);

        new IEnumerable<TEntity> Query<TEntity>(string sql, CommandType commandType=CommandType.Text, object paramValueCollection = null) where TEntity : new();

        new IEnumerable<object> ExecuteMultiQuery(string sql, CommandType commandType=CommandType.Text, object paramCollection = null, params Type[] types);

    }
}