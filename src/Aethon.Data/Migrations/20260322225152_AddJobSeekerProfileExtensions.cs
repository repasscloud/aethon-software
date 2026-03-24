using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Aethon.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddJobSeekerProfileExtensions : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsIdVerified",
                table: "JobSeekerProfiles",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsNameLocked",
                table: "JobSeekerProfiles",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "LinkedInId",
                table: "JobSeekerProfiles",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "LinkedInVerifiedAt",
                table: "JobSeekerProfiles",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "ProfilePictureStoredFileId",
                table: "JobSeekerProfiles",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "JobSeekerCertificates",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    JobSeekerProfileId = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    IssuingOrganisation = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    IssuedMonth = table.Column<int>(type: "integer", nullable: true),
                    IssuedYear = table.Column<int>(type: "integer", nullable: true),
                    ExpiryYear = table.Column<int>(type: "integer", nullable: true),
                    CredentialId = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    CredentialUrl = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    SortOrder = table.Column<int>(type: "integer", nullable: false),
                    CreatedUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    UpdatedByUserId = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_JobSeekerCertificates", x => x.Id);
                    table.ForeignKey(
                        name: "FK_JobSeekerCertificates_JobSeekerProfiles_JobSeekerProfileId",
                        column: x => x.JobSeekerProfileId,
                        principalTable: "JobSeekerProfiles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "JobSeekerQualifications",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    JobSeekerProfileId = table.Column<Guid>(type: "uuid", nullable: false),
                    Title = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Institution = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    CompletedYear = table.Column<int>(type: "integer", nullable: true),
                    Description = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    SortOrder = table.Column<int>(type: "integer", nullable: false),
                    CreatedUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    UpdatedByUserId = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_JobSeekerQualifications", x => x.Id);
                    table.ForeignKey(
                        name: "FK_JobSeekerQualifications_JobSeekerProfiles_JobSeekerProfileId",
                        column: x => x.JobSeekerProfileId,
                        principalTable: "JobSeekerProfiles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "JobSeekerSkills",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    JobSeekerProfileId = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    SkillLevel = table.Column<int>(type: "integer", nullable: true),
                    SortOrder = table.Column<int>(type: "integer", nullable: false),
                    CreatedUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    UpdatedByUserId = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_JobSeekerSkills", x => x.Id);
                    table.ForeignKey(
                        name: "FK_JobSeekerSkills_JobSeekerProfiles_JobSeekerProfileId",
                        column: x => x.JobSeekerProfileId,
                        principalTable: "JobSeekerProfiles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "JobSeekerWorkExperiences",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    JobSeekerProfileId = table.Column<Guid>(type: "uuid", nullable: false),
                    JobTitle = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    EmployerName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    StartMonth = table.Column<int>(type: "integer", nullable: false),
                    StartYear = table.Column<int>(type: "integer", nullable: false),
                    EndMonth = table.Column<int>(type: "integer", nullable: true),
                    EndYear = table.Column<int>(type: "integer", nullable: true),
                    IsCurrent = table.Column<bool>(type: "boolean", nullable: false),
                    Description = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: true),
                    SortOrder = table.Column<int>(type: "integer", nullable: false),
                    CreatedUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    UpdatedByUserId = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_JobSeekerWorkExperiences", x => x.Id);
                    table.ForeignKey(
                        name: "FK_JobSeekerWorkExperiences_JobSeekerProfiles_JobSeekerProfile~",
                        column: x => x.JobSeekerProfileId,
                        principalTable: "JobSeekerProfiles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_JobSeekerCertificates_JobSeekerProfileId",
                table: "JobSeekerCertificates",
                column: "JobSeekerProfileId");

            migrationBuilder.CreateIndex(
                name: "IX_JobSeekerQualifications_JobSeekerProfileId",
                table: "JobSeekerQualifications",
                column: "JobSeekerProfileId");

            migrationBuilder.CreateIndex(
                name: "IX_JobSeekerSkills_JobSeekerProfileId",
                table: "JobSeekerSkills",
                column: "JobSeekerProfileId");

            migrationBuilder.CreateIndex(
                name: "IX_JobSeekerWorkExperiences_JobSeekerProfileId",
                table: "JobSeekerWorkExperiences",
                column: "JobSeekerProfileId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "JobSeekerCertificates");

            migrationBuilder.DropTable(
                name: "JobSeekerQualifications");

            migrationBuilder.DropTable(
                name: "JobSeekerSkills");

            migrationBuilder.DropTable(
                name: "JobSeekerWorkExperiences");

            migrationBuilder.DropColumn(
                name: "IsIdVerified",
                table: "JobSeekerProfiles");

            migrationBuilder.DropColumn(
                name: "IsNameLocked",
                table: "JobSeekerProfiles");

            migrationBuilder.DropColumn(
                name: "LinkedInId",
                table: "JobSeekerProfiles");

            migrationBuilder.DropColumn(
                name: "LinkedInVerifiedAt",
                table: "JobSeekerProfiles");

            migrationBuilder.DropColumn(
                name: "ProfilePictureStoredFileId",
                table: "JobSeekerProfiles");
        }
    }
}
