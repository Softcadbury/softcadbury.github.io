namespace VersionedStoredProcedures.Extensions;

using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using VersionedStoredProcedures.StoredProcedures;

internal static class DbContextExtensions
{
    /// <summary>
    /// Execute a stored procedure and return a list of objects
    /// </summary>
    /// <typeparam name="T">Type of the returned objects</typeparam>
    /// <param name="context">Entity Framework context</param>
    /// <param name="storedProcedure">Stored procedure to execute</param>
    /// <param name="parameters">List of parameters</param>
    /// <returns>IQueryable containing the list of objects returned</returns>
    public static IQueryable<T> ExecuteStoredProcedure<T>(this DbContext context, StoredProcedures storedProcedure, params SqlParameter[] parameters)
       where T : class
    {
        return context.Set<T>().FromSqlRaw(GetSql(storedProcedure, parameters), parameters.Cast<object>().ToArray());
    }

    /// <summary>
    /// Execute a stored procedure
    /// </summary>
    /// <param name="context">Entity Framework context</param>
    /// <param name="storedProcedure">Stored procedure to execute</param>
    /// <param name="parameters">List of parameters</param>
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