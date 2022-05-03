using System.Data;
using System.Text.RegularExpressions;

namespace Datask.Providers.Standards;

public abstract class StandardsProvider<TConnection> : SubProviderBase<TConnection>, IStandardsProvider
    where TConnection : IDbConnection
{
    protected StandardsProvider(TConnection connection)
        : base(connection)
    {
    }

    public abstract string CreateFullObjectName(string schemaName, string objectName);

    public abstract string GetDefaultSchemaName();

    public virtual bool IsValidObjectName(string objectName)
    {
        if (objectName is null)
            throw new ArgumentNullException(nameof(objectName));
        return StandardsProviderHelper.ValidObjectNamePattern.IsMatch(objectName);
    }
}

internal static class StandardsProviderHelper
{
    internal static readonly Regex ValidObjectNamePattern = new(@"^[a-zA-Z_][\w_]*",
        RegexOptions.Compiled | RegexOptions.ExplicitCapture, TimeSpan.FromSeconds(1));
}
