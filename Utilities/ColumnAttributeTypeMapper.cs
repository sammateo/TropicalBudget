
using Dapper;
using System.ComponentModel.DataAnnotations.Schema;
using System.Reflection;

namespace TropicalBudget.Utilities
{
    public class ColumnAttributeTypeMapper<T> : SqlMapper.ITypeMap
    {
        private readonly SqlMapper.ITypeMap _defaultMap;

        public ColumnAttributeTypeMapper()
        {
            _defaultMap = new CustomPropertyTypeMap(
                typeof(T),
                (type, columnName) =>
                    type.GetProperties().FirstOrDefault(prop =>
                        prop.GetCustomAttributes(false)
                            .OfType<ColumnAttribute>()
                            .Any(attr => attr.Name.Equals(columnName, StringComparison.OrdinalIgnoreCase)))
                    ?? type.GetProperties().FirstOrDefault(prop =>
                        prop.Name.Equals(columnName, StringComparison.OrdinalIgnoreCase))
            );
        }

        public ConstructorInfo FindConstructor(string[] names, Type[] types) =>
            _defaultMap.FindConstructor(names, types);

        public ConstructorInfo FindExplicitConstructor() =>
            _defaultMap.FindExplicitConstructor();

        public SqlMapper.IMemberMap GetConstructorParameter(ConstructorInfo constructor, string columnName) =>
            _defaultMap.GetConstructorParameter(constructor, columnName);

        public SqlMapper.IMemberMap GetMember(string columnName) =>
            _defaultMap.GetMember(columnName);
    }
}
