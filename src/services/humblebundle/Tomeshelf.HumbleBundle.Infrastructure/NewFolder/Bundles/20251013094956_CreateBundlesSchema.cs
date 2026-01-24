using System;

namespace Tomeshelf.HumbleBundle.Infrastructure.NewFolder.Bundles;

/// <inheritdoc />
public partial class CreateBundlesSchema : Migration
{
    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable("Bundles");
    }

    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable("Bundles", table => new
        {
            Id = table.Column<int>("int", nullable: false)
                      .Annotation("SqlServer:Identity", "1, 1"),
            MachineName = table.Column<string>("nvarchar(200)", maxLength: 200, nullable: false),
            Category = table.Column<string>("nvarchar(100)", maxLength: 100, nullable: true),
            Stamp = table.Column<string>("nvarchar(50)", maxLength: 50, nullable: true),
            Title = table.Column<string>("nvarchar(512)", maxLength: 512, nullable: true),
            ShortName = table.Column<string>("nvarchar(256)", maxLength: 256, nullable: true),
            Url = table.Column<string>("nvarchar(512)", maxLength: 512, nullable: true),
            TileImageUrl = table.Column<string>("nvarchar(512)", maxLength: 512, nullable: true),
            HeroImageUrl = table.Column<string>("nvarchar(512)", maxLength: 512, nullable: true),
            TileLogoUrl = table.Column<string>("nvarchar(512)", maxLength: 512, nullable: true),
            ShortDescription = table.Column<string>("nvarchar(1024)", maxLength: 1024, nullable: true),
            StartsAt = table.Column<DateTimeOffset>("datetimeoffset(0)", nullable: true),
            EndsAt = table.Column<DateTimeOffset>("datetimeoffset(0)", nullable: true),
            FirstSeenUtc = table.Column<DateTimeOffset>("datetimeoffset(0)", nullable: false),
            LastSeenUtc = table.Column<DateTimeOffset>("datetimeoffset(0)", nullable: false),
            LastUpdatedUtc = table.Column<DateTimeOffset>("datetimeoffset(0)", nullable: false)
        }, constraints: table =>
        {
            table.PrimaryKey("PK_Bundles", x => x.Id);
        });

        migrationBuilder.CreateIndex("IX_Bundles_MachineName", "Bundles", "MachineName", unique: true);
    }
}