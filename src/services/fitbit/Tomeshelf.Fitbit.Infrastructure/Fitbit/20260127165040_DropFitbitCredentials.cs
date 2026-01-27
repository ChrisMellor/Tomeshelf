using Microsoft.EntityFrameworkCore.Migrations;
using System;

#nullable disable

namespace Tomeshelf.Infrastructure.Persistence.Fitbit.Migrations
{
    /// <inheritdoc />
    public partial class DropFitbitCredentials : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "FitbitCredentials");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "FitbitCredentials",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false),
                    AccessToken = table.Column<string>(type: "nvarchar(2048)", maxLength: 2048, nullable: true),
                    RefreshToken = table.Column<string>(type: "nvarchar(2048)", maxLength: 2048, nullable: true),
                    ExpiresAtUtc = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FitbitCredentials", x => x.Id);
                });
        }
    }
}
