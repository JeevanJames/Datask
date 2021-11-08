using System;
using System.IO;
using System.Reflection;
using System.Resources;
using System.Threading.Tasks;

namespace Datask.Common.Utilities
{
    public static class ResourceExtensions
    {
        public static Task<string> LoadResourceAsString(this Assembly assembly, string resourceName)
        {
            return assembly.LoadResource(null, resourceName, StringConverter);
        }

        public static Task<string> LoadResourceAsString(this Assembly assembly, Type type, string resourceName)
        {
            return assembly.LoadResource(type, resourceName, StringConverter);
        }

        private static async Task<string> StringConverter(Stream resourceStream)
        {
            using StreamReader reader = new(resourceStream);
            return await reader.ReadToEndAsync();
        }

        public static Task<byte[]> LoadResourceAsBinary(this Assembly assembly, string resourceName)
        {
            return assembly.LoadResource(null, resourceName, BinaryConverter);
        }

        public static Task<byte[]> LoadResourceAsBinary(this Assembly assembly, Type type, string resourceName)
        {
            return assembly.LoadResource(type, resourceName, BinaryConverter);
        }

        private static async Task<byte[]> BinaryConverter(Stream resourceStream)
        {
            await using MemoryStream ms = new();
            await resourceStream.CopyToAsync(ms).ConfigureAwait(false);
            return ms.ToArray();
        }

        private static Task<T> LoadResource<T>(this Assembly assembly, Type? nsScopedType, string resourceName,
            Func<Stream, Task<T>> converter)
        {
            if (assembly is null)
                throw new ArgumentNullException(nameof(assembly));

            Stream? resourceStream = nsScopedType is null
                ? assembly.GetManifestResourceStream(resourceName)
                : assembly.GetManifestResourceStream(nsScopedType, resourceName);

            if (resourceStream is null)
                throw new MissingManifestResourceException($"Could not find resource named {resourceStream}.");

            return converter(resourceStream);
        }
    }
}
