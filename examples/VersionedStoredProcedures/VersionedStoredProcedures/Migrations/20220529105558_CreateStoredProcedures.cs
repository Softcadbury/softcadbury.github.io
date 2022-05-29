#nullable disable

namespace VersionedStoredProcedures.Migrations;

using Microsoft.EntityFrameworkCore.Migrations;
using VersionedStoredProcedures.Extensions;
using VersionedStoredProcedures.StoredProcedures;

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