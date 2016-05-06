using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Reflection;

namespace NextDataLayer
{
    public class DataReaderMapper<T> where T : new()
    {
        private readonly DataTable _tableSchema;
        readonly List<PropertyInfo> _mappings;

        public DataReaderMapper()
        {
            this._mappings = Mappings();
        }

        // int part is column indices (ordinals)
        List<PropertyInfo> Mappings()
        {
            
            var properties = typeof(T).GetProperties().Where(x => x.CanWrite).ToList();
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