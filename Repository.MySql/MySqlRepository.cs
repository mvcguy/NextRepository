using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using MySql.Data.MySqlClient;

namespace Repository.MySql
{
    public class MySqlRepository : IMySqlRepository
    {
        public MySqlRepository(IMySqlDbContext sqlDbContext)
        {
            SqlDbContext = sqlDbContext;
        }

        public MySqlRepository(string connectionString, int commandTimeout = 30, bool useCache = false)
        {
            SqlDbContext = new MySqlDbContext(connectionString, commandTimeout, useCache);
        }

        public IEnumerable<TEntity> Query<TEntity>(string sql, CommandType commandType = CommandType.Text, object paramValueCollection = null) where TEntity : new()
        {
            return SqlDbContext.ExecuteQuery<TEntity>(sql, commandType, paramValueCollection);
        }

        public IEnumerable<object> ExecuteMultiQuery(string sql, CommandType commandType = CommandType.Text, object paramCollection = null, params Type[] types)
        {
            return SqlDbContext.ExecuteMultiQuery(sql, commandType, paramCollection, types);
        }

        public int NonQuery(string sql, CommandType commandType = CommandType.Text, object paramCollection = null, bool useTransaction = true,
            Func<MySqlConnection, MySqlTransaction, bool> preQueryOperation = null, Func<MySqlConnection, MySqlTransaction, bool> postQueryOperation = null)
        {
            return SqlDbContext.NonQuery(sql, commandType, paramCollection, useTransaction, preQueryOperation, postQueryOperation);
        }

        /// <summary>
        /// Bulk insert records into the specified table.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="tableName"></param>
        /// <param name="records"></param>
        /// <param name="sqlBulkCopyOptions"></param>
        /// <param name="batchSize"></param>
        /// <param name="timeOut"></param>
        /// <param name="preQueryOperation">Operation to perform before bulk insert in the same transaction</param>
        /// <param name="postQueryOperation">Operation to perform after bulk insert in the same trransaction</param>
        public void BulkInsert<T>(string tableName, IList<T> records, SqlBulkCopyOptions sqlBulkCopyOptions = SqlBulkCopyOptions.Default, int batchSize = 5000, int timeOut = 500,
            Func<MySqlConnection, MySqlTransaction, bool> preQueryOperation = null, Func<MySqlConnection, MySqlTransaction, bool> postQueryOperation = null)
        {
            SqlDbContext.BulkInsert(tableName, records, sqlBulkCopyOptions, batchSize, timeOut, preQueryOperation, postQueryOperation);
        }

        public object ExecuteScaler(string sql, CommandType commandType = CommandType.Text, object paramCollection = null)
        {
            return SqlDbContext.ExecuteScaler(sql, commandType, paramCollection);
        }

        public IMySqlDbContext SqlDbContext { get; }
    }
}