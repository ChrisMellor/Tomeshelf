using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Tomeshelf.Infrastructure.Persistence.Migrations.Mcm;

/// <inheritdoc />
public partial class InitialCreate : Migration
{
    /// <inheritdoc />
    /// <summary>
    ///     Builds the initial database schema for Tomeshelf.
    /// </summary>
    /// <param name="migrationBuilder">The migration builder.</param>
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable("Categories", table => new
        {
                Id = table.Column<int>("int", nullable: false)
                          .Annotation("SqlServer:Identity", "1, 1"),
                ExternalId = table.Column<string>("nvarchar(450)", nullable: true),
                Name = table.Column<string>("nvarchar(200)", maxLength: 200, nullable: false)
        }, constraints: table =>
        {
            table.PrimaryKey("PK_Categories", x => x.Id);
        });

        migrationBuilder.CreateTable("Events", table => new
        {
                Id = table.Column<int>("int", nullable: false)
                          .Annotation("SqlServer:Identity", "1, 1"),
                ExternalId = table.Column<string>("nvarchar(450)", nullable: true),
                Name = table.Column<string>("nvarchar(300)", maxLength: 300, nullable: false),
                Slug = table.Column<string>("nvarchar(200)", maxLength: 200, nullable: false),
                CreatedUtc = table.Column<DateTime>("datetime2", nullable: false),
                UpdatedUtc = table.Column<DateTime>("datetime2", nullable: true)
        }, constraints: table =>
        {
            table.PrimaryKey("PK_Events", x => x.Id);
        });

        migrationBuilder.CreateTable("People", table => new
        {
                Id = table.Column<int>("int", nullable: false)
                          .Annotation("SqlServer:Identity", "1, 1"),
                ExternalId = table.Column<string>("nvarchar(450)", nullable: true),
                Uid = table.Column<string>("nvarchar(max)", nullable: true),
                PubliclyVisible = table.Column<bool>("bit", nullable: false),
                FirstName = table.Column<string>("nvarchar(150)", maxLength: 150, nullable: true),
                LastName = table.Column<string>("nvarchar(150)", maxLength: 150, nullable: true),
                AltName = table.Column<string>("nvarchar(200)", maxLength: 200, nullable: true),
                Bio = table.Column<string>("nvarchar(max)", nullable: true),
                KnownFor = table.Column<string>("nvarchar(500)", maxLength: 500, nullable: true),
                ProfileUrl = table.Column<string>("nvarchar(1000)", maxLength: 1000, nullable: true),
                ProfileUrlLabel = table.Column<string>("nvarchar(max)", nullable: true),
                VideoLink = table.Column<string>("nvarchar(1000)", maxLength: 1000, nullable: true),
                Twitter = table.Column<string>("nvarchar(300)", maxLength: 300, nullable: true),
                Facebook = table.Column<string>("nvarchar(300)", maxLength: 300, nullable: true),
                Instagram = table.Column<string>("nvarchar(300)", maxLength: 300, nullable: true),
                YouTube = table.Column<string>("nvarchar(300)", maxLength: 300, nullable: true),
                Twitch = table.Column<string>("nvarchar(300)", maxLength: 300, nullable: true),
                Snapchat = table.Column<string>("nvarchar(300)", maxLength: 300, nullable: true),
                DeviantArt = table.Column<string>("nvarchar(300)", maxLength: 300, nullable: true),
                Tumblr = table.Column<string>("nvarchar(300)", maxLength: 300, nullable: true),
                CreatedUtc = table.Column<DateTime>("datetime2", nullable: false),
                UpdatedUtc = table.Column<DateTime>("datetime2", nullable: true)
        }, constraints: table =>
        {
            table.PrimaryKey("PK_People", x => x.Id);
        });

        migrationBuilder.CreateTable("VenueLocations", table => new
        {
                Id = table.Column<int>("int", nullable: false)
                          .Annotation("SqlServer:Identity", "1, 1"),
                ExternalId = table.Column<string>("nvarchar(450)", nullable: true),
                Name = table.Column<string>("nvarchar(300)", maxLength: 300, nullable: false)
        }, constraints: table =>
        {
            table.PrimaryKey("PK_VenueLocations", x => x.Id);
        });

        migrationBuilder.CreateTable("EventAppearances", table => new
        {
                Id = table.Column<int>("int", nullable: false)
                          .Annotation("SqlServer:Identity", "1, 1"),
                EventId = table.Column<int>("int", nullable: false),
                PersonId = table.Column<int>("int", nullable: false),
                DaysAtShow = table.Column<string>("nvarchar(50)", maxLength: 50, nullable: true),
                BoothNumber = table.Column<string>("nvarchar(200)", maxLength: 200, nullable: true),
                AutographAmount = table.Column<decimal>("decimal(10,2)", nullable: true),
                PhotoOpAmount = table.Column<decimal>("decimal(10,2)", nullable: true),
                PhotoOpTableAmount = table.Column<decimal>("decimal(10,2)", nullable: true)
        }, constraints: table =>
        {
            table.PrimaryKey("PK_EventAppearances", x => x.Id);
            table.ForeignKey("FK_EventAppearances_Events_EventId", x => x.EventId, "Events", "Id", onDelete: ReferentialAction.Cascade);
            table.ForeignKey("FK_EventAppearances_People_PersonId", x => x.PersonId, "People", "Id", onDelete: ReferentialAction.Cascade);
        });

        migrationBuilder.CreateTable("PersonCategories", table => new
        {
                PersonId = table.Column<int>("int", nullable: false),
                CategoryId = table.Column<int>("int", nullable: false)
        }, constraints: table =>
        {
            table.PrimaryKey("PK_PersonCategories", x => new
            {
                    x.PersonId,
                    x.CategoryId
            });
            table.ForeignKey("FK_PersonCategories_Categories_CategoryId", x => x.CategoryId, "Categories", "Id", onDelete: ReferentialAction.Cascade);
            table.ForeignKey("FK_PersonCategories_People_PersonId", x => x.PersonId, "People", "Id", onDelete: ReferentialAction.Cascade);
        });

        migrationBuilder.CreateTable("PersonImages", table => new
        {
                Id = table.Column<int>("int", nullable: false)
                          .Annotation("SqlServer:Identity", "1, 1"),
                PersonId = table.Column<int>("int", nullable: false),
                Big = table.Column<string>("nvarchar(1000)", maxLength: 1000, nullable: true),
                Med = table.Column<string>("nvarchar(1000)", maxLength: 1000, nullable: true),
                Small = table.Column<string>("nvarchar(1000)", maxLength: 1000, nullable: true),
                Thumb = table.Column<string>("nvarchar(1000)", maxLength: 1000, nullable: true)
        }, constraints: table =>
        {
            table.PrimaryKey("PK_PersonImages", x => x.Id);
            table.ForeignKey("FK_PersonImages_People_PersonId", x => x.PersonId, "People", "Id", onDelete: ReferentialAction.Cascade);
        });

        migrationBuilder.CreateTable("Schedules", table => new
        {
                Id = table.Column<int>("int", nullable: false)
                          .Annotation("SqlServer:Identity", "1, 1"),
                ExternalId = table.Column<string>("nvarchar(450)", nullable: true),
                EventAppearanceId = table.Column<int>("int", nullable: false),
                Title = table.Column<string>("nvarchar(300)", maxLength: 300, nullable: false),
                Description = table.Column<string>("nvarchar(max)", nullable: true),
                StartTimeUtc = table.Column<DateTime>("datetime2", nullable: false),
                EndTimeUtc = table.Column<DateTime>("datetime2", nullable: true),
                NoEndTime = table.Column<bool>("bit", nullable: false),
                Location = table.Column<string>("nvarchar(300)", maxLength: 300, nullable: true),
                VenueLocationId = table.Column<int>("int", nullable: true)
        }, constraints: table =>
        {
            table.PrimaryKey("PK_Schedules", x => x.Id);
            table.ForeignKey("FK_Schedules_EventAppearances_EventAppearanceId", x => x.EventAppearanceId, "EventAppearances", "Id", onDelete: ReferentialAction.Cascade);
            table.ForeignKey("FK_Schedules_VenueLocations_VenueLocationId", x => x.VenueLocationId, "VenueLocations", "Id");
        });

        migrationBuilder.CreateIndex("IX_Categories_ExternalId", "Categories", "ExternalId", unique: true, filter: "[ExternalId] IS NOT NULL");

        migrationBuilder.CreateIndex("IX_EventAppearances_EventId_PersonId", "EventAppearances", new[] { "EventId", "PersonId" }, unique: true);

        migrationBuilder.CreateIndex("IX_EventAppearances_PersonId", "EventAppearances", "PersonId");

        migrationBuilder.CreateIndex("IX_Events_ExternalId", "Events", "ExternalId", unique: true, filter: "[ExternalId] IS NOT NULL");

        migrationBuilder.CreateIndex("IX_Events_Slug", "Events", "Slug", unique: true);

        migrationBuilder.CreateIndex("IX_People_ExternalId", "People", "ExternalId", unique: true, filter: "[ExternalId] IS NOT NULL");

        migrationBuilder.CreateIndex("IX_PersonCategories_CategoryId", "PersonCategories", "CategoryId");

        migrationBuilder.CreateIndex("IX_PersonImages_PersonId", "PersonImages", "PersonId");

        migrationBuilder.CreateIndex("IX_Schedules_EventAppearanceId_ExternalId", "Schedules", new[] { "EventAppearanceId", "ExternalId" }, unique: true, filter: "[ExternalId] IS NOT NULL");

        migrationBuilder.CreateIndex("IX_Schedules_VenueLocationId", "Schedules", "VenueLocationId");

        migrationBuilder.CreateIndex("IX_VenueLocations_ExternalId", "VenueLocations", "ExternalId", unique: true, filter: "[ExternalId] IS NOT NULL");
    }

    /// <inheritdoc />
    /// <summary>
    ///     Drops all tables created by this migration.
    /// </summary>
    /// <param name="migrationBuilder">The migration builder.</param>
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable("PersonCategories");

        migrationBuilder.DropTable("PersonImages");

        migrationBuilder.DropTable("Schedules");

        migrationBuilder.DropTable("Categories");

        migrationBuilder.DropTable("EventAppearances");

        migrationBuilder.DropTable("VenueLocations");

        migrationBuilder.DropTable("Events");

        migrationBuilder.DropTable("People");
    }
}