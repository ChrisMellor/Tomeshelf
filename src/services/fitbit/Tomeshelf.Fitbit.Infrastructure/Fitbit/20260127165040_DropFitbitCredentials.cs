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
        // SQL Server throws when dropping a non-existent table; keep this migration idempotent.
        // Note: keep schema unqualified to match the behavior of migrationBuilder.DropTable(...).
        migrationBuilder.Sql(@"
IF OBJECT_ID(N'[FitbitCredentials]', N'U') IS NOT NULL
BEGIN
    DROP TABLE [FitbitCredentials];
END
");
    }
}
