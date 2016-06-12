using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Dynamic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NextRepository.Common;
using Repository.MsSql;

namespace NextRepository.MemCache.UnitTests
{
    [TestClass]
    public class QueryCacheTests
    {
        private static readonly string ConnectionString = ConfigurationManager.ConnectionStrings["Default"].ConnectionString;

        [TestMethod]
        public void QueryStore_Should_Cache_QueryResult()
        {
            const string sql = "SELECT top 10 * FROM PRODUCTS WHERE Name like @Name";
            var param = new { Name = "%galaxy%" };
            var cache = new QueryCache();

            cache.CleanCache();
            var databaseHits = 0;

            Func<DataSchema> databaseOperation = () =>
            {
                var dataSchema = new DataSchema
                {
                    SchemaTable = GetFakeTable("products"),
                    Data = GetFakeObjects()
                };
                databaseHits++;
                return dataSchema;
            };

            //first time call should increment the databaseHits to 1
            var data = cache.QueryStore(databaseOperation, sql, ConnectionString, param) as IEnumerable<object>;
            Assert.IsNotNull(data);
            Assert.IsTrue(data.Any());
            Assert.IsTrue(databaseHits == 1);

            //subsequent calls should fetch the data from the cache only!
            data = cache.QueryStore(databaseOperation, sql, ConnectionString, param) as IEnumerable<object>;
            data = cache.QueryStore(databaseOperation, sql, ConnectionString, param) as IEnumerable<object>;

            Assert.IsNotNull(data);
            Assert.IsTrue(data.Any());

            //this proves that the func 'databaseOperation' is called only once
            Assert.IsTrue(databaseHits == 1);
        }

        [TestMethod]
        public void QueryStore_Should_Cache_Separate_QueryResult_For_Separate_Params()
        {
            const string sql = "SELECT top 10 * FROM PRODUCTS WHERE Name like @Name";
            var param = new { Name = "%galaxy%" };
            var cache = new QueryCache();

            cache.CleanCache();
            var databaseHits = 0;

            Func<DataSchema> databaseOperation = () =>
            {
                var dataSchema = new DataSchema
                {
                    SchemaTable = GetFakeTable("products"),
                    Data = GetFakeObjects()
                };
                databaseHits++;
                return dataSchema;
            };


            var data = cache.QueryStore(databaseOperation, sql, ConnectionString, param) as IEnumerable<object>;
            Assert.IsNotNull(data);
            Assert.IsTrue(data.Any());
            Assert.IsTrue(databaseHits == 1);

            data = cache.QueryStore(databaseOperation, sql, ConnectionString, new { Name = "%sony%" }) as IEnumerable<object>;

            Assert.IsNotNull(data);
            Assert.IsTrue(data.Any());
            Assert.IsTrue(databaseHits == 2);


            //subsequent calls should not invoke the db operation, rather the data is served from cache
            data = cache.QueryStore(databaseOperation, sql, ConnectionString, param) as IEnumerable<object>;
            Assert.IsNotNull(data);
            Assert.IsTrue(data.Any());
            Assert.IsTrue(databaseHits == 2);

            data = cache.QueryStore(databaseOperation, sql, ConnectionString, new { Name = "%sony%" }) as IEnumerable<object>;

            Assert.IsNotNull(data);
            Assert.IsTrue(data.Any());
            Assert.IsTrue(databaseHits == 2);

        }

        [TestMethod]
        public void QueryStore_Should_Allow_Concurrent_Tasks()
        {
            var tasks = new List<Task>();
            var cache=new QueryCache();
            for (var i = 0; i < 500; i++)
            {
                var task = new Task(() =>
                {
                    const string sql = "SELECT top 10 * FROM PRODUCTS WHERE Name like @Name";
                    var param = new { Name = "%galaxy%" };


                    Func<DataSchema> databaseOperation = () =>
                    {
                        var dataSchema = new DataSchema
                        {
                            SchemaTable = GetFakeTable("products"),
                            Data = GetFakeObjects()
                        };
                        return dataSchema;
                    };

                    var data = cache.QueryStore(databaseOperation, sql, ConnectionString, param) as IEnumerable<object>;
                    Assert.IsNotNull(data);
                    Assert.IsTrue(data.Any());
                });
                task.Start();
                tasks.Add(task);
            }

            Task.WaitAll(tasks.ToArray());

        }

        [TestMethod]
        public void InvalidateCache_Should_Invalidate_Cache_When_DML_Query_Is_Executed()
        {
            const string sql = "SELECT top 10 * FROM PRODUCTS WHERE Name like @Name";
            var param = new { Name = "%galaxy%" };
            var cache = new QueryCache();

            cache.CleanCache();
            var databaseHits = 0;

            Func<DataSchema> databaseOperation = () =>
            {
                var dataSchema = new DataSchema
                {
                    SchemaTable = GetFakeTable("products"),
                    Data = GetFakeObjects()
                };
                databaseHits++;
                return dataSchema;
            };

            //first time call should increment the databaseHits to 1
            var data = cache.QueryStore(databaseOperation, sql, ConnectionString, param) as IEnumerable<object>;
            Assert.IsNotNull(data);
            Assert.IsTrue(data.Any());
            Assert.IsTrue(databaseHits == 1);

            //lets assume we have executed a DML query against this table, next we call the following method.
            //this method would drop all caches that has table name Products included in them.
            cache.InvalidateCache("DELETE FROM PRODUCTS WHERE ID = 10", ConnectionString);

            //now if we execute cache.QueryStore, the databaseHits should be incremented to 2
            data = cache.QueryStore(databaseOperation, sql, ConnectionString) as IEnumerable<object>;

            Assert.IsNotNull(data);
            Assert.IsTrue(data.Any());
            Assert.IsTrue(databaseHits == 2);

            //execute another dml query
            cache.InvalidateCache("UPDATE PRODUCTS SET NAME='Apples' WHERE ID = 10", ConnectionString);

            //now if we execute cache.QueryStore, the databaseHits should be incremented to 3
            data = cache.QueryStore(databaseOperation, sql, ConnectionString, param) as IEnumerable<object>;

            Assert.IsNotNull(data);
            Assert.IsTrue(data.Any());
            Assert.IsTrue(databaseHits == 3);

            //execute another dml query
            cache.InvalidateCache("INSERT INTO Products ([Name],[Description]) values ('Galaxy S6', '3GB RAM 32 Internal Storage')", ConnectionString);

            //now if we execute cache.QueryStore, the databaseHits should be incremented to 4
            data = cache.QueryStore(databaseOperation, sql, ConnectionString, param) as IEnumerable<object>;

            Assert.IsNotNull(data);
            Assert.IsTrue(data.Any());
            Assert.IsTrue(databaseHits == 4);

        }

        private DataTable GetFakeTable(string tableName)
        {
            var table = new DataTable(tableName);
            table.Columns.Add("BaseTableName");
            var row = table.NewRow();
            row["BaseTableName"] = tableName;
            table.Rows.Add(row);
            return table;
        }

        private IEnumerable<object> GetFakeObjects()
        {
            var list = new List<dynamic>();

            for (int i = 0; i < 20; i++)
            {
                var item = new ExpandoObject() as IDictionary<string, object>;
                item.Add(string.Format("item{0}", i), i);
                list.Add(item);
            }

            return list;
        }
    }
}
