using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using NextRepository.Common;

namespace NextRepository.WebSample.Models
{
    [TableName("Products")]
    public class Product
    {
        public int Id { get; set; }

        [DisplayName("Name")]
        [StringLength(20)]
        [MinLength(5)]
        [Required(AllowEmptyStrings = false)]
        public string Name { get; set; }

        [DisplayName("Description")]
        [StringLength(20)]
        [Required(AllowEmptyStrings = false)]
        [MinLength(5)]
        public string Description { get; set; }
    }
}