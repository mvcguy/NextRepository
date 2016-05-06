using System;
using System.Collections.Generic;

namespace NextDataLayer
{
    public interface IRepository<TDbConnection, out TDbCommand, in TCommandType, out TBulkCopy, in TBulkCopyOptions, TTransaction>
    {
        ISqlDbContext<TDbConnection, TDbCommand, TCommandType, TBulkCopy, TBulkCopyOptions, TTransaction> SqlDbContext { get; }

        void BulkInsert<T>(string tableName, IList<T> records, TBulkCopyOptions bulkCopyOptions, int batchSize = 5000, int timeOut = 500,
            Func<TDbConnection, TTransaction, bool> preQueryOperation = null, Func<TDbConnection, TTransaction, bool> postQueryOperation = null);

        int NonQuery(string sql, TCommandType commandType, object paramCollection = null, bool useTransaction = true,
            Func<TDbConnection, TTransaction, bool> preQueryOperation = null, Func<TDbConnection, TTransaction, bool> postQueryOperation = null);

        IEnumerable<TEntity> Query<TEntity>(string sql, TCommandType commandType, object paramValueCollection = null) where TEntity : new();

    }
}