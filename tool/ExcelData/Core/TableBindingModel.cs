// Copyright (c) 2021 Jeevan James
// This file is licensed to you under the MIT License.
// See the LICENSE file in the project root for more information.

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

        public int ParameterSize { get; set; }
    }
}
