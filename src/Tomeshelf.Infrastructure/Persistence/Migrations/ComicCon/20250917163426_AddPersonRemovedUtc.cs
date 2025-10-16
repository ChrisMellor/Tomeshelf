using Microsoft.EntityFrameworkCore.Migrations;
using System;

namespace Tomeshelf.Infrastructure.Persistence.Migrations.ComicCon;

/// <inheritdoc />
public partial class AddPersonRemovedUtc : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddColumn<DateTime>("RemovedUtc", "People", "datetime2", nullable: true);
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropColumn("RemovedUtc", "People");
    }
}