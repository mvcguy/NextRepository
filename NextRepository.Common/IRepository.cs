using System;
using System.Collections.Generic;

namespace NextRepository.Common
{
    public interface IRepository<out TDbConnection, in TCommandType, in TBulkCopyOptions, out TTransaction>
    {
        void BulkInsert<T>(string tableName, IList<T> records, TBulkCopyOptions bulkCopyOptions, int batchSize = 5000, int timeOut = 500,
            Func<TDbConnection, TTransaction, bool> preQueryOperation = null, Func<TDbConnection, TTransaction, bool> postQueryOperation = null);

        int NonQuery(string sql, TCommandType commandType, object paramCollection = null, bool useTransaction = true,
            Func<TDbConnection, TTransaction, bool> preQueryOperation = null, Func<TDbConnection, TTransaction, bool> postQueryOperation = null);

        IEnumerable<TEntity> Query<TEntity>(string sql, TCommandType commandType, object paramValueCollection = null) where TEntity : new();

        IEnumerable<object> ExecuteMultiQuery(string sql, TCommandType commandType, object paramCollection = null, params Type[] types);

        object ExecuteScaler(string sql, TCommandType commandType, object paramCollection = null);

    }
}