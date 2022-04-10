using Datask.Common.Utilities;

namespace Datask.Tool.ExcelData.Core.Bases;

public abstract class Executor<TOptions, TStatusEvents>
    where TOptions : ExecutorOptions
    where TStatusEvents : Enum
{
    protected Executor(TOptions options)
    {
        Options = options;
    }

    public abstract Task ExecuteAsync();

    public TOptions Options { get; }

    public event EventHandler<StatusEventArgs<TStatusEvents>>? OnStatus;

    protected void FireStatusEvent(TStatusEvents status, string message, object? metadata = null)
    {
        OnStatus?.Invoke(this, new StatusEventArgs<TStatusEvents>(status, message, metadata));
    }
}

public abstract class ExecutorOptions
{
}
