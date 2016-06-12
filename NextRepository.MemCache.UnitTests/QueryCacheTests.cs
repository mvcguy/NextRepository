using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NextRepository.Common;
using Repository.MsSql;

namespace NextRepository.MemCache.UnitTests
{
    [TestClass]
    public class QueryCacheTests
    {

        private QueryCache _cache=new QueryCache();
        private readonly string _connectionString = ConfigurationManager.ConnectionStrings["Default"].ConnectionString;

        [TestMethod]
        public void TestMethod1()
        {
            const string sql = "SELECT * FROM NextDataLayer.dbo.PRODUCTS";

            var data = Query(sql);
            //var data2 = Query(sql);

            _cache.InvalidateCache("     UPDATE      db.dbo.PRODUCTS SET NAME='SOMETHING'", _connectionString);

            //var data3 = Query(sql);

        }

        private IEnumerable<object> Query(string sql, CommandType commandType = CommandType.Text, object paramCollection = null)
        {
            Func<DataSchema> func = () =>
            {
                var dataSchema = new DataSchema();
                DataTable schemaTable;

                var dbContext = new MsSqlDbContext(_connectionString);
                var items=new List<object>();

                using (var connection = dbContext.InitializeConnection())
                {
                    using (var command = dbContext.GetSqlCommand(sql, paramCollection, commandType, connection))
                    {
                        using (var reader = command.ExecuteReader(CommandBehavior.KeyInfo))
                        {
                            schemaTable = reader.GetSchemaTable();

                            var columns = Enumerable.Range(0, reader.FieldCount).Select(reader.GetName).ToList();
                            var mapper = new DataReaderMapper<object>(columns, schemaTable);
                            while (reader.Read())
                                items.Add(mapper.MapFrom(reader));
                        }
                    }
                }

                dataSchema.SchemaTable = schemaTable;
                dataSchema.Data = items;

                return dataSchema;
            };

            return _cache.QueryStore(func,sql,_connectionString) as IEnumerable<object>;
        } 

    }
}
