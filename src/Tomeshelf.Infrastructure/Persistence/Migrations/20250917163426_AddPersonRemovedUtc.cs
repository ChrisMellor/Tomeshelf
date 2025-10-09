using Microsoft.EntityFrameworkCore.Migrations;
using System;

#nullable disable

namespace Tomeshelf.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddPersonRemovedUtc : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "RemovedUtc",
                table: "People",
                type: "datetime2",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "RemovedUtc",
                table: "People");
        }
    }
}
