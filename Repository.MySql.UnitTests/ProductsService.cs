using System;
using System.Collections.Generic;
using System.Data;

namespace Repository.MySql.UnitTests
{
    public class ProductsService
    {
        private readonly IMySqlRepository _repository;
        public ProductsService(IMySqlRepository repository)
        {
            _repository = repository;
        }

        public IEnumerable<Product> GetProducts(int id)
        {
            var param = new
            {
                Id = id
            };

            const string sql = "SELECT * FROM NextDataLayer.Products WHERE Id = @Id";

            return _repository.Query<Product>(sql, CommandType.Text, param);
        }

        public void InsertProduct(Product product)
        {
            using (var connection = _repository.SqlDbContext.InitializeConnection())
            {
                const string sql = "INSERT INTO NextDataLayer.Products (Name, Description) VALUES (@Name,@Description);SELECT LAST_INSERT_ID() AS id;";
                var cmd = _repository.SqlDbContext.GetSqlCommand(sql, product, CommandType.Text, connection, null);
                var id = (ulong) cmd.ExecuteScalar();
               product.Id = (int)id;
            }

        }
    }
}