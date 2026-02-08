using Microsoft.EntityFrameworkCore.Migrations;
using System;

namespace Tomeshelf.Fitbit.Infrastructure.Fitbit;

/// <inheritdoc />
public partial class DropFitbitCredentials : Migration
{
    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable("FitbitCredentials", table => new
        {
            Id = table.Column<int>("int", nullable: false),
            AccessToken = table.Column<string>("nvarchar(2048)", maxLength: 2048, nullable: true),
            RefreshToken = table.Column<string>("nvarchar(2048)", maxLength: 2048, nullable: true),
            ExpiresAtUtc = table.Column<DateTimeOffset>("datetimeoffset", nullable: true)
        }, constraints: table =>
        {
            table.PrimaryKey("PK_FitbitCredentials", x => x.Id);
        });
    }

    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable("FitbitCredentials");
    }
}