using Datask.Providers.Standards;

using Microsoft.Data.SqlClient;

namespace Datask.Providers.SqlServer;

public sealed class SqlServerStandardsProvider : StandardsProvider<SqlConnection>
{
    public SqlServerStandardsProvider(SqlConnection connection)
        : base(connection)
    {
    }

    public override string CreateFullObjectName(string schemaName, string objectName)
    {
        return $"[{schemaName}].[{objectName}]";
    }

    public override string GetDefaultSchemaName()
    {
        return "dbo";
    }
}
