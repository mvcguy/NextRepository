using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace NextRepository.MemCache
{
    public class QueryCache
    {
        private static readonly ConcurrentDictionary<int, object> Store = new ConcurrentDictionary<int, object>();

        private static readonly ConcurrentDictionary<int, string[]> StoreKeys = new ConcurrentDictionary<int, string[]>();

        private static readonly ConcurrentDictionary<int, List<string>> QueryTables = new ConcurrentDictionary<int, List<string>>();

        private static readonly object Sync = new object();

        private static readonly object Sync2 = new object();

        private static readonly object Sync3 = new object();

        public object QueryStore(Func<DataSchema> action, string sql, string connection, object paramCollection = null)
        {
            lock (Sync)
            {
                var normSql = NormalizedQuery(sql, paramCollection);

                var key = GetHash(normSql, connection);
                object data;

                if (Store.ContainsKey(key))
                {
                    data = Store[key];
                }
                else
                {
                    var dataSchema = action.Invoke();
                    Store[key] = dataSchema.Data;
                    data = dataSchema.Data;
                    if (!StoreKeys.ContainsKey(key))
                    {
                        StoreKeys[key] = new[] { normSql, connection };
                    }

                    if (!QueryTables.ContainsKey(key))
                    {
                        QueryTables[key] = GetTableNames(dataSchema.SchemaTable).Select(x => string.Format("{0}_{1}", x, connection)).ToList();
                    }
                }

                return data;
            }

        }

        public void InvalidateCache(string sql, string connection)
        {
            lock (Sync2)
            {
                var matchedEntries = GetMatchedEntries(sql, connection);
                foreach (var matchedEntry in matchedEntries)
                {
                    if (Store.ContainsKey(matchedEntry))
                    {
                        object data;
                        Store.TryRemove(matchedEntry, out data);
                    }
                }
            }

        }

        private int GetHash(params string[] keys)
        {
            var keyStr = string.Empty;

            foreach (var key in keys)
            {
                keyStr = keyStr == string.Empty ? key : string.Format("{0}_{1}", keyStr, key);
            }

            return keyStr.GetHashCode();
        }

        private string NormalizedQuery(string sql, object paramCollection = null)
        {

            if (paramCollection == null || paramCollection.GetType().IsSimpleType())
                return sql.ToLower();


            var normSql = sql.ToLower();

            var sqlParams = new List<SqlParameter>();

            if (paramCollection.GetType() == typeof(Dictionary<string, object>) || paramCollection.GetType() == typeof(IDictionary<string, object>))
            {
                var dictionary = paramCollection as IDictionary<string, object>;
                if (dictionary != null)
                {
                    foreach (var item in dictionary)
                    {
                        var paramater = new SqlParameter(item.Key, item.Value ?? DBNull.Value);
                        sqlParams.Add(paramater);
                    }
                }
            }

            else if (paramCollection.GetType() == typeof(IEnumerable<SqlParameter>))
            {
                sqlParams = paramCollection as List<SqlParameter>;
            }

            else
            {
                foreach (var pInfo in paramCollection.GetType().GetProperties())
                {
                    var paramater = new SqlParameter(pInfo.Name, pInfo.GetValue(paramCollection, null) ?? DBNull.Value);
                    sqlParams.Add(paramater);
                }
            }

            if (sqlParams != null)
            {
                foreach (var sqlParameter in sqlParams)
                {
                    var paramName = string.Format("@{0}", sqlParameter.ParameterName.ToLower());
                    normSql = normSql.Replace(paramName, sqlParameter.Value.GetHashCode().ToString());
                }
            }

            return normSql;
        }

        private IEnumerable<int> GetMatchedEntries(string query, string connection)
        {
            var normQuery = query.Trim();
            var dml = normQuery.StartsWith("insert into", StringComparison.OrdinalIgnoreCase)
                || normQuery.StartsWith("update", StringComparison.OrdinalIgnoreCase)
                || normQuery.StartsWith("delete from", StringComparison.OrdinalIgnoreCase);

            var tableName = string.Empty;

            if (dml)
            {
                var r = new Regex(@"(insert into|update|delete from)\s+(?<table>\S+)", RegexOptions.IgnoreCase);
                var m = r.Match(normQuery);
                if (m.Success)
                {
                    tableName = m.Groups["table"].Value.ToLower().Split('.').Last().Replace("[", "").Replace("]", "");
                }
            }

            var matchedEntries = QueryTables.Where(x => x.Value.Contains(string.Format("{0}_{1}", tableName, connection))).Select(x => x.Key);
            return matchedEntries;
        }

        private List<string> GetTableNames(DataTable schemaTable)
        {
            var tables = new List<string>();

            foreach (DataRow myField in schemaTable.Rows)
            {
                var tableNameRaw = myField["BaseTableName"];

                if (tableNameRaw is DBNull) continue;

                var tableName = (string)tableNameRaw;

                if (string.IsNullOrWhiteSpace(tableName)) continue;

                if (!tables.Contains(tableName.ToLower()))
                {
                    tables.Add(tableName.ToLower());
                }
            }

            return tables;
        }

        public void CleanCache()
        {
            lock (Sync3)
            {
                Store.Clear();
                StoreKeys.Clear();
                QueryTables.Clear();
            }

        }
    }

    public class DataSchema
    {
        public object Data { get; set; }

        public DataTable SchemaTable { get; set; }
    }

    internal static class Extensions
    {
        public static bool IsSimpleType(this Type type)
        {
            var underlyingType = Nullable.GetUnderlyingType(type);
            type = underlyingType ?? type;
            var simpleTypes = new List<Type>
                               {
                                   typeof(byte),
                                   typeof(sbyte),
                                   typeof(short),
                                   typeof(ushort),
                                   typeof(int),
                                   typeof(uint),
                                   typeof(long),
                                   typeof(ulong),
                                   typeof(float),
                                   typeof(double),
                                   typeof(decimal),
                                   typeof(bool),
                                   typeof(string),
                                   typeof(char),
                                   typeof(Guid),
                                   typeof(DateTime),
                                   typeof(DateTimeOffset),
                                   typeof(byte[])
                               };
            return simpleTypes.Contains(type) || type.IsEnum;
        }
    }
}
