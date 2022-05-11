using Datask.Common.Utilities;

namespace Datask.Tool.ExcelData.Core.Generators;

public abstract class GeneratorBase<TGeneratorOptions, TStatusEvents> : Executor<TGeneratorOptions, TStatusEvents>
    where TGeneratorOptions : ExecutorOptions
    where TStatusEvents : Enum
{
    protected GeneratorBase(TGeneratorOptions options)
        : base(options)
    {
    }
}
