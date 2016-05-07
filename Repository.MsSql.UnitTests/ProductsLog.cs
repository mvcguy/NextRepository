using System;

namespace Repository.MsSql.UnitTests
{
    public class ProductsLog
    {
        public int Id { get; set; }

        public string Message { get; set; }

        public DateTime LastUpdated { get; set; }
    }
}