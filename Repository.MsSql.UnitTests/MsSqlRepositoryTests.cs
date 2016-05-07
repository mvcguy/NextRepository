﻿using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Repository.MsSql.UnitTests.Properties;

namespace Repository.MsSql.UnitTests
{
    [TestClass]
    public class MsSqlRepositoryTests
    {
        private static IMsSqlRepository _repository;

        [AssemblyInitialize]
        public static void OnTestClassInit(TestContext context)
        {
            _repository = new MsSqlRepository(ConfigurationManager.ConnectionStrings["Default"].ConnectionString);
            InitializeDatabase();
        }

        [TestMethod]
        public void Query_With_Params()
        {
            var productsService = new ProductsService(_repository);
            var product = productsService.GetProducts(id: 1).FirstOrDefault();
            Assert.IsNotNull(product);
            Assert.AreEqual(1, product.Id);
        }

        [TestMethod]
        public void NonQuery_With_Params()
        {
            CleanProducts("Milk");
            var product = new Product { Name = "Milk", Description = "1 Litre Pack" };
            var productsService = new ProductsService(_repository);
            productsService.InsertProduct(product);

            Assert.IsTrue(product.Id > 0);
        }

        [TestMethod]
        public void Query_Sp_With_Params()
        {
            var param = new {Name = "%Galaxy%"};
            var producs = _repository.Query<Product>("NextDataLayer.dbo.GetProducts", CommandType.StoredProcedure, param);

            Assert.IsTrue(producs.Any());
        }

        [TestMethod]
        public void BulkInsert_With_Pre_Post_Operations()
        {
            var products = new List<Product>();
            for (var i = 0; i < 10; i++)
            {
                var product = new Product() { Name = string.Format("Name-{0}", i), Description = string.Format("Description-{0}", i) };
                products.Add(product);
            }

            Func<SqlConnection, SqlTransaction, bool> preOperation = (connection, transaction) =>
             {
                 //clean 
                 CleanProducts("%Name-%");
                 CleanProductsLog();

                 if (InsertProductLog(connection, transaction, "Inserting products Name-1 to Name-9") <= 0)
                 {
                     throw new Exception("Pre-operation failed. Products log cannot be inserted.");
                 }

                 return true;
             };

            Func<SqlConnection, SqlTransaction, bool> postOperation = (connection, transaction) =>
             {
                 if (InsertProductLog(connection, transaction, "products Name-1 to Name-9 were Inserted successfully!") <= 0)
                 {
                     throw new Exception("Post Operation Failed: Products log cannot be inserted.");
                 }

                 return true;
             };

            //will bulk insert only if the pre & post-operation succeeds
            _repository.BulkInsert("[NextDataLayer].[dbo].[Products]", products, SqlBulkCopyOptions.Default, preQueryOperation: preOperation, postQueryOperation: postOperation);

            Assert.AreEqual(2, GetProductsLog().Count());
        }

        #region helpers

        private static void InitializeDatabase()
        {
            var dbSql = Resources.CreateDatabase;

            var tableSql = Resources.CreateTables;
            var seedTables = Resources.SeedDatabase;
            _repository.NonQuery(dbSql, CommandType.Text, useTransaction: false);
            _repository.NonQuery(tableSql, CommandType.Text, useTransaction: false);
            _repository.NonQuery(seedTables, CommandType.Text, useTransaction: false);

        }

        private IEnumerable<ProductsLog> GetProductsLog()
        {
            const string sql = "SELECT * FROM [NextDataLayer].[dbo].[ProductsLog]";
            return _repository.Query<ProductsLog>(sql, CommandType.Text);
        }
        private void CleanProducts(string query)
        {
            var deleteParam = new { Name = query };
            const string deleteSql = "DELETE FROM [NextDataLayer].[dbo].[Products] WHERE Name LIKE @Name";
            _repository.NonQuery(deleteSql, CommandType.Text, deleteParam);
        }

        private void CleanProductsLog()
        {
            const string deleteSql = "DELETE FROM [NextDataLayer].[dbo].[ProductsLog]";
            _repository.NonQuery(deleteSql, CommandType.Text);
        }

        private int InsertProductLog(SqlConnection connection, SqlTransaction transaction, string message)
        {
            var insertParams = new { Message = message, LastUpdated = DateTime.Now };
            const string insertProductLog = "INSERT INTO [NextDataLayer].[dbo].[ProductsLog] (Message, LastUpdated) VALUES (@Message, @LastUpdated)";
            var cmd = _repository.SqlDbContext.GetSqlCommand(insertProductLog, insertParams, CommandType.Text, connection, transaction);
            return cmd.ExecuteNonQuery();
        }

        #endregion
    }
}