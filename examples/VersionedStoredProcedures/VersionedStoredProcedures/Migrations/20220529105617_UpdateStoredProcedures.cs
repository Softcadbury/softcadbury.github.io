#nullable disable

namespace VersionedStoredProcedures.Migrations;

using Microsoft.EntityFrameworkCore.Migrations;
using VersionedStoredProcedures.Extensions;
using VersionedStoredProcedures.StoredProcedures;

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