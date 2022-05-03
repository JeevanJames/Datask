using System.Reflection;

namespace Datask.Providers;

public static class ProviderFactory
{
    private static readonly IDictionary<Type, ConstructorInfo> _ctorCache = new Dictionary<Type, ConstructorInfo>();

    public static IProvider Create<TProvider>(string connectionString, string? databaseName = null)
        where TProvider : class, IProvider
    {
        return Create(typeof(TProvider), connectionString, databaseName);
    }

    public static IProvider Create(Type providerType, string connectionString, string? databaseName = null)
    {
        if (providerType is null)
            throw new ArgumentNullException(nameof(providerType));
        if (connectionString is null)
            throw new ArgumentNullException(nameof(connectionString));

        if (_ctorCache.TryGetValue(providerType, out ConstructorInfo cachedCtor))
            return (IProvider)cachedCtor.Invoke(new object?[] { connectionString, databaseName });

        if (!typeof(IProvider).IsAssignableFrom(providerType))
        {
            throw new ArgumentException(
                $"The specified provider type '{providerType}' does not implement '{typeof(IProvider)}'.",
                nameof(providerType));
        }

        ConstructorInfo? ctor = providerType.GetConstructor(new[] { typeof(string), typeof(string) });
        if (ctor is null)
        {
            string errorMessage =
                $"The specified provider type '{providerType}' does not have a constructor " +
                "with two parameters - connection string and database name.";
            throw new ArgumentException(errorMessage, nameof(providerType));
        }

        _ctorCache.Add(providerType, ctor);

        return (IProvider)ctor.Invoke(new object?[] { connectionString, databaseName });
    }
}
