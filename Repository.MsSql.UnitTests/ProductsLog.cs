using System;
using NextRepository.Common;

namespace Repository.MsSql.UnitTests
{
    [TableName("ProductsLog")]
    public class ProductsLog
    {
        public int Id { get; set; }

        public string Message { get; set; }

        public DateTime LastUpdated { get; set; }
    }
}