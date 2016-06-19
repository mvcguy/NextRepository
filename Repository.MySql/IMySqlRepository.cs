using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using MySql.Data.MySqlClient;
using NextRepository.Common;

namespace Repository.MySql
{
    public interface IMySqlRepository : IRepository<MySqlConnection, CommandType, SqlBulkCopyOptions, MySqlTransaction>
    {
        IMySqlDbContext SqlDbContext { get; }
        new IEnumerable<TEntity> Query<TEntity>(string sql, CommandType commandType = CommandType.Text, object paramValueCollection = null) where TEntity : new();
        
        new void BulkInsert<T>(string tableName, IList<T> records, SqlBulkCopyOptions bulkCopyOptions = SqlBulkCopyOptions.Default, int batchSize = 5000, int timeOut = 500,
            Func<MySqlConnection, MySqlTransaction, bool> preQueryOperation = null, Func<MySqlConnection, MySqlTransaction, bool> postQueryOperation = null);

        new int NonQuery(string sql, CommandType commandType = CommandType.Text, object paramCollection = null, bool useTransaction = true,
            Func<MySqlConnection, MySqlTransaction, bool> preQueryOperation = null, Func<MySqlConnection, MySqlTransaction, bool> postQueryOperation = null);
        
        new IEnumerable<object> ExecuteMultiQuery(string sql, CommandType commandType = CommandType.Text, object paramCollection = null, params Type[] types);

        new object ExecuteScaler(string sql, CommandType commandType=CommandType.Text, object paramCollection = null);
    }
}