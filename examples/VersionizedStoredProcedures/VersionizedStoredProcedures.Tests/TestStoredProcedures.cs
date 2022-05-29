namespace VersionizedStoredProcedures.Tests;

using Microsoft.EntityFrameworkCore;
using NUnit.Framework;
using VersionizedStoredProcedures.Contexts;
using VersionizedStoredProcedures.Entities;

[TestFixture]
public class TestStoredProcedures
{
    // Change this connection string if you want to run tests
    private const string ConnectionString = "Server=.;Database=VersionizedStoredProceduresExample;User ID=user id;Password=password";

    [SetUp]
    public async Task Setup()
    {
        // Ensure that each test start without any items
        Context context = GetEntityFrameworkContext();
        context.Items.RemoveRange(context.Items);
        await context.SaveChangesAsync();
    }

    [Test]
    public async Task StoredProcedures_DeleteItems()
    {
        // Arrange
        Context context = GetEntityFrameworkContext();
        context.Items.Add(new Item("item 1"));
        context.Items.Add(new Item("item 2"));
        await context.SaveChangesAsync();

        // Assert
        Context assertContext1 = GetEntityFrameworkContext();
        Assert.That(await assertContext1.Items.ToArrayAsync(), Has.Length.EqualTo(2));

        // Act
        await context.DeleteItems();

        // Assert
        Context assertContext2 = GetEntityFrameworkContext();
        Assert.That(await assertContext2.Items.ToArrayAsync(), Is.Empty);
    }

    [Test]
    public async Task StoredProcedures_GetItems()
    {
        // Arrange
        Context context = GetEntityFrameworkContext();
        context.Items.Add(new Item("item 1"));
        context.Items.Add(new Item("item 2"));
        context.Items.Add(new Item("item 2"));
        await context.SaveChangesAsync();

        // Assert
        Context assertContext1 = GetEntityFrameworkContext();
        Assert.That(await assertContext1.Items.ToArrayAsync(), Has.Length.EqualTo(3));

        // Act
        Item[] items = await context.GetItems("item 2").ToArrayAsync();

        // Assert
        Assert.That(items, Has.Length.EqualTo(2));
        Assert.That(items.Select(p => p.Label), Is.EqualTo(new[] { "item 2", "item 2" }));
    }

    private static Context GetEntityFrameworkContext()
    {
        DbContextOptions<Context> options = new DbContextOptionsBuilder<Context>()
            .UseSqlServer(ConnectionString).Options;

        var context = new Context(options);
        context.Database.Migrate();

        return context;
    }
}