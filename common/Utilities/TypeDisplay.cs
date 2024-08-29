// Copyright (c) 2021 Jeevan James
// This file is licensed to you under the MIT License.
// See the LICENSE file in the project root for more information.

namespace Datask.Common.Utilities;

public static class TypeDisplay
{
    public static string GetTypeName(Type type, params string[] trimmableNamespaces)
    {
        if (_typeAliases.TryGetValue(type, out string? alias))
            return alias;

        // Deal with generics

        return Array.Exists(trimmableNamespaces, ns => string.Equals(type.Namespace, ns, StringComparison.Ordinal))
            ? type.Name
            : type.FullName ?? type.Name;
    }

    private static readonly Dictionary<Type, string> _typeAliases = new()
    {
        [typeof(string)] = "string",
        [typeof(char)] = "char",
        [typeof(bool)] = "bool",
        [typeof(byte)] = "byte",
        [typeof(sbyte)] = "sbyte",
        [typeof(short)] = "short",
        [typeof(ushort)] = "ushort",
        [typeof(int)] = "int",
        [typeof(uint)] = "uint",
        [typeof(long)] = "long",
        [typeof(ulong)] = "ulong",
        [typeof(float)] = "float",
        [typeof(double)] = "double",
        [typeof(decimal)] = "decimal",
        [typeof(object)] = "object",
    };
}
