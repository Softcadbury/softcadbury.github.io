---
layout: post
title: "Versioned stored procedures with Entity Framework"
date: 2022-05-10 10:27:52 +0200
categories: dotnet ef
comments: true
---

## Entity Framework (EF)

[EF](https://docs.microsoft.com/en-us/ef/) is an awesome .Net library developed by Microsoft that allows your applications to interact with databases.

One of the best feature of the library is the possibility to [versionised](https://docs.microsoft.com/en-us/ef/core/managing-schemas/) your database. Its functioning is simple, you create classes that will represent your database tables, then generate a [migration](https://docs.microsoft.com/en-us/ef/core/managing-schemas/migrations). Once you run this migration, your database schema will be updated accordingly.

In addition to this migration feature, EF provides methods to create, update and delete entities easily. Unfortunatly, nothing's perfect, an EF can suffer performance problems if you want do to complex tasks like doing bulk modifications or requests with many condiditions.

To resolve these performance problems, you can use additional libraries like [this one](https://entityframework-extensions.net/bulk-insert), but for more specific cases, you will need a native SQL solution, like <a href="https://docs.microsoft.com/en-us/sql/relational-databases/stored-procedures/create-a-stored-procedure?view=sql-server-ver16">stored procedures</a>.

## Stored procedures in EF

With EF, it's pretty easy to execute stored procedure. The complexity lies in how to versionize these stored procedure as EF doesn't provide a native solution inside its migrations.

Migrations allows you to exceture raw SQL, so you can just copy/past the code of your stored procedures. Here's an example found <a href="https://dotnetthoughts.net/creating-stored-procs-in-efcore-migrations">on the web</a>:
{% highlight csharp %}
public partial class GetAllTodoItemsByStatusProc : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        var createProcSql = @"CREATE OR ALTER PROC usp_GetAllTodoItemsByStatus(@isCompleted BIT) AS SELECT * FROM TodoItems WHERE IsCompleted = @isCompleted";
        migrationBuilder.Sql(createProcSql);
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        var dropProcSql = "DROP PROC usp_GetAllTodoItemsByStatus";
        migrationBuilder.Sql(dropProcSql);
    }
}
{% endhighlight %}

⚠️ Unfortunatly, there are many downside to doing that:

-   It is just a string, so no SQL hightlithing.
-   It is easy to do a mistake when copying the code (especially when you want to update the code of an existing stored procedure).
-   When updating the code of a stored procedure, it's hard to see modifications with GIT (no great for people doing reviews).

In the following section, I will show you how I handle this in my projects.
I won't address views or functions in this article, but the principle is exaclty the same.

## How to versioned stored procedures in EF

You can see the example project <a href="https://github.com/Softcadbury/softcadbury.github.io/tree/main/examples/VersionedStoredProcedures">here</a>.

### Create the <a href="https://github.com/Softcadbury/softcadbury.github.io/blob/main/examples/VersionedStoredProcedures/VersionedStoredProcedures/Contexts/Context.cs">EF context</a> and an <a href="https://github.com/Softcadbury/softcadbury.github.io/blob/main/examples/VersionedStoredProcedures/VersionedStoredProcedures/Entities/Item.cs">entity</a>

{% highlight csharp %}
public class Context : DbContext
{
    public Context(DbContextOptions<Context> options)
           : base(options)
    { }

    public DbSet<Item> Items { get; set; } = null!;
}
{% endhighlight %}

{% highlight csharp %}
public class Item
{
    public int Id { get; set; }

    public string Label { get; set; }

    public Item(string label)
    {
        Label = label;
    }
}
{% endhighlight %}

### Create the <a href="https://github.com/Softcadbury/softcadbury.github.io/blob/main/examples/VersionedStoredProcedures/VersionedStoredProcedures/StoredProcedures/StoredProcedures.cs">stored procedure enumeration</a>

Create an enumeration listing all your stored procedures.

{% highlight csharp %}
internal enum StoredProcedures
{
    GetItems,
    DeleteItems,
}
{% endhighlight %}

### Create the <a href="https://github.com/Softcadbury/softcadbury.github.io/tree/main/examples/VersionedStoredProcedures/VersionedStoredProcedures/StoredProcedures/StoredProcedures">stored procedure SQL files</a>

Create the SQL files containing the code of the stored procedures. Theses files must respect a few rules:

-   Files must be stored in the same folder as the enumeration StoredProcedures.cs (this is important for MigrationBuilderExtensions.CreateStoredProcedure() which have to know where to retrieve these files)
    -   /StoredProcedures.cs
    -   /StoredProcedures/*.sql

-   Files name must contain the version number (two digits format). When creating a new version, you will have to create a new file and increment the version number.
    - GetItems_01.sql
    - GetItems_02.sql
    - etc.

-   Files must be embedded resources (you can change that in files properties when using Visual Studio). This will change the csproj file as follow.
{% highlight csharp %}
<ItemGroup>
    <None Remove="StoredProcedures\StoredProcedures\DeleteItems_01.sql" />
    <None Remove="StoredProcedures\StoredProcedures\GetItems_01.sql" />
    <None Remove="StoredProcedures\StoredProcedures\GetItems_02.sql" />
</ItemGroup>

<ItemGroup>
    <EmbeddedResource Include="StoredProcedures\StoredProcedures\DeleteItems_01.sql" />
    <EmbeddedResource Include="StoredProcedures\StoredProcedures\GetItems_01.sql" />
    <EmbeddedResource Include="StoredProcedures\StoredProcedures\GetItems_02.sql" />
</ItemGroup>
{% endhighlight %}

-   SQL scripts must be reusable to simplify new versions of these scripts, so always drop stored procedures if already created.
{% highlight sql %}
IF OBJECT_ID('dbo.GetItems', 'P') IS NOT NULL
	DROP PROCEDURE dbo.GetItems
GO

CREATE PROCEDURE dbo.GetItems
	@label VARCHAR(max)

AS
BEGIN
    SET NOCOUNT ON;

	SELECT Id, Label
	FROM Items
	WHERE label = @label

END
GO
{% endhighlight %}

### Create and update your stored procedures in your <a href="https://github.com/Softcadbury/softcadbury.github.io/tree/main/examples/VersionedStoredProcedures/VersionedStoredProcedures/Migrations">migrations</a> using these <a href="https://github.com/Softcadbury/softcadbury.github.io/blob/main/examples/VersionedStoredProcedures/VersionedStoredProcedures/Extensions/DbContextExtensions.cs">extentions</a>

{% highlight csharp %}
internal static class MigrationBuilderExtensions
{
    public static void CreateStoredProcedure(this MigrationBuilder migrationBuilder, StoredProcedures storedProcedure, int version)
    {
        string fullVersion = version.ToString(CultureInfo.InvariantCulture).PadLeft(2, '0');
        string fullName = $"{storedProcedure.GetType().Namespace}.StoredProcedures.{storedProcedure}_{fullVersion}.sql";
        migrationBuilder.ExecuteSqlFile(fullName);
    }

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
{% endhighlight %}

{% highlight csharp %}
public partial class CreateStoredProcedures : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        // Creates the two stored procedures
        migrationBuilder.CreateStoredProcedure(StoredProcedures.DeleteItems, 1);
        migrationBuilder.CreateStoredProcedure(StoredProcedures.GetItems, 1);
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        // Drops the two stored procedures
        migrationBuilder.DropStoredProcedure(StoredProcedures.DeleteItems);
        migrationBuilder.DropStoredProcedure(StoredProcedures.GetItems);
    }
}
{% endhighlight %}

{% highlight csharp %}
public partial class UpdateStoredProcedures : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        // Update the stored procedure to version 2
        migrationBuilder.CreateStoredProcedure(StoredProcedures.GetItems, 2);
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        // Revert the stored procedure to version 1
        migrationBuilder.CreateStoredProcedure(StoredProcedures.GetItems, 1);
    }
}
{% endhighlight %}

### Complete the <a href="https://github.com/Softcadbury/softcadbury.github.io/blob/main/examples/VersionedStoredProcedures/VersionedStoredProcedures/Contexts/Context.cs">EF context</a> to execute your stored procedures using these <a href="https://github.com/Softcadbury/softcadbury.github.io/blob/main/examples/VersionedStoredProcedures/VersionedStoredProcedures/Extensions/DbContextExtensions.cs">extentions</a>

{% highlight csharp %}
internal static class DbContextExtensions
{
    public static IQueryable<T> ExecuteStoredProcedure<T>(this DbContext context, StoredProcedures storedProcedure, params SqlParameter[] parameters)
       where T : class
    {
        return context.Set<T>().FromSqlRaw(GetSql(storedProcedure, parameters), parameters.Cast<object>().ToArray());
    }

    public static async Task ExecuteStoredProcedure(this DbContext context, StoredProcedures storedProcedure, params SqlParameter[] parameters)
    {
        await context.Database.ExecuteSqlRawAsync(GetSql(storedProcedure, parameters), parameters.Cast<object>().ToArray());
    }

    private static string GetSql(StoredProcedures storedProcedure, params SqlParameter[] parameters)
    {
        string parameterList = string.Join(", ", parameters.Select(p => "@" + p.ParameterName));
        return $"EXECUTE {storedProcedure} {parameterList}";
    }
}
{% endhighlight %}

{% highlight csharp %}
public class Context : DbContext
{
    public Context(DbContextOptions<Context> options)
           : base(options)
    { }

    public DbSet<Item> Items { get; set; } = null!;

    public async Task DeleteItems()
    {
        await this.ExecuteStoredProcedure(StoredProcedures.DeleteItems);
    }

    public IQueryable<Item> GetItems(string label)
    {
        SqlParameter labelParameter = new("label", label);
        return this.ExecuteStoredProcedure<Item>(StoredProcedures.GetItems, labelParameter);
    }
}
{% endhighlight %}

## Feedbacks

If you have any comments, please feel free to create an issue <a href="https://github.com/Softcadbury/softcadbury.github.io/issues">here</a>