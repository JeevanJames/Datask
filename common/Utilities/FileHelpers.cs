namespace Datask.Common.Utilities;

public static class FileHelpers
{
    public static void EnsureDirectoryExists(string filePath)
    {
        string? directory = Path.GetDirectoryName(Path.GetFullPath(filePath));
        if (directory is null)
            throw new DataskException($"Could not extract directory information for file '{filePath}'.");
        if (!Directory.Exists(directory))
            Directory.CreateDirectory(directory);
    }
}
