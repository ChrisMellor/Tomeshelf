using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Tomeshelf.Infrastructure.Persistence.Migrations.Mcm
{
    /// <inheritdoc />
    public partial class CreateMcmSchema : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Events",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Events", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Guests",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    GuestInfoId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    EventId = table.Column<string>(type: "nvarchar(450)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Guests", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Guests_Events_EventId",
                        column: x => x.EventId,
                        principalTable: "Events",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "GuestInformation",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    FirstName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    LastName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Bio = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    KnownFor = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Category = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DaysAppearing = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ImageUrl = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    GuestId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GuestInformation", x => x.Id);
                    table.ForeignKey(
                        name: "FK_GuestInformation_Guests_GuestId",
                        column: x => x.GuestId,
                        principalTable: "Guests",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "GuestSocials",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Twitter = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Facebook = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Instagram = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Imdb = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    YouTube = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Twitch = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Snapchat = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DeviantArt = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Tumblr = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Fandom = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    GuestInfoId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GuestSocials", x => x.Id);
                    table.ForeignKey(
                        name: "FK_GuestSocials_GuestInformation_GuestInfoId",
                        column: x => x.GuestInfoId,
                        principalTable: "GuestInformation",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_GuestInformation_GuestId",
                table: "GuestInformation",
                column: "GuestId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Guests_EventId",
                table: "Guests",
                column: "EventId");

            migrationBuilder.CreateIndex(
                name: "IX_GuestSocials_GuestInfoId",
                table: "GuestSocials",
                column: "GuestInfoId",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "GuestSocials");

            migrationBuilder.DropTable(
                name: "GuestInformation");

            migrationBuilder.DropTable(
                name: "Guests");

            migrationBuilder.DropTable(
                name: "Events");
        }
    }
}
