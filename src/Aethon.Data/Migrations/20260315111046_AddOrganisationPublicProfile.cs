using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Aethon.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddOrganisationPublicProfile : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsPublicProfileEnabled",
                table: "Organisations",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "LogoUrl",
                table: "Organisations",
                type: "character varying(1000)",
                maxLength: 1000,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PublicContactEmail",
                table: "Organisations",
                type: "character varying(320)",
                maxLength: 320,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PublicContactPhone",
                table: "Organisations",
                type: "character varying(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PublicLocationText",
                table: "Organisations",
                type: "character varying(250)",
                maxLength: 250,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Slug",
                table: "Organisations",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Summary",
                table: "Organisations",
                type: "character varying(4000)",
                maxLength: 4000,
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Organisations_Slug",
                table: "Organisations",
                column: "Slug",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Organisations_Slug",
                table: "Organisations");

            migrationBuilder.DropColumn(
                name: "IsPublicProfileEnabled",
                table: "Organisations");

            migrationBuilder.DropColumn(
                name: "LogoUrl",
                table: "Organisations");

            migrationBuilder.DropColumn(
                name: "PublicContactEmail",
                table: "Organisations");

            migrationBuilder.DropColumn(
                name: "PublicContactPhone",
                table: "Organisations");

            migrationBuilder.DropColumn(
                name: "PublicLocationText",
                table: "Organisations");

            migrationBuilder.DropColumn(
                name: "Slug",
                table: "Organisations");

            migrationBuilder.DropColumn(
                name: "Summary",
                table: "Organisations");
        }
    }
}
