using System;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;

using Datask.Tool.ExcelData.Generator.Exceptions;

namespace Datask.Tool.ExcelData.Generator.Extensions
{
    public static class ReflectionExtensions
    {
        public static Task<string> LoadResourceAsTextAsync(this Assembly assembly, Type type, string name)
        {
            if (type is null)
                throw new ArgumentNullException(nameof(type));

            return LoadResourceAsync(assembly, ResourceToTextConverter, name, type);
        }

        private static Task<string> ResourceToTextConverter(Stream stream)
        {
            using var reader = new StreamReader(stream);
            return reader.ReadToEndAsync();
        }

        private static Task<T> LoadResourceAsync<T>(this Assembly assembly, Func<Stream, Task<T>> converter,
            string name, Type? type = null)
        {
            if (assembly is null)
                throw new ArgumentNullException(nameof(assembly));
            name.AssertNotNullOrWhitespace(nameof(name),
                "The specified resource name cannot be all whitespaces.",
                "The specified resource name cannot be empty.");

            using Stream? resourceStream = type is null
                ? assembly.GetManifestResourceStream(name)
                : assembly.GetManifestResourceStream(type, name);
            if (resourceStream is null)
            {
                string errorMessage = type is null
                    ? $"Could not load resource named '{name}' from assembly {assembly.FullName}"
                    : $"Could not load resource named '{type.Namespace}.{name}' from assembly {assembly.FullName}.";
                throw new PlaceholderArgumentException(errorMessage, nameof(assembly));
            }

            return converter(resourceStream);
        }
    }
}
