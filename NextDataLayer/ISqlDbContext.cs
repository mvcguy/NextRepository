using System;
using System.Collections.Generic;

namespace NextDataLayer
{
    public interface ISqlDbContext<TDbConnection, out TDbCommand, in TCommandType, out TBulkCopy, in TBulkCopyOptions, TTransaction>
    {
        void BulkInsert<T>(string tableName, IList<T> records, TBulkCopyOptions bulkCopyOptions, int batchSize, int timeOut = 500,
            Func<TDbConnection, TTransaction, bool> preQueryOperation = null, Func<TDbConnection, TTransaction, bool> postQueryOperation = null);

        int NonQuery(string sql, TCommandType commandType, object paramCollection = null, bool useTransaction = true,
            Func<TDbConnection, TTransaction, bool> preQueryOperation = null, Func<TDbConnection,TTransaction, bool> postQueryOperation = null);

        IEnumerable<TEntity> ExecuteQuery<TEntity>(string sql, TCommandType commandType, object paramCollection = null) where TEntity : new();

        TDbConnection InitializeConnection();

        void OpenConnection(TDbConnection sqlConnection);

        TDbCommand GetSqlCommand(string sql, object paramCollection, TCommandType commandType, TDbConnection connection, TTransaction transaction);

        TBulkCopy GetBulkCopy(TDbConnection connection, TBulkCopyOptions bulkCopyOptions, TTransaction transaction, int batchSize, int timeOut, string tableName);
    }
}