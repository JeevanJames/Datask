// Copyright (c) 2021 Jeevan James
// This file is licensed to you under the MIT License.
// See the LICENSE file in the project root for more information.
using System.Data;

namespace Datask.Providers.SqlServer;

public static class DbTypeMapping
{
    public static SqlDbType GetSqlDbTypeMapping(string dbType)
    {
        if (_mapping.TryGetValue(dbType, out SqlDbType sqlDbType))
            return sqlDbType;

        return SqlDbType.VarChar;
    }

    private static readonly Dictionary<string, SqlDbType> _mapping = new(StringComparer.OrdinalIgnoreCase)
    {
        ["bigint"] = SqlDbType.BigInt,
        ["binary"] = SqlDbType.Binary,
        ["bit"] = SqlDbType.Bit,
        ["char"] = SqlDbType.Char,
        ["date"] = SqlDbType.Date,
        ["datetime"] = SqlDbType.DateTime,
        ["datetime2"] = SqlDbType.DateTime2,
        ["datetimeoffset"] = SqlDbType.DateTimeOffset,
        ["decimal"] = SqlDbType.Decimal,
        ["float"] = SqlDbType.Float,
        ["image"] = SqlDbType.VarBinary,
        ["int"] = SqlDbType.Int,
        ["money"] = SqlDbType.Money,
        ["nchar"] = SqlDbType.NChar,
        ["ntext"] = SqlDbType.NText,
        ["numeric"] = SqlDbType.Decimal,
        ["nvarchar"] = SqlDbType.NVarChar,
        ["real"] = SqlDbType.Real,
        ["rowversion"] = SqlDbType.Timestamp,
        ["smalldatetime"] = SqlDbType.SmallDateTime,
        ["smallint"] = SqlDbType.SmallInt,
        ["smallmoney"] = SqlDbType.SmallMoney,
        ["sql_variant"] = SqlDbType.Variant,
        ["text"] = SqlDbType.Text,
        ["time"] = SqlDbType.Time,
        ["timestamp"] = SqlDbType.Timestamp,
        ["tinyint"] = SqlDbType.TinyInt,
        ["uniqueidentifier"] = SqlDbType.UniqueIdentifier,
        ["varbinary"] = SqlDbType.VarBinary,
        ["varchar"] = SqlDbType.VarChar,
        ["xml"] = SqlDbType.Xml,
    };
}
