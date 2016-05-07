using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Threading;
using MySql.Data.MySqlClient;
using NextRepository.Common;

namespace Repository.MySql
{
    public class MySqlDbContext : IMySqlDbContext
    {
        private readonly string _connectionString;
        private readonly int _commandTimeout;

        public MySqlDbContext(string connectionString, int commandTimeout = 30)
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
                    using (var reader = command.ExecuteReader())
                    {
                        var columns = Enumerable.Range(0, reader.FieldCount).Select(reader.GetName).ToList();
                        var mapper = new DataReaderMapper<TEntity>(columns);
                        while (reader.Read())
                            yield return mapper.MapFrom(reader);
                    }
                }
            }
        }

        public virtual MySqlConnection InitializeConnection()
        {
            var connection = DatabaseFactory.CreateDatabaseConnection(typeof(MySqlDatabase)).GetConnection(_connectionString) as MySqlConnection;
            OpenConnection(connection);
            return connection;
        }

        public virtual void OpenConnection(MySqlConnection sqlConnection)
        {
            if (sqlConnection == null) throw new Exception("Connection is null");
            var log = new TraceLog();
            log.Info("OpenConnection. Thread Id: " + Thread.CurrentThread.ManagedThreadId);
            sqlConnection.Open();
        }

        public virtual int NonQuery(string sql, CommandType commandType = CommandType.Text, object paramCollection = null, bool useTransaction = true,
            Func<MySqlConnection, MySqlTransaction, bool> preQueryOperation = null, Func<MySqlConnection, MySqlTransaction, bool> postQueryOperation = null)
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

        public virtual IEnumerable<IList<T>> Split<T>(List<T> input, int chunkSize)
        {
            return Enumerable.Range(0, (input.Count - 1) / chunkSize + 1)
                     .Select(i => input.GetRange(i * chunkSize, Math.Min(chunkSize, input.Count - i * chunkSize)))
                     .ToList();
        }

        public virtual void BulkInsert<T>(string tableName, IList<T> records, SqlBulkCopyOptions bulkCopyOptions = SqlBulkCopyOptions.Default, int batchSize = 5000, int timeOut = 500,
            Func<MySqlConnection, MySqlTransaction, bool> preQueryOperation = null, Func<MySqlConnection, MySqlTransaction, bool> postQueryOperation = null)
        {
            if (batchSize <= 0 || batchSize > records.Count)
            {
                batchSize = records.Count;
            }

            using (var connection = InitializeConnection())
            {
                var bulkCopyTimeout = _commandTimeout < timeOut
                                ? timeOut
                                : _commandTimeout;

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

                        var bulkCopy = GetBulkCopy(connection, SqlBulkCopyOptions.Default, transaction, batchSize, bulkCopyTimeout, tableName);

                        var batches = Split((List<T>)records, batchSize);

                        foreach (var batch in batches)
                        {
                            WriteInBulk(batch, bulkCopy);
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

        public virtual void WriteInBulk<T>(IList<T> batch, MySqlBulkLoader bulkCopy)
        {
            string csvFileName = string.Format("{0}-dump.csv", Guid.NewGuid());
            CreateTemporaryCsvFile(batch, csvFileName);
            bulkCopy.FileName = csvFileName;
            bulkCopy.Load();
            File.Delete(csvFileName);
        }

        public virtual void CreateTemporaryCsvFile<T>(IList<T> records, string tempCsvFileName)
        {
            //write in chunks of 5000 records
            const int batchSize = 5000;
            if (records.Count > batchSize)
            {
                var batches = Split((List<T>)records, batchSize);
                foreach (var batch in batches)
                {
                    var sqlTable = batch.ToDataTable();
                    using (var writer = new StreamWriter(tempCsvFileName, true))
                    {
                        Rfc4180Writer.WriteDataTable(sqlTable, writer, false);
                    }
                }
            }

            else
            {
                var sqlTable = records.ToDataTable();
                using (var writer = new StreamWriter(tempCsvFileName, false))
                {
                    Rfc4180Writer.WriteDataTable(sqlTable, writer, false);
                }
            }
        }

        public virtual MySqlCommand GetSqlCommand(string sql, object paramCollection, CommandType commandType, MySqlConnection connection, MySqlTransaction transaction = null)
        {
            var cmd = new MySqlCommand();

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

                else if (paramCollection.GetType() == typeof(IEnumerable<MySqlParameter>))
                {
                    var sqlParams = paramCollection as IEnumerable<MySqlParameter>;
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
                        var paramater = new MySqlParameter(pInfo.Name, pInfo.GetValue(paramCollection, null) ?? DBNull.Value);
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

        public virtual MySqlBulkLoader GetBulkCopy(MySqlConnection connection, SqlBulkCopyOptions bulkCopyOptions,
            MySqlTransaction transaction, int batchSize, int timeOut, string tableName)
        {

            //references:
            //http://stackoverflow.com/questions/30615443/bulk-copy-a-datatable-into-mysql-similar-to-system-data-sqlclient-sqlbulkcopy
            //http://stackoverflow.com/questions/25323560/most-efficient-way-to-insert-rows-into-mysql-database
            var sqlBulkLoader = new MySqlBulkLoader(connection)
            {
                Timeout = timeOut,
                TableName = tableName,
                FieldTerminator = ",",
                LineTerminator = "\r\n",
                FieldQuotationCharacter = '"',
            };

            return sqlBulkLoader;
        }
    }

    public static class Rfc4180Writer
    {
        public static void WriteDataTable(DataTable sourceTable, TextWriter writer, bool includeHeaders)
        {
            if (includeHeaders)
            {
                IEnumerable<String> headerValues = sourceTable.Columns
                    .OfType<DataColumn>()
                    .Select(column => QuoteValue(column.ColumnName));

                writer.WriteLine(String.Join(",", headerValues));
            }

            IEnumerable<String> items = null;

            foreach (DataRow row in sourceTable.Rows)
            {
                items = row.ItemArray.Select(o => QuoteValue(o.ToString()));
                writer.WriteLine(String.Join(",", items));
            }

            writer.Flush();
        }

        private static string QuoteValue(string value)
        {
            return String.Concat("\"",
            value.Replace("\"", "\"\""), "\"");
        }
    }
}