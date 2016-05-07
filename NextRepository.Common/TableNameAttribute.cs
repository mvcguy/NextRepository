using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NextRepository.Common
{
    [AttributeUsage(AttributeTargets.Class)]
    public class TableNameAttribute : Attribute
    {
       
        public TableNameAttribute(string tableName)
        {
            Name = tableName;
        }
      
        public string Name { get; private set; }
       
    }
}
