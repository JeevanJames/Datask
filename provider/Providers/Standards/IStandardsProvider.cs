namespace Datask.Providers.Standards;

public interface IStandardsProvider
{
    string CreateFullObjectName(string schemaName, string objectName);

    string GetDefaultSchemaName();

    bool IsValidObjectName(string objectName);
}

public static class StandardsProviderExtensions
{
    public static string CreateFullObjectName(this IStandardsProvider provider, DbObjectName dbObjectName)
    {
        return provider.CreateFullObjectName(dbObjectName.Schema, dbObjectName.Name);
    }
}
