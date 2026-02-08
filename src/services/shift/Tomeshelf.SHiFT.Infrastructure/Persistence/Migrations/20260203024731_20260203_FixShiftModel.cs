using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Tomeshelf.SHiFT.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class _20260203_FixShiftModel : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("UPDATE [ShiftSettings] SET [EncryptedPassword] = '' WHERE [EncryptedPassword] IS NULL");
            migrationBuilder.Sql("UPDATE [ShiftSettings] SET [Email] = '' WHERE [Email] IS NULL");
            migrationBuilder.Sql("UPDATE [ShiftSettings] SET [DefaultService] = '' WHERE [DefaultService] IS NULL");

            migrationBuilder.AlterColumn<string>(
                name: "EncryptedPassword",
                table: "ShiftSettings",
                type: "nvarchar(4000)",
                maxLength: 4000,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(4000)",
                oldMaxLength: 4000,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Email",
                table: "ShiftSettings",
                type: "nvarchar(256)",
                maxLength: 256,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(256)",
                oldMaxLength: 256,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "DefaultService",
                table: "ShiftSettings",
                type: "nvarchar(32)",
                maxLength: 32,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(32)",
                oldMaxLength: 32,
                oldNullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "EncryptedPassword",
                table: "ShiftSettings",
                type: "nvarchar(4000)",
                maxLength: 4000,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(4000)",
                oldMaxLength: 4000);

            migrationBuilder.AlterColumn<string>(
                name: "Email",
                table: "ShiftSettings",
                type: "nvarchar(256)",
                maxLength: 256,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(256)",
                oldMaxLength: 256);

            migrationBuilder.AlterColumn<string>(
                name: "DefaultService",
                table: "ShiftSettings",
                type: "nvarchar(32)",
                maxLength: 32,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(32)",
                oldMaxLength: 32);
        }
    }
}
