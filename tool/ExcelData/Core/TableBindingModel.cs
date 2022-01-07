using Datask.Providers.Schemas;

namespace Datask.Tool.ExcelData.Core
{
    public record TableBindingModel
    {
        public TableBindingModel(string name, string schema)
        {
            Name = name ?? throw new ArgumentNullException(nameof(name));
            Schema = schema ?? throw new ArgumentNullException(nameof(schema));
        }

        public string Name { get; }

        public string Schema { get; }

        public IList<ColumnBindingModel> Columns { get; } = new List<ColumnBindingModel>();
    }

    public class ColumnBindingModel : ColumnDefinition
    {
        public ColumnBindingModel(string name) : base(name)
        {
        }

        public string CSharpType { get; set; } = null!;

        public string NativeType { get; set; } = null!;
    }
}
