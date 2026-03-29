using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Aethon.Data.Migrations
{
    /// <inheritdoc />
    public partial class AgePolicyAndAccountDeletion : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DateOfBirth",
                table: "JobSeekerProfiles");

            migrationBuilder.AddColumn<DateTime>(
                name: "AgeConfirmedUtc",
                table: "JobSeekerProfiles",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "AgeGroup",
                table: "JobSeekerProfiles",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "BirthMonth",
                table: "JobSeekerProfiles",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "BirthYear",
                table: "JobSeekerProfiles",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsSchoolLeaverTargeted",
                table: "Jobs",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsSuitableForSchoolLeavers",
                table: "Jobs",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "LastLoginUtc",
                table: "AspNetUsers",
                type: "timestamp with time zone",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AgeConfirmedUtc",
                table: "JobSeekerProfiles");

            migrationBuilder.DropColumn(
                name: "AgeGroup",
                table: "JobSeekerProfiles");

            migrationBuilder.DropColumn(
                name: "BirthMonth",
                table: "JobSeekerProfiles");

            migrationBuilder.DropColumn(
                name: "BirthYear",
                table: "JobSeekerProfiles");

            migrationBuilder.DropColumn(
                name: "IsSchoolLeaverTargeted",
                table: "Jobs");

            migrationBuilder.DropColumn(
                name: "IsSuitableForSchoolLeavers",
                table: "Jobs");

            migrationBuilder.DropColumn(
                name: "LastLoginUtc",
                table: "AspNetUsers");

            migrationBuilder.AddColumn<DateOnly>(
                name: "DateOfBirth",
                table: "JobSeekerProfiles",
                type: "date",
                nullable: true);
        }
    }
}
