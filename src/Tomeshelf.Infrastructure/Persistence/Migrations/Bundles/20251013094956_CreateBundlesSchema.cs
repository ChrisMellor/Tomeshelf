using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Tomeshelf.Infrastructure.Persistence.Migrations.Bundles
{
    /// <inheritdoc />
    public partial class CreateBundlesSchema : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Bundles",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    MachineName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Category = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Stamp = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    Title = table.Column<string>(type: "nvarchar(512)", maxLength: 512, nullable: true),
                    ShortName = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    Url = table.Column<string>(type: "nvarchar(512)", maxLength: 512, nullable: true),
                    TileImageUrl = table.Column<string>(type: "nvarchar(512)", maxLength: 512, nullable: true),
                    HeroImageUrl = table.Column<string>(type: "nvarchar(512)", maxLength: 512, nullable: true),
                    TileLogoUrl = table.Column<string>(type: "nvarchar(512)", maxLength: 512, nullable: true),
                    ShortDescription = table.Column<string>(type: "nvarchar(1024)", maxLength: 1024, nullable: true),
                    StartsAt = table.Column<DateTimeOffset>(type: "datetimeoffset(0)", nullable: true),
                    EndsAt = table.Column<DateTimeOffset>(type: "datetimeoffset(0)", nullable: true),
                    FirstSeenUtc = table.Column<DateTimeOffset>(type: "datetimeoffset(0)", nullable: false),
                    LastSeenUtc = table.Column<DateTimeOffset>(type: "datetimeoffset(0)", nullable: false),
                    LastUpdatedUtc = table.Column<DateTimeOffset>(type: "datetimeoffset(0)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Bundles", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Bundles_MachineName",
                table: "Bundles",
                column: "MachineName",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Bundles");
        }
    }
}
