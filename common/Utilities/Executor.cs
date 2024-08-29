namespace Datask.Common.Utilities
{
    /// <summary>
    ///     Base class for performing any actions with support for options and sending status events.
    /// </summary>
    /// <typeparam name="TOptions">The type of options that apply to this class.</typeparam>
    /// <typeparam name="TStatusEvents">The type of status events that apply to this class.</typeparam>
    public abstract class Executor<TOptions, TStatusEvents>
        where TOptions : ExecutorOptions
        where TStatusEvents : Enum
    {
        protected Executor(TOptions options)
        {
            Options = options ?? throw new ArgumentNullException(nameof(options));
        }

        public TOptions Options { get; }

        /// <summary>
        ///     Executes the action.
        /// </summary>
        /// <returns>A <see cref="Task"/> that represents the asynchronous operation.</returns>
        public Task ExecuteAsync()
        {
            ValidationResult validationResult = Validate();
            if (validationResult.IsFailure)
                throw new InvalidOperationException(validationResult.ErrorMessage);
            return InternalExecuteAsync();
        }

        protected virtual ValidationResult Validate()
        {
            return ValidationResult.Success;
        }

        protected abstract Task InternalExecuteAsync();

        public event EventHandler<StatusEventArgs<TStatusEvents>>? OnStatus;

        protected void FireStatusEvent(TStatusEvents status, string message, object? metadata = null)
        {
            OnStatus?.Invoke(this, new StatusEventArgs<TStatusEvents>(status, message, metadata));
        }
    }
}

#pragma warning disable S2094 // Classes should not be empty
public abstract class ExecutorOptions;
#pragma warning restore S2094 // Classes should not be empty

public sealed class ValidationResult
{
    private ValidationResult()
    {
        IsSuccess = true;
        IsFailure = false;
        ErrorMessage = string.Empty;
    }

    private ValidationResult(string errorMessage)
    {
        if (string.IsNullOrEmpty(errorMessage))
            throw new ArgumentException($"'{nameof(errorMessage)}' cannot be null or empty.", nameof(errorMessage));

        IsSuccess = false;
        IsFailure = true;
        ErrorMessage = errorMessage;
    }

    public bool IsSuccess { get; }

    public bool IsFailure { get; }

    public string ErrorMessage { get; }

    public static readonly ValidationResult Success = new();

    public static ValidationResult Fail(string errorMessage)
    {
        return new ValidationResult(errorMessage);
    }
}
