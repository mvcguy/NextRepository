using NextRepository.Common;

namespace NextRepository.WebSample.Models
{
    [TableName("Products")]
    public class Product
    {
        public int Id { get; set; }

        public string Name { get; set; }

        public string Description { get; set; }
    }
}