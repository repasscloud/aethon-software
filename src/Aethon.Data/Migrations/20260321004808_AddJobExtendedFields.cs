using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Aethon.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddJobExtendedFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "AllowAutoMatch",
                table: "Jobs",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "ApplicationSpecialRequirements",
                table: "Jobs",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "BenefitsTags",
                table: "Jobs",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Category",
                table: "Jobs",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Countries",
                table: "Jobs",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IncludeCompanyLogo",
                table: "Jobs",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsHighlighted",
                table: "Jobs",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "Keywords",
                table: "Jobs",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PoNumber",
                table: "Jobs",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Region",
                table: "Jobs",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ShortUrlCode",
                table: "Jobs",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "StickyUntilUtc",
                table: "Jobs",
                type: "timestamp with time zone",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AllowAutoMatch",
                table: "Jobs");

            migrationBuilder.DropColumn(
                name: "ApplicationSpecialRequirements",
                table: "Jobs");

            migrationBuilder.DropColumn(
                name: "BenefitsTags",
                table: "Jobs");

            migrationBuilder.DropColumn(
                name: "Category",
                table: "Jobs");

            migrationBuilder.DropColumn(
                name: "Countries",
                table: "Jobs");

            migrationBuilder.DropColumn(
                name: "IncludeCompanyLogo",
                table: "Jobs");

            migrationBuilder.DropColumn(
                name: "IsHighlighted",
                table: "Jobs");

            migrationBuilder.DropColumn(
                name: "Keywords",
                table: "Jobs");

            migrationBuilder.DropColumn(
                name: "PoNumber",
                table: "Jobs");

            migrationBuilder.DropColumn(
                name: "Region",
                table: "Jobs");

            migrationBuilder.DropColumn(
                name: "ShortUrlCode",
                table: "Jobs");

            migrationBuilder.DropColumn(
                name: "StickyUntilUtc",
                table: "Jobs");
        }
    }
}
