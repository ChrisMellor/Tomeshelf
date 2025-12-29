using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Tomeshelf.Infrastructure.Persistence.Migrations.Mcm
{
    /// <inheritdoc />
    public partial class AddGuestTimestamps : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "AddedAt",
                table: "Guests",
                type: "datetimeoffset",
                nullable: false,
                defaultValueSql: "SYSDATETIMEOFFSET()");

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "RemovedAt",
                table: "Guests",
                type: "datetimeoffset",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AddedAt",
                table: "Guests");

            migrationBuilder.DropColumn(
                name: "RemovedAt",
                table: "Guests");
        }
    }
}
