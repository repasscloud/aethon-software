using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Aethon.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddOrganisationExtendedProfile : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "PrimaryContactName",
                table: "Organisations",
                type: "character varying(200)",
                maxLength: 200,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(150)",
                oldMaxLength: 150,
                oldNullable: true);

            migrationBuilder.AddColumn<string>(
                name: "BusinessRegistrationNumber",
                table: "Organisations",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PrimaryContactPhoneDialCode",
                table: "Organisations",
                type: "character varying(10)",
                maxLength: 10,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PublicContactPhoneDialCode",
                table: "Organisations",
                type: "character varying(10)",
                maxLength: 10,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "RegisteredAddressLine1",
                table: "Organisations",
                type: "character varying(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "RegisteredAddressLine2",
                table: "Organisations",
                type: "character varying(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "RegisteredCity",
                table: "Organisations",
                type: "character varying(150)",
                maxLength: 150,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "RegisteredCountry",
                table: "Organisations",
                type: "character varying(150)",
                maxLength: 150,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "RegisteredCountryCode",
                table: "Organisations",
                type: "character varying(10)",
                maxLength: 10,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "RegisteredPostcode",
                table: "Organisations",
                type: "character varying(20)",
                maxLength: 20,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "RegisteredState",
                table: "Organisations",
                type: "character varying(150)",
                maxLength: 150,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "TaxRegistrationNumber",
                table: "Organisations",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "BusinessRegistrationNumber",
                table: "Organisations");

            migrationBuilder.DropColumn(
                name: "PrimaryContactPhoneDialCode",
                table: "Organisations");

            migrationBuilder.DropColumn(
                name: "PublicContactPhoneDialCode",
                table: "Organisations");

            migrationBuilder.DropColumn(
                name: "RegisteredAddressLine1",
                table: "Organisations");

            migrationBuilder.DropColumn(
                name: "RegisteredAddressLine2",
                table: "Organisations");

            migrationBuilder.DropColumn(
                name: "RegisteredCity",
                table: "Organisations");

            migrationBuilder.DropColumn(
                name: "RegisteredCountry",
                table: "Organisations");

            migrationBuilder.DropColumn(
                name: "RegisteredCountryCode",
                table: "Organisations");

            migrationBuilder.DropColumn(
                name: "RegisteredPostcode",
                table: "Organisations");

            migrationBuilder.DropColumn(
                name: "RegisteredState",
                table: "Organisations");

            migrationBuilder.DropColumn(
                name: "TaxRegistrationNumber",
                table: "Organisations");

            migrationBuilder.AlterColumn<string>(
                name: "PrimaryContactName",
                table: "Organisations",
                type: "character varying(150)",
                maxLength: 150,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(200)",
                oldMaxLength: 200,
                oldNullable: true);
        }
    }
}
