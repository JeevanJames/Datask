namespace Datask.Common.Utilities
{
    public abstract class Executor<TOptions, TStatusEvents>
        where TOptions : ExecutorOptions
        where TStatusEvents : Enum
    {
        protected Executor(TOptions options)
        {
            Options = options ?? throw new ArgumentNullException(nameof(options));
        }

        public TOptions Options { get; }

        public abstract Task ExecuteAsync();

        public event EventHandler<StatusEventArgs<TStatusEvents>>? OnStatus;

        protected void FireStatusEvent(TStatusEvents status, string message, object? metadata = null)
        {
            OnStatus?.Invoke(this, new StatusEventArgs<TStatusEvents>(status, message, metadata));
        }
    }
}

public abstract class ExecutorOptions
{
}
