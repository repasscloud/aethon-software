using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Aethon.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddRegionsCountriesExpiry : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Region",
                table: "Jobs");

            migrationBuilder.AddColumn<DateTime>(
                name: "PostingExpiresUtc",
                table: "Jobs",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Regions",
                table: "Jobs",
                type: "text",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PostingExpiresUtc",
                table: "Jobs");

            migrationBuilder.DropColumn(
                name: "Regions",
                table: "Jobs");

            migrationBuilder.AddColumn<int>(
                name: "Region",
                table: "Jobs",
                type: "integer",
                nullable: true);
        }
    }
}
