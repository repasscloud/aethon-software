using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Aethon.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddVerificationPendingTier : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "VerificationPendingTier",
                table: "Organisations",
                type: "integer",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "VerificationPendingTier",
                table: "Organisations");
        }
    }
}
