using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Threading;
using NextRepository.Common;

namespace Repository.MsSql
{
    public class MsSqlDbContext : IMsSqlDbContext
    {
        private readonly string _connectionString;
        private readonly int _commandTimeout;

        public MsSqlDbContext(string connectionString, int commandTimeout = 30)
        {
            _connectionString = connectionString;
            _commandTimeout = commandTimeout;
        }

        public virtual IEnumerable<TEntity> ExecuteQuery<TEntity>(string sql, CommandType commandType = CommandType.Text, object paramCollection = null) where TEntity : new()
        {
            using (var connection = InitializeConnection())
            {
                using (var command = GetSqlCommand(sql, paramCollection, commandType, connection))
                {
                    using (var reader = command.ExecuteReader(CommandBehavior.KeyInfo))
                    {
                        var columns = Enumerable.Range(0, reader.FieldCount).Select(reader.GetName).ToList();
                        var mapper = new DataReaderMapper<TEntity>(columns,reader.GetSchemaTable());
                        while (reader.Read())
                            yield return mapper.MapFrom(reader);
                    }
                }
            }
        }

        public IEnumerable<object> ExecuteMultiQuery(string sql, CommandType commandType, object paramCollection = null, params Type[] types)
        {
            using (var connection = InitializeConnection())
            {
                using (var command = GetSqlCommand(sql, paramCollection, commandType, connection))
                {
                    using (var reader = command.ExecuteReader(CommandBehavior.KeyInfo))
                    {
                        var columns = Enumerable.Range(0, reader.FieldCount).Select(reader.GetName).ToList();
                        var mapper = new DataReaderMapper<object>(columns, reader.GetSchemaTable(), types);
                        while (reader.Read())
                            yield return mapper.MapFromMultpleTables(reader);
                    }
                }
            }
        }

        public virtual SqlConnection InitializeConnection()
        {
            var connection = DatabaseFactory.CreateDatabaseConnection(typeof(MsSqlDatabase)).GetConnection(_connectionString) as SqlConnection;
            OpenConnection(connection);
            return connection;
        }

        public virtual void OpenConnection(SqlConnection sqlConnection)
        {
            if (sqlConnection == null) throw new Exception("Connection is null");
            var log = new TraceLog();
            log.Info("OpenConnection. Thread Id: " + Thread.CurrentThread.ManagedThreadId);
            sqlConnection.Open();
        }

        public virtual int NonQuery(string sql, CommandType commandType = CommandType.Text, object paramCollection = null, bool useTransaction = true,
            Func<SqlConnection, SqlTransaction, bool> preQueryOperation = null, Func<SqlConnection, SqlTransaction, bool> postQueryOperation = null)
        {

            using (var connection = InitializeConnection())
            {

                if (!useTransaction)
                {
                    using (var cmd = GetSqlCommand(sql, paramCollection, commandType, connection))
                    {
                        return cmd.ExecuteNonQuery();
                    }
                }

                using (var trans = connection.BeginTransaction())
                {
                    using (var cmd = GetSqlCommand(sql, paramCollection, commandType, connection, trans))
                    {
                        try
                        {
                            if (preQueryOperation != null)
                            {
                                var isOkay = preQueryOperation.Invoke(connection, trans);
                                if (!isOkay)
                                {
                                    return -1;
                                }
                            }

                            var affectedRows = cmd.ExecuteNonQuery();

                            if (postQueryOperation != null)
                            {
                                var isOkay = postQueryOperation.Invoke(connection, trans);
                                if (!isOkay)
                                {
                                    trans.Rollback();
                                    return -1;
                                }
                            }

                            trans.Commit();
                            return affectedRows;
                        }
                        catch (Exception)
                        {
                            trans.Rollback();
                            throw;
                        }
                    }
                }
            }
        }

        public virtual void BulkInsert<T>(string tableName, IList<T> records, SqlBulkCopyOptions bulkCopyOptions = SqlBulkCopyOptions.Default, int batchSize = 5000, int timeOut = 500,
            Func<SqlConnection, SqlTransaction, bool> preQueryOperation = null, Func<SqlConnection, SqlTransaction, bool> postQueryOperation = null)
        {
            var sqlTable = records.ToDataTable();

            var bulkCopyTimeout = _commandTimeout < timeOut
                            ? timeOut
                            : _commandTimeout;

            using (var connection = InitializeConnection())
            {
                using (var transaction = connection.BeginTransaction())
                {
                    try
                    {
                        if (preQueryOperation != null)
                        {
                            var isOkay = preQueryOperation.Invoke(connection, transaction);
                            if (!isOkay)
                            {
                                return;
                            }
                        }
                        using (var bulkCopy = GetBulkCopy(connection, SqlBulkCopyOptions.Default, transaction, batchSize, bulkCopyTimeout, tableName))
                        {
                            bulkCopy.WriteToServer(sqlTable);
                        }
                        if (postQueryOperation != null)
                        {
                            var isOkay = postQueryOperation.Invoke(connection, transaction);
                            if (!isOkay)
                            {
                                transaction.Rollback();
                                return;
                            }
                        }

                        transaction.Commit();
                    }
                    catch (Exception)
                    {
                        transaction.Rollback();
                        throw;
                    }

                }
            }
        }

        public virtual SqlCommand GetSqlCommand(string sql, object paramCollection, CommandType commandType, SqlConnection connection, SqlTransaction transaction = null)
        {
            var cmd = new SqlCommand();

            if (paramCollection != null && !paramCollection.GetType().IsSimpleType())
            {
                if (paramCollection.GetType() == typeof(Dictionary<string, object>) || paramCollection.GetType() == typeof(IDictionary<string, object>))
                {
                    var dictionary = paramCollection as IDictionary<string, object>;
                    if (dictionary != null)
                    {
                        foreach (var item in dictionary)
                        {
                            var paramater = new SqlParameter(item.Key, item.Value ?? DBNull.Value);
                            cmd.Parameters.Add(paramater);
                        }
                    }
                }

                else if (paramCollection.GetType() == typeof(IEnumerable<SqlParameter>))
                {
                    var sqlParams = paramCollection as IEnumerable<SqlParameter>;
                    if (sqlParams != null)
                    {
                        foreach (var param in sqlParams)
                        {
                            cmd.Parameters.Add(param);
                        }
                    }

                }

                else
                {
                    foreach (var pInfo in paramCollection.GetType().GetProperties())
                    {
                        var paramater = new SqlParameter(pInfo.Name, pInfo.GetValue(paramCollection, null) ?? DBNull.Value);
                        cmd.Parameters.Add(paramater);
                    }
                }
            }
            if (transaction != null)
            {
                cmd.Transaction = transaction;
            }
            cmd.Connection = connection;
            cmd.CommandText = sql;
            cmd.CommandType = commandType;
            cmd.CommandTimeout = _commandTimeout;
            return cmd;
        }

        public virtual SqlBulkCopy GetBulkCopy(SqlConnection connection, SqlBulkCopyOptions bulkCopyOptions,
            SqlTransaction transaction, int batchSize, int timeOut, string tableName)
        {
            var sqlbulkcopy = new SqlBulkCopy(connection, bulkCopyOptions, transaction)
            {
                BatchSize = batchSize,
                BulkCopyTimeout = timeOut,
                DestinationTableName = tableName
            };
            return sqlbulkcopy;
        }
    }
}