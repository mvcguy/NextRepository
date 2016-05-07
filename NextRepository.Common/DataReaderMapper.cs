using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Dynamic;
using System.Linq;
using System.Reflection;

namespace NextRepository.Common
{
    public class DataReaderMapper<T> where T : new()
    {
        private readonly Type[] _types;
        private readonly IEnumerable<string> _columns;
        private readonly DataTable _schemaTable;
        private readonly IDictionary<int, string> _ordinalColumnMapping;
        private readonly List<PropertyInfo> _mappings;
        private readonly IDictionary<string, List<PropertyInfo>> _tableAndTypeProperties;
        private readonly IDictionary<string, string> _tableAndTypeMapping;
        private readonly IDictionary<string, IList<string>> _tableAndColumns;


        public DataReaderMapper(IEnumerable<string> columns, DataTable schemaTable)
        {
            _columns = columns;
            _schemaTable = schemaTable;
            _mappings = Mappings(typeof(T));
            _ordinalColumnMapping = new Dictionary<int, string>();
            _tableAndColumns = new Dictionary<string, IList<string>>();
            CreateOrinalColumnMapping();
        }

        public DataReaderMapper(IEnumerable<string> columns, DataTable schemaTable, params Type[] types) : this(columns, schemaTable)
        {
            _types = types;
            _tableAndTypeProperties = new Dictionary<string, List<PropertyInfo>>();
            _tableAndTypeMapping = new Dictionary<string, string>();
            InitilizeTableAndTypeProperties();
        }


        private void CreateOrinalColumnMapping()
        {
            //Reference
            //https://support.microsoft.com/en-us/kb/310107

            var zeroBasedIndex = false;

            foreach (DataRow myField in _schemaTable.Rows)
            {
                var tableNameRaw = myField["BaseTableName"];
                var ordinalRaw = myField["ColumnOrdinal"];
                var columnNameRaw = myField["BaseColumnName"];

                if (columnNameRaw is DBNull || tableNameRaw is DBNull || ordinalRaw is DBNull) continue;

                var tableName = (string) tableNameRaw;
                var ordinal = (int) ordinalRaw;
                var columnName = (string) columnNameRaw;

                if(string.IsNullOrWhiteSpace(tableName) || string.IsNullOrWhiteSpace(columnName) || ordinal<0) continue;

                if (ordinal == 0)
                {
                    zeroBasedIndex = true;
                }

                if (!zeroBasedIndex)
                {
                    ordinal = ordinal - 1;
                }

                if (_tableAndColumns.ContainsKey(tableName))
                {
                    _tableAndColumns[tableName].Add(columnName);
                }
                else
                {
                    _tableAndColumns.Add(tableName, new List<string>() { columnName });
                }

                if (_ordinalColumnMapping.ContainsKey(ordinal)) continue;
                _ordinalColumnMapping.Add(ordinal, string.Format("{0}_X1117_{1}", tableName, columnName));

                //For each property of the field...
                //foreach (DataColumn myProperty in _schemaTable.Columns)
                //{
                //    //Display the field name and value.
                //    var info = myProperty.ColumnName + " = " + myField[myProperty].ToString();
                //    Debug.WriteLine(info);

                //}

            }
        }

        List<PropertyInfo> Mappings(Type t)
        {
            var properties = t.GetProperties().Where(x => x.CanWrite && _columns.Contains(x.Name)).ToList();
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
                var item = new ExpandoObject() as IDictionary<string, object>;

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

        public TType MapFrom<TType>(DbDataReader record) where TType : new()
        {
            if (typeof(TType) == typeof(object))
            {
                var item = new ExpandoObject() as IDictionary<string, Object>;

                foreach (var mapping in _ordinalColumnMapping)
                {
                    if (item.Keys.Contains(mapping.Value)) continue;
                    item.Add(mapping.Value, record[mapping.Key]);
                }
                return (TType)item;
            }

            var element = new TType();
            foreach (var map in _mappings)
                map.SetValue(element, ChangeType(record[map.Name], map.PropertyType));

            return element;
        }

        public dynamic MapFromMultpleTables(DbDataReader record)
        {
            var result = new List<object>();

            var elements = MapFrom<dynamic>(record) as IDictionary<string, object>;
            if (elements != null)
            {
                foreach (var tableAndColumn in _tableAndColumns)
                {
                    var type = _types.FirstOrDefault(x => string.Equals(GetTableName(x), tableAndColumn.Key, StringComparison.CurrentCultureIgnoreCase));

                    var typeProperties = _tableAndTypeProperties.FirstOrDefault(x => string.Equals(x.Key, tableAndColumn.Key, StringComparison.CurrentCultureIgnoreCase)).Value;

                    if (type == null) continue;

                    var newInstance = Activator.CreateInstance(type);
                    foreach (var typeProperty in typeProperties)
                    {
                        var fieldLookup = string.Format("{0}_X1117_{1}", tableAndColumn.Key, typeProperty.Name);
                        var fieldValue = elements[fieldLookup];
                        typeProperty.SetValue(newInstance, ChangeType(fieldValue, typeProperty.PropertyType));
                    }
                    result.Add(newInstance);
                }
            }
            return result;
        }

        static object ChangeType(object value, Type targetType)
        {
            if (value == null || value == DBNull.Value)
                return null;

            return Convert.ChangeType(value, Nullable.GetUnderlyingType(targetType) ?? targetType);
        }

        private void InitilizeTableAndTypeProperties()
        {
            foreach (var type in _types)
            {
                var tableName = GetTableName(type);
                if (_tableAndTypeProperties.ContainsKey(tableName)) continue;
                _tableAndTypeProperties.Add(tableName, Mappings(type));
            }
        }

        private string GetTableName(Type type)
        {
            if (_tableAndTypeMapping.ContainsKey(type.Name)) return _tableAndTypeMapping[type.Name];

            var tableName = type.Name;
            var tableattr = type.GetCustomAttributes(true).SingleOrDefault(attr => attr.GetType().Name == "TableNameAttribute") as dynamic;
            if (tableattr != null)
            {
                tableName = tableattr.Name;
            }
            _tableAndTypeMapping[type.Name] = tableName;
            return tableName;
        }
    }
}