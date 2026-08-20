using System.Reflection;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Business.Infrastructure.Persistence.Migrations;

internal static class EmbeddedSql
{
    public static void ExecuteFolder(MigrationBuilder migrationBuilder, string relativeFolder)
    {
        ArgumentNullException.ThrowIfNull(migrationBuilder);
        ArgumentException.ThrowIfNullOrWhiteSpace(relativeFolder);

        var assembly = typeof(EmbeddedSql).Assembly;
        var folderToken = $".{ToResourcePath(relativeFolder).Trim('.')}.";
        var resourceNames = assembly
            .GetManifestResourceNames()
            .Where(name =>
                name.Contains(folderToken, StringComparison.Ordinal)
                && name.EndsWith(".sql", StringComparison.OrdinalIgnoreCase))
            .OrderBy(name => name, StringComparer.Ordinal)
            .ToArray();

        foreach (var resourceName in resourceNames)
        {
            migrationBuilder.Sql(ReadResource(assembly, resourceName));
        }
    }

    public static string Read(string relativePath)
    {
        var assembly = typeof(EmbeddedSql).Assembly;
        var resourceSuffix = ToResourcePath(relativePath);
        var resourceNames = assembly
            .GetManifestResourceNames()
            .Where(name => name.EndsWith(resourceSuffix, StringComparison.Ordinal))
            .ToArray();

        if (resourceNames.Length != 1)
        {
            throw new InvalidOperationException(
                $"Expected one embedded SQL resource ending with '{resourceSuffix}', but found {resourceNames.Length}.");
        }

        return ReadResource(assembly, resourceNames[0]);
    }

    private static string ReadResource(Assembly assembly, string resourceName)
    {
        using var stream = assembly.GetManifestResourceStream(resourceName)
            ?? throw new InvalidOperationException(
                $"Embedded SQL resource '{resourceName}' could not be opened.");
        using var reader = new StreamReader(stream);
        return reader.ReadToEnd();
    }

    private static string ToResourcePath(string path) =>
        path.Replace('/', '.').Replace('\\', '.');
}
