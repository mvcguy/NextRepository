using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data;
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

        public object QueryStore(Func<DataSchema> action, string sql, string connection)
        {
            var key = GetHash(sql, connection);
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
                    StoreKeys[key] = new[] { sql, connection };
                }

                if (!QueryTables.ContainsKey(key))
                {
                    QueryTables[key] = GetTableNames(dataSchema.SchemaTable).Select(x => string.Format("{0}_{1}", x, connection)).ToList();
                }
            }

            return data;
        }

        public void InvalidateCache(string sql, string connection)
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

        private int GetHash(params string[] keys)
        {
            var keyStr = string.Empty;

            foreach (var key in keys)
            {
                keyStr = keyStr == string.Empty ? key : string.Format("{0}_{1}", keyStr, key);
            }

            return keyStr.GetHashCode();
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
                    tableName = m.Groups["table"].Value.ToLower().Split('.').Last();
                }
            }

            var matchedEntries = QueryTables.Where(x => x.Value.Contains(string.Format("{0}_{1}", tableName, connection))).Select(x => x.Key);
            return matchedEntries;
        }

        public static List<string> GetTableNames(DataTable schemaTable)
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
            Store.Clear();
            StoreKeys.Clear();
            QueryTables.Clear();
        }
    }

    public class DataSchema
    {
        public object Data { get; set; }

        public DataTable SchemaTable { get; set; }
    }
}
