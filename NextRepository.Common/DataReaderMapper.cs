using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Diagnostics;
using System.Dynamic;
using System.Linq;
using System.Reflection;

namespace NextRepository.Common
{
    public class DataReaderMapper<T> where T : new()
    {
        private readonly IEnumerable<string> _columns;
        private readonly DataTable _schemaTable;
        private readonly IDictionary<int, string> _ordinalColumnMapping;
        private readonly List<PropertyInfo> _mappings;

        public DataReaderMapper(IEnumerable<string> columns, DataTable schemaTable)
        {
            _columns = columns;
            _schemaTable = schemaTable;
            this._mappings = Mappings();
            _ordinalColumnMapping = new Dictionary<int, string>();
            CreateOrinalColumnMapping();
        }

        private void CreateOrinalColumnMapping()
        {
            //foreach (DataColumn columnInfo in _schemaTable.Columns)
            //{
            //    var firstRow = _schemaTable.Rows[0];
            //    var tableName = firstRow["TableName"];
            //    var ordinal = (int)firstRow["Ordinal"];
            //    var columnName = firstRow["ColumnName"];

            //    if (_ordinalColumnMapping.ContainsKey(ordinal)) continue;

            //    _ordinalColumnMapping.Add(ordinal, string.Format("{0}_{1}", tableName, columnName));
            //}

            var zeroBasedIndex = false;

            foreach (DataRow myField in _schemaTable.Rows)
            {

                var tableName = myField["BaseTableName"];

                //the ordinal starts from 1
                var ordinal = (int)myField["ColumnOrdinal"];

                if (ordinal == 0)
                {
                    zeroBasedIndex = true;
                }

                if (!zeroBasedIndex)
                {
                    ordinal = ordinal - 1;
                }

                //var type = ordinal.GetType();
                var columnName = myField["BaseColumnName"];
                if (_ordinalColumnMapping.ContainsKey(ordinal)) continue;
                _ordinalColumnMapping.Add(ordinal, string.Format("{0}_{1}", tableName, columnName));

                //For each property of the field...
                //foreach (DataColumn myProperty in _schemaTable.Columns)
                //{
                //    //Display the field name and value.
                //    var info = myProperty.ColumnName + " = " + myField[myProperty].ToString();
                //    Debug.WriteLine(info);

                //}

            }
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
            if (typeof(T) == typeof(object))
            {
                var item = new ExpandoObject() as IDictionary<string, Object>;

                foreach (var mapping in _ordinalColumnMapping)
                {
                    if (item.Keys.Contains(mapping.Value)) continue;
                    item.Add(mapping.Value, record[mapping.Key]);
                }
                return (T)item;
            }

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