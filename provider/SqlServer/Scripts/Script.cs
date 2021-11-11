using System.Reflection;
using System.Runtime.CompilerServices;

using CodeBits;

namespace Datask.Providers.SqlServer.Scripts;

public static class Script
{
    public static Task<string> GetTables() => GetScriptAsync();

    public static Task<string> GetAllTableColumns() => GetScriptAsync();

    public static Task<string> GetAllTableReferences() => GetScriptAsync();

    private static async Task<string> GetScriptAsync([CallerMemberName] string? methodName = null)
    {
        return await Assembly.GetExecutingAssembly()
            .LoadResourceAsStringAsync(typeof(Script), $"{methodName}.sql")
            .ConfigureAwait(false);
    }
}
