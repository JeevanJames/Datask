// Copyright (c) 2021 Jeevan James
// This file is licensed to you under the MIT License.
// See the LICENSE file in the project root for more information.

using System.Reflection;
using System.Threading.Tasks;

using CodeBits;

namespace Datask.Tool.ExcelData.Core.Scripts
{
    public static class SqlScripts
    {
        public static async Task<string> BaseTables()
        {
            return await Assembly.GetExecutingAssembly()
                .LoadResourceAsStringAsync(typeof(SqlScripts), $"{nameof(BaseTables)}.sql")
                .ConfigureAwait(false);
        }

        public static async Task<string> TableColumnSchema()
        {
            return await Assembly.GetExecutingAssembly()
                .LoadResourceAsStringAsync(typeof(SqlScripts), $"{nameof(TableColumnSchema)}.sql")
                .ConfigureAwait(false);
        }

        public static async Task<string> TableReferenceSchema()
        {
            return await Assembly.GetExecutingAssembly()
                .LoadResourceAsStringAsync(typeof(SqlScripts), $"{nameof(TableReferenceSchema)}.sql")
                .ConfigureAwait(false);
        }
    }
}
