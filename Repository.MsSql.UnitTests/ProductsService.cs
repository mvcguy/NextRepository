using System.Collections.Generic;
using System.Data;

namespace Repository.MsSql.UnitTests
{
    public class ProductsService
    {
        private readonly IMsSqlRepository _repository;
        public ProductsService(IMsSqlRepository repository)
        {
            _repository = repository;
        }

        public IEnumerable<Product> GetProducts(int id)
        {
            var param = new
            {
                Id = id
            };

            const string sql = "SELECT * FROM [NextDataLayer].[dbo].[Products] WHERE Id = @Id";

            return _repository.Query<Product>(sql, CommandType.Text, param);
        }

        public void InsertProduct(Product product)
        {
            using (var connection = _repository.SqlDbContext.InitializeConnection())
            {
                const string sql = "INSERT INTO [NextDataLayer].[dbo].[Products] (Name, Description) VALUES (@Name,@Description);SELECT CAST(SCOPE_IDENTITY() AS INT) AS [Id]";
                var cmd = _repository.SqlDbContext.GetSqlCommand(sql, product, CommandType.Text, connection, null);
                var id = cmd.ExecuteScalar();
                product.Id = (int)id;
            }

        }
    }
}