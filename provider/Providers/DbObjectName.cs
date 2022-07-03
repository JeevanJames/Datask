using Datask.Common.Utilities;

namespace Datask.Providers;

public readonly record struct DbObjectName(string Schema, string Name)
{
    public override string ToString()
    {
        return Schema + '.' + Name;
    }

    public static bool TryParse(string str, out DbObjectName objectName)
    {
        string[] parts = str.Split(new[] { '.' }, 2, StringSplitOptions.None);
        if (parts.Length == 2)
        {
            objectName = new DbObjectName(parts[0], parts[1]);
            return true;
        }

        objectName = None;
        return false;
    }

    public static readonly DbObjectName None = default;
}
