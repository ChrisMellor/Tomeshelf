using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Tomeshelf.Infrastructure.Persistence.Migrations.Mcm;

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