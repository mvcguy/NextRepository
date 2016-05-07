using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Reflection;

namespace Repository.MySql
{
    public class DataReaderMapper<T> where T : new()
    {
        private readonly IEnumerable<string> _columns;

        private readonly List<PropertyInfo> _mappings;

        public DataReaderMapper(IEnumerable<string> columns)
        {
            _columns = columns;
            this._mappings = Mappings();
        }
        
        List<PropertyInfo> Mappings()
        {
            var properties = typeof(T).GetProperties().Where(x => x.CanWrite && _columns.Contains(x.Name)).ToList();
            CheckDuplicates(properties);
            return properties;
        }

        private static void CheckDuplicates(List<PropertyInfo> properties)
        {
            var duplicateExist = properties.Select(x => new { Name = x.Name.ToLower() }).Distinct().Count() != properties.Count();
            if (duplicateExist)
                throw new Exception("The model contains duplicate properties. Check for lower and upper case.");
        }

        public T MapFrom(DbDataReader record)
        {
            var element = new T();
            foreach (var map in _mappings)
                map.SetValue(element, ChangeType(record[map.Name], map.PropertyType));

            return element;
        }

        static object ChangeType(object value, Type targetType)
        {
            if (value == null || value == DBNull.Value)
                return null;

            return Convert.ChangeType(value, Nullable.GetUnderlyingType(targetType) ?? targetType);
        }


    }
}