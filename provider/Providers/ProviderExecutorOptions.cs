namespace Datask.Providers;

public abstract class ProviderExecutorOptions : ExecutorOptions
{
    protected ProviderExecutorOptions(Type providerType, string connectionString)
    {
        if (providerType is null)
            throw new ArgumentNullException(nameof(providerType));
        if (!typeof(IProvider).IsAssignableFrom(providerType))
            throw new ArgumentException($"Type {providerType} is not a valid DB provider.", nameof(providerType));

        ProviderType = providerType;
        ConnectionString = connectionString ?? throw new ArgumentNullException(nameof(connectionString));
    }

    public Type ProviderType { get; }

    public string ConnectionString { get; }
}
