namespace VersionizedStoredProcedures.Contexts;

using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using VersionizedStoredProcedures.Entities;
using VersionizedStoredProcedures.Extensions;
using VersionizedStoredProcedures.StoredProcedures;

/// <summary>
/// Entity Framework context
/// </summary>
public class Context : DbContext
{
    public Context(DbContextOptions<Context> options)
           : base(options)
    { }

    public DbSet<Item> Items { get; set; } = null!;

    /// <summary>
    /// Method to delete all items
    /// </summary>
    public async Task DeleteItems()
    {
        await this.ExecuteStoredProcedure(StoredProcedures.DeleteItems);
    }

    /// <summary>
    /// Method to retrieve items by label
    /// </summary>
    public IQueryable<Item> GetItems(string label)
    {
        SqlParameter labelParameter = new("label", label);
        return this.ExecuteStoredProcedure<Item>(StoredProcedures.GetItems, labelParameter);
    }
}