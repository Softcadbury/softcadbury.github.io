namespace VersionedStoredProcedures.Contexts;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

/// <summary>
/// Factory used to generate Entity Framework migrations
/// => Run the command `dotnet ef migrations add MigrationName`
/// </summary>
public class ContextDesignTimeFactory : IDesignTimeDbContextFactory<Context>
{
    public Context CreateDbContext(string[] args)
    {
        const string connectionString = "Server=.;Database=VersionedStoredProceduresExample";
        DbContextOptions<Context> options = new DbContextOptionsBuilder<Context>().UseSqlServer(connectionString).Options;

        return new Context(options);
    }
}