using System.Data;

namespace Datask.Tool.ExcelData.Core
{
    internal static class TypeMappings
    {
        internal static (Type Type, DbType DbType) GetMappings(string dbType)
        {
            return _mappings.TryGetValue(dbType, out (Type Type, DbType DbType) mapping)
                ? mapping
                : (typeof(object), DbType.Object);
        }

        private static readonly Dictionary<string, (Type Type, DbType DbType)> _mappings = new(StringComparer.OrdinalIgnoreCase)
        {
            ["bigint"] = (typeof(long), DbType.Int64),
            ["binary"] = (typeof(byte[]), DbType.Binary),
            ["bit"] = (typeof(bool), DbType.Boolean),
            ["char"] = (typeof(string), DbType.AnsiStringFixedLength),
            ["date"] = (typeof(DateTime), DbType.Date),
            ["datetime"] = (typeof(DateTime), DbType.DateTime),
            ["datetime2"] = (typeof(DateTime), DbType.DateTime2),
            ["datetimeoffset"] = (typeof(DateTimeOffset), DbType.DateTimeOffset),
            ["decimal"] = (typeof(decimal), DbType.Decimal),
            ["float"] = (typeof(double), DbType.Double),
            ["image"] = (typeof(byte[]), DbType.Binary),
            ["int"] = (typeof(int), DbType.Int32),
            ["money"] = (typeof(decimal), DbType.Decimal),
            ["nchar"] = (typeof(string), DbType.StringFixedLength),
            ["ntext"] = (typeof(string), DbType.String),
            ["numeric"] = (typeof(decimal), DbType.Decimal),
            ["nvarchar"] = (typeof(string), DbType.String),
            ["real"] = (typeof(float), DbType.Single),
            ["rowversion"] = (typeof(byte[]), DbType.Binary),
            ["smalldatetime"] = (typeof(DateTime), DbType.DateTime),
            ["smallint"] = (typeof(short), DbType.Int16),
            ["smallmoney"] = (typeof(decimal), DbType.Decimal),
            ["sql_variant"] = (typeof(object), DbType.Object),
            ["text"] = (typeof(string), DbType.String),
            ["time"] = (typeof(TimeSpan), DbType.Time),
            ["timestamp"] = (typeof(byte[]), DbType.Binary),
            ["tinyint"] = (typeof(byte), DbType.Byte),
            ["uniqueidentifier"] = (typeof(Guid), DbType.Guid),
            ["varbinary"] = (typeof(byte[]), DbType.Binary),
            ["varchar"] = (typeof(string), DbType.AnsiString),
            ["xml"] = (typeof(string), DbType.Xml),
        };
    }
}
