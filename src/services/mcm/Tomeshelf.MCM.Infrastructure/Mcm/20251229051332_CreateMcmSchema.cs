using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Tomeshelf.MCM.Infrastructure.Mcm;

/// <inheritdoc />
public partial class CreateMcmSchema : Migration
{
    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable("GuestSocials");

        migrationBuilder.DropTable("GuestInformation");

        migrationBuilder.DropTable("Guests");

        migrationBuilder.DropTable("Events");
    }

    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable("Events", table => new
        {
            Id = table.Column<string>("nvarchar(450)", nullable: false),
            Name = table.Column<string>("nvarchar(30)", maxLength: 30, nullable: false),
            UpdatedAt = table.Column<DateTimeOffset>("datetimeoffset", nullable: false)
        }, constraints: table =>
        {
            table.PrimaryKey("PK_Events", x => x.Id);
        });

        migrationBuilder.CreateTable("Guests", table => new
        {
            Id = table.Column<Guid>("uniqueidentifier", nullable: false),
            IsDeleted = table.Column<bool>("bit", nullable: false),
            GuestInfoId = table.Column<Guid>("uniqueidentifier", nullable: false),
            EventId = table.Column<string>("nvarchar(450)", nullable: true)
        }, constraints: table =>
        {
            table.PrimaryKey("PK_Guests", x => x.Id);
            table.ForeignKey("FK_Guests_Events_EventId", x => x.EventId, "Events", "Id");
        });

        migrationBuilder.CreateTable("GuestInformation", table => new
        {
            Id = table.Column<Guid>("uniqueidentifier", nullable: false),
            FirstName = table.Column<string>("nvarchar(max)", nullable: true),
            LastName = table.Column<string>("nvarchar(max)", nullable: true),
            Bio = table.Column<string>("nvarchar(max)", nullable: true),
            KnownFor = table.Column<string>("nvarchar(max)", nullable: true),
            Category = table.Column<string>("nvarchar(max)", nullable: true),
            DaysAppearing = table.Column<string>("nvarchar(max)", nullable: true),
            ImageUrl = table.Column<string>("nvarchar(max)", nullable: true),
            GuestId = table.Column<Guid>("uniqueidentifier", nullable: false)
        }, constraints: table =>
        {
            table.PrimaryKey("PK_GuestInformation", x => x.Id);
            table.ForeignKey("FK_GuestInformation_Guests_GuestId", x => x.GuestId, "Guests", "Id", onDelete: ReferentialAction.Cascade);
        });

        migrationBuilder.CreateTable("GuestSocials", table => new
        {
            Id = table.Column<Guid>("uniqueidentifier", nullable: false),
            Twitter = table.Column<string>("nvarchar(max)", nullable: true),
            Facebook = table.Column<string>("nvarchar(max)", nullable: true),
            Instagram = table.Column<string>("nvarchar(max)", nullable: true),
            Imdb = table.Column<string>("nvarchar(max)", nullable: true),
            YouTube = table.Column<string>("nvarchar(max)", nullable: true),
            Twitch = table.Column<string>("nvarchar(max)", nullable: true),
            Snapchat = table.Column<string>("nvarchar(max)", nullable: true),
            DeviantArt = table.Column<string>("nvarchar(max)", nullable: true),
            Tumblr = table.Column<string>("nvarchar(max)", nullable: true),
            Fandom = table.Column<string>("nvarchar(max)", nullable: true),
            GuestInfoId = table.Column<Guid>("uniqueidentifier", nullable: false)
        }, constraints: table =>
        {
            table.PrimaryKey("PK_GuestSocials", x => x.Id);
            table.ForeignKey("FK_GuestSocials_GuestInformation_GuestInfoId", x => x.GuestInfoId, "GuestInformation", "Id", onDelete: ReferentialAction.Cascade);
        });

        migrationBuilder.CreateIndex("IX_GuestInformation_GuestId", "GuestInformation", "GuestId", unique: true);

        migrationBuilder.CreateIndex("IX_Guests_EventId", "Guests", "EventId");

        migrationBuilder.CreateIndex("IX_GuestSocials_GuestInfoId", "GuestSocials", "GuestInfoId", unique: true);
    }
}