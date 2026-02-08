using Microsoft.EntityFrameworkCore.Migrations;
using System;

namespace Tomeshelf.SHiFT.Infrastructure.Persistence.Migrations;

/// <inheritdoc />
public partial class CreateShiftSchema : Migration
{
    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable("DataProtectionKeys");

        migrationBuilder.DropTable("ShiftSettings");
    }

    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable("DataProtectionKeys", table => new
        {
            Id = table.Column<int>("int", nullable: false)
                      .Annotation("SqlServer:Identity", "1, 1"),
            FriendlyName = table.Column<string>("nvarchar(max)", nullable: true),
            Xml = table.Column<string>("nvarchar(max)", nullable: true)
        }, constraints: table =>
        {
            table.PrimaryKey("PK_DataProtectionKeys", x => x.Id);
        });

        migrationBuilder.CreateTable("ShiftSettings", table => new
        {
            Id = table.Column<int>("int", nullable: false)
                      .Annotation("SqlServer:Identity", "1, 1"),
            Email = table.Column<string>("nvarchar(256)", maxLength: 256, nullable: true),
            EncryptedPassword = table.Column<string>("nvarchar(4000)", maxLength: 4000, nullable: true),
            DefaultService = table.Column<string>("nvarchar(32)", maxLength: 32, nullable: true),
            UpdatedUtc = table.Column<DateTimeOffset>("datetimeoffset", nullable: false)
        }, constraints: table =>
        {
            table.PrimaryKey("PK_ShiftSettings", x => x.Id);
        });
    }
}