using Datask.Providers.Standards;

using Npgsql;

namespace Datask.Providers.PostgreSql;

public sealed class PostgreSqlStandardsProvider : StandardsProvider<NpgsqlConnection>
{
    public PostgreSqlStandardsProvider(NpgsqlConnection connection)
        : base(connection)
    {
    }

    public override string CreateFullObjectName(string schemaName, string objectName)
    {
        return $@"""{schemaName}"".""{objectName}""";
    }

    public override string GetDefaultSchemaName()
    {
        return "public";
    }
}
