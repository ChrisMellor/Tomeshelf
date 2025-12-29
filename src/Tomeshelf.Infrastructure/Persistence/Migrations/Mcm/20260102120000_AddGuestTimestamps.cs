using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Tomeshelf.Infrastructure.Persistence.Migrations.Mcm;

/// <inheritdoc />
public partial class AddGuestTimestamps : Migration
{
    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropColumn("AddedAt", "Guests");

        migrationBuilder.DropColumn("RemovedAt", "Guests");
    }

    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddColumn<DateTimeOffset>("AddedAt", "Guests", "datetimeoffset", nullable: false, defaultValueSql: "SYSDATETIMEOFFSET()");

        migrationBuilder.AddColumn<DateTimeOffset>("RemovedAt", "Guests", "datetimeoffset", nullable: true);
    }
}