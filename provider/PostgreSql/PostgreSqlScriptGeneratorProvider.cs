using Datask.Providers.Scripts;

using Npgsql;

namespace Datask.Providers.PostgreSql
{
    public sealed class PostgreSqlScriptGeneratorProvider : ScriptGeneratorProvider<NpgsqlConnection>
    {
        public PostgreSqlScriptGeneratorProvider(NpgsqlConnection connection)
            : base(connection)
        {
        }
    }
}
