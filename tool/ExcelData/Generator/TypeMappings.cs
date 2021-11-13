//using System;
//using System.Collections.Generic;

//namespace Datask.Tool.ExcelData.Generator
//{
//    internal static class TypeMappings
//    {
//        internal static (string CSharpAliasType, string DbType) GetMappings(string dbType)
//        {
//            return _mappings.TryGetValue(dbType, out (string CSharpAliasType, string DbType) mapping)
//                ? mapping
//                : ("object", "SqlDbType.VarChar");
//        }

//        private static readonly Dictionary<string, (string CSharpAliasType, string SqlDbType)> _mappings = new(StringComparer.OrdinalIgnoreCase)
//        {
//            ["bigint"] = ("long", "SqlDbType.BigInt"),
//            ["binary"] = ("byte[]", "SqlDbType.Binary"),
//            ["bit"] = ("bool", "SqlDbType.Bit"),
//            ["char"] = ("string", "SqlDbType.Char"),
//            ["date"] = ("DateTime", "SqlDbType.Date"),
//            ["datetime"] = ("DateTime", "SqlDbType.DateTime"),
//            ["datetime2"] = ("DateTime", "SqlDbType.DateTime2"),
//            ["datetimeoffset"] = ("DateTimeOffset", "SqlDbType.DateTimeOffset"),
//            ["decimal"] = ("decimal", "SqlDbType.Decimal"),
//            ["float"] = ("double", "SqlDbType.Float"),
//            ["image"] = ("byte[]", "SqlDbType.VarBinary"),
//            ["int"] = ("int", "SqlDbType.Int"),
//            ["money"] = ("decimal", "SqlDbType.Money"),
//            ["nchar"] = ("string", "SqlDbType.NChar"),
//            ["ntext"] = ("string", "SqlDbType.NText"),
//            ["numeric"] = ("decimal", "SqlDbType.Decimal"),
//            ["nvarchar"] = ("string", "SqlDbType.NVarChar"),
//            ["real"] = ("float", "SqlDbType.Real"),
//            ["rowversion"] = ("byte[]", "SqlDbType.Timestamp"),
//            ["smalldatetime"] = ("DateTime", "SqlDbType.SmallDateTime"),
//            ["smallint"] = ("short", "SqlDbType.SmallInt"),
//            ["smallmoney"] = ("decimal", "SqlDbType.SmallMoney"),
//            ["sql_variant"] = ("object", "SqlDbType.Variant"),
//            ["text"] = ("string", "SqlDbType.Text"),
//            ["time"] = ("TimeSpan", "SqlDbType.Time"),
//            ["timestamp"] = ("byte[]", "SqlDbType.Timestamp"),
//            ["tinyint"] = ("byte", "SqlDbType.TinyInt"),
//            ["uniqueidentifier"] = ("Guid", "SqlDbType.UniqueIdentifier"),
//            ["varbinary"] = ("byte[]", "SqlDbType.VarBinary"),
//            ["varchar"] = ("string", "SqlDbType.VarChar"),
//            ["xml"] = ("string", "SqlDbType.Xml"),
//        };
//    }
//}
