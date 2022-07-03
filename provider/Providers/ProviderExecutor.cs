using Datask.Common.Utilities;

namespace Datask.Providers;

public abstract class ProviderExecutor<TOptions, TStatusEvents> : Executor<TOptions, TStatusEvents>
    where TOptions : ProviderExecutorOptions
    where TStatusEvents : Enum
{
    protected ProviderExecutor(TOptions options)
        : base(options)
    {
        Provider = (IProvider)Activator.CreateInstance(options.ProviderType, options.ConnectionString, null);
    }

    protected IProvider Provider { get; }
}
