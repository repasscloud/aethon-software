using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Aethon.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddVerificationTier : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Organisations_IsVerified",
                table: "Organisations");

            migrationBuilder.DropColumn(
                name: "IsVerified",
                table: "Organisations");

            migrationBuilder.AddColumn<int>(
                name: "VerificationTier",
                table: "Organisations",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_Organisations_VerificationTier",
                table: "Organisations",
                column: "VerificationTier");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Organisations_VerificationTier",
                table: "Organisations");

            migrationBuilder.DropColumn(
                name: "VerificationTier",
                table: "Organisations");

            migrationBuilder.AddColumn<bool>(
                name: "IsVerified",
                table: "Organisations",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateIndex(
                name: "IX_Organisations_IsVerified",
                table: "Organisations",
                column: "IsVerified");
        }
    }
}
