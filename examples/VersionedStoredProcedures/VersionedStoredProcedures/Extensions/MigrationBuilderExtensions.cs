namespace VersionedStoredProcedures.Extensions;

using System.Globalization;
using System.Reflection;
using Microsoft.EntityFrameworkCore.Migrations;
using VersionedStoredProcedures.StoredProcedures;

internal static class MigrationBuilderExtensions
{
    /// <summary>
    /// Create a stored procedure
    /// </summary>
    /// <param name="migrationBuilder">Migration builder used to execute the request</param>
    /// <param name="storedProcedure">Stored procedure to create</param>
    /// <param name="version">Version of the stored procedure to create</param>
    public static void CreateStoredProcedure(this MigrationBuilder migrationBuilder, StoredProcedures storedProcedure, int version)
    {
        string fullVersion = version.ToString(CultureInfo.InvariantCulture).PadLeft(2, '0');
        string fullName = $"{storedProcedure.GetType().Namespace}.StoredProcedures.{storedProcedure}_{fullVersion}.sql";
        migrationBuilder.ExecuteSqlFile(fullName);
    }

    /// <summary>
    /// Drop a stored procedure
    /// </summary>
    /// <param name="migrationBuilder">Migration builder used to execute the request</param>
    /// <param name="storedProcedure">Stored procedure to drop</param>
    public static void DropStoredProcedure(this MigrationBuilder migrationBuilder, StoredProcedures storedProcedure)
    {
        migrationBuilder.Sql($"DROP PROCEDURE {storedProcedure}");
    }

    private static void ExecuteSqlFile(this MigrationBuilder migrationBuilder, string sqlFileFullPath)
    {
        Assembly assembly = Assembly.GetExecutingAssembly();
        using Stream? stream = assembly.GetManifestResourceStream(sqlFileFullPath);
        using StreamReader reader = new(stream ?? throw new ArgumentException($"The file {sqlFileFullPath} was not found"));
        migrationBuilder.Sql(reader.ReadToEnd());
    }
}