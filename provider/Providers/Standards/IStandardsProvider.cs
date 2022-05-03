namespace Datask.Providers.Standards;

public interface IStandardsProvider
{
    string CreateFullObjectName(string schemaName, string objectName);

    string GetDefaultSchemaName();

    bool IsValidObjectName(string objectName);
}
