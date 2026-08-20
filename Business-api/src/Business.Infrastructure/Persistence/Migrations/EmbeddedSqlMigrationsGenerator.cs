using System.Text.RegularExpressions;
using Microsoft.EntityFrameworkCore.Migrations.Design;
using Microsoft.EntityFrameworkCore.Migrations.Operations;

namespace Business.Infrastructure.Persistence.Migrations;

public sealed class EmbeddedSqlMigrationsGenerator(
    MigrationsCodeGeneratorDependencies dependencies,
    CSharpMigrationsGeneratorDependencies csharpDependencies)
    : CSharpMigrationsGenerator(dependencies, csharpDependencies)
{
    public override string GenerateMigration(
        string? migrationNamespace,
        string migrationName,
        IReadOnlyList<MigrationOperation> upOperations,
        IReadOnlyList<MigrationOperation> downOperations)
    {
        var code = base.GenerateMigration(
            migrationNamespace,
            migrationName,
            upOperations,
            downOperations);
        var sqlFolder = $"Persistence/Sql/Migrations/{migrationName}";

        code = InsertExecuteFolderAtEnd(code, "Up", $"{sqlFolder}/Up");
        return InsertExecuteFolderAtStart(code, "Down", $"{sqlFolder}/Down");
    }

    private static string InsertExecuteFolderAtStart(
        string code,
        string methodName,
        string relativeFolder)
    {
        var pattern = $@"(protected override void {methodName}\(MigrationBuilder migrationBuilder\)\s*\{{)";
        var matches = Regex.Matches(code, pattern, RegexOptions.CultureInvariant);
        if (matches.Count != 1)
        {
            throw new InvalidOperationException(
                $"Expected one generated {methodName} method, but found {matches.Count}.");
        }

        return Regex.Replace(
            code,
            pattern,
            match => $"{match.Value}{Environment.NewLine}" +
                     $"            global::Business.Infrastructure.Persistence.Migrations.EmbeddedSql.ExecuteFolder({Environment.NewLine}" +
                     $"                migrationBuilder,{Environment.NewLine}" +
                     $"                {Convert.ToChar(34)}{relativeFolder}{Convert.ToChar(34)});",
            RegexOptions.CultureInvariant,
            TimeSpan.FromSeconds(1));
    }

    private static string InsertExecuteFolderAtEnd(
        string code,
        string methodName,
        string relativeFolder)
    {
        var methodPattern = $@"protected override void {methodName}\(MigrationBuilder migrationBuilder\)\s*\{{";
        var methodMatches = Regex.Matches(code, methodPattern, RegexOptions.CultureInvariant);
        if (methodMatches.Count != 1)
        {
            throw new InvalidOperationException(
                $"Expected one generated {methodName} method, but found {methodMatches.Count}.");
        }

        var nextMethodIndex = code.IndexOf(
            "protected override void Down",
            methodMatches[0].Index + methodMatches[0].Length,
            StringComparison.Ordinal);
        if (nextMethodIndex < 0)
        {
            throw new InvalidOperationException("Could not find the generated Down method.");
        }

        var newLine = code.Contains("\r\n", StringComparison.Ordinal) ? "\r\n" : "\n";
        var closingBraceIndex = code.LastIndexOf(
            $"{newLine}        }}",
            nextMethodIndex,
            StringComparison.Ordinal);
        if (closingBraceIndex < 0)
        {
            throw new InvalidOperationException(
                $"Could not find the end of generated {methodName} method.");
        }

        var statement =
            $"            global::Business.Infrastructure.Persistence.Migrations.EmbeddedSql.ExecuteFolder({newLine}" +
            $"                migrationBuilder,{newLine}" +
            $"                {Convert.ToChar(34)}{relativeFolder}{Convert.ToChar(34)});";

        return code.Insert(closingBraceIndex, $"{newLine}{statement}");
    }
}
