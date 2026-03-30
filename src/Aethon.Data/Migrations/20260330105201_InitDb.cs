using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Aethon.Data.Migrations
{
    /// <inheritdoc />
    public partial class InitDb : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AspNetRoles",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    NormalizedName = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    ConcurrencyStamp = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetRoles", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUsers",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    DisplayName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    IsEnabled = table.Column<bool>(type: "boolean", nullable: false),
                    UserType = table.Column<int>(type: "integer", nullable: false),
                    IsIdentityVerified = table.Column<bool>(type: "boolean", nullable: false),
                    IdentityVerifiedUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    IdentityVerificationNotes = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    IsPhoneNumberVerified = table.Column<bool>(type: "boolean", nullable: false),
                    PhoneNumberVerifiedUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    MustChangePassword = table.Column<bool>(type: "boolean", nullable: false),
                    MustEnableMfa = table.Column<bool>(type: "boolean", nullable: false),
                    LastLoginUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    UserName = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    NormalizedUserName = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    Email = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    NormalizedEmail = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    EmailConfirmed = table.Column<bool>(type: "boolean", nullable: false),
                    PasswordHash = table.Column<string>(type: "text", nullable: true),
                    SecurityStamp = table.Column<string>(type: "text", nullable: true),
                    ConcurrencyStamp = table.Column<string>(type: "text", nullable: true),
                    PhoneNumber = table.Column<string>(type: "text", nullable: true),
                    PhoneNumberConfirmed = table.Column<bool>(type: "boolean", nullable: false),
                    TwoFactorEnabled = table.Column<bool>(type: "boolean", nullable: false),
                    LockoutEnd = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    LockoutEnabled = table.Column<bool>(type: "boolean", nullable: false),
                    AccessFailedCount = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUsers", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Locations",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    DisplayName = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: false),
                    City = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: true),
                    State = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: true),
                    Country = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    CountryCode = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: true),
                    Latitude = table.Column<double>(type: "double precision", nullable: false),
                    Longitude = table.Column<double>(type: "double precision", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    SortOrder = table.Column<int>(type: "integer", nullable: false),
                    CreatedUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Locations", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "StoredFiles",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    FileName = table.Column<string>(type: "character varying(260)", maxLength: 260, nullable: false),
                    OriginalFileName = table.Column<string>(type: "character varying(260)", maxLength: 260, nullable: false),
                    ContentType = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    LengthBytes = table.Column<long>(type: "bigint", nullable: false),
                    StorageProvider = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    StoragePath = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    UploadedByUserId = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    UpdatedByUserId = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StoredFiles", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "SystemLogs",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TimestampUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Level = table.Column<int>(type: "integer", nullable: false),
                    Category = table.Column<string>(type: "text", nullable: false),
                    Message = table.Column<string>(type: "text", nullable: false),
                    Details = table.Column<string>(type: "text", nullable: true),
                    ExceptionType = table.Column<string>(type: "text", nullable: true),
                    ExceptionMessage = table.Column<string>(type: "text", nullable: true),
                    UserId = table.Column<Guid>(type: "uuid", nullable: true),
                    RequestPath = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SystemLogs", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "SystemSettings",
                columns: table => new
                {
                    Key = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Value = table.Column<string>(type: "character varying(8000)", maxLength: 8000, nullable: false),
                    Description = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    UpdatedUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedByUserId = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SystemSettings", x => x.Key);
                });

            migrationBuilder.CreateTable(
                name: "AspNetRoleClaims",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    RoleId = table.Column<Guid>(type: "uuid", nullable: false),
                    ClaimType = table.Column<string>(type: "text", nullable: true),
                    ClaimValue = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetRoleClaims", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AspNetRoleClaims_AspNetRoles_RoleId",
                        column: x => x.RoleId,
                        principalTable: "AspNetRoles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserClaims",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    ClaimType = table.Column<string>(type: "text", nullable: true),
                    ClaimValue = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserClaims", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AspNetUserClaims_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserLogins",
                columns: table => new
                {
                    LoginProvider = table.Column<string>(type: "text", nullable: false),
                    ProviderKey = table.Column<string>(type: "text", nullable: false),
                    ProviderDisplayName = table.Column<string>(type: "text", nullable: true),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserLogins", x => new { x.LoginProvider, x.ProviderKey });
                    table.ForeignKey(
                        name: "FK_AspNetUserLogins_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserRoles",
                columns: table => new
                {
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    RoleId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserRoles", x => new { x.UserId, x.RoleId });
                    table.ForeignKey(
                        name: "FK_AspNetUserRoles_AspNetRoles_RoleId",
                        column: x => x.RoleId,
                        principalTable: "AspNetRoles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_AspNetUserRoles_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserTokens",
                columns: table => new
                {
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    LoginProvider = table.Column<string>(type: "text", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false),
                    Value = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserTokens", x => new { x.UserId, x.LoginProvider, x.Name });
                    table.ForeignKey(
                        name: "FK_AspNetUserTokens_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "IdentityVerificationRequests",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    FullName = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: false),
                    EmailAddress = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    PhoneNumber = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    AdditionalNotes = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    ReviewedByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    ReviewedUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ReviewNotes = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    ReviewerType = table.Column<int>(type: "integer", nullable: true),
                    CreatedUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    UpdatedByUserId = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_IdentityVerificationRequests", x => x.Id);
                    table.ForeignKey(
                        name: "FK_IdentityVerificationRequests_AspNetUsers_ReviewedByUserId",
                        column: x => x.ReviewedByUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_IdentityVerificationRequests_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "JobSeekerProfiles",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    FirstName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    MiddleName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    LastName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    AgeGroup = table.Column<int>(type: "integer", nullable: false),
                    BirthMonth = table.Column<int>(type: "integer", nullable: true),
                    BirthYear = table.Column<int>(type: "integer", nullable: true),
                    AgeConfirmedUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    PhoneNumber = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    WhatsAppNumber = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    Headline = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: true),
                    Summary = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: true),
                    CurrentLocation = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: true),
                    PreferredLocation = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: true),
                    LinkedInUrl = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    OpenToWork = table.Column<bool>(type: "boolean", nullable: false),
                    DesiredSalaryFrom = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: true),
                    DesiredSalaryTo = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: true),
                    DesiredSalaryCurrency = table.Column<int>(type: "integer", nullable: true),
                    WillRelocate = table.Column<bool>(type: "boolean", nullable: true),
                    RequiresSponsorship = table.Column<bool>(type: "boolean", nullable: true),
                    HasWorkRights = table.Column<bool>(type: "boolean", nullable: true),
                    AvailabilityText = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: true),
                    IsPublicProfileEnabled = table.Column<bool>(type: "boolean", nullable: false),
                    IsSearchable = table.Column<bool>(type: "boolean", nullable: false),
                    Slug = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: true),
                    AboutMe = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    ProfileVisibility = table.Column<int>(type: "integer", nullable: false, defaultValue: 1),
                    LinkedInId = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    LinkedInVerifiedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ProfilePictureStoredFileId = table.Column<Guid>(type: "uuid", nullable: true),
                    IsIdVerified = table.Column<bool>(type: "boolean", nullable: false),
                    IsNameLocked = table.Column<bool>(type: "boolean", nullable: false),
                    LastProfileUpdatedUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    UpdatedByUserId = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_JobSeekerProfiles", x => x.Id);
                    table.ForeignKey(
                        name: "FK_JobSeekerProfiles_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

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
                name: "JobSeekerLanguages",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    JobSeekerProfileId = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    AbilityType = table.Column<int>(type: "integer", nullable: false),
                    ProficiencyLevel = table.Column<int>(type: "integer", nullable: true),
                    IsVerified = table.Column<bool>(type: "boolean", nullable: false),
                    VerifiedUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    VerificationNotes = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    CreatedUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    UpdatedByUserId = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_JobSeekerLanguages", x => x.Id);
                    table.ForeignKey(
                        name: "FK_JobSeekerLanguages_JobSeekerProfiles_JobSeekerProfileId",
                        column: x => x.JobSeekerProfileId,
                        principalTable: "JobSeekerProfiles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "JobSeekerNationalities",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    JobSeekerProfileId = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    IsVerified = table.Column<bool>(type: "boolean", nullable: false),
                    VerifiedUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    VerificationNotes = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    CreatedUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    UpdatedByUserId = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_JobSeekerNationalities", x => x.Id);
                    table.ForeignKey(
                        name: "FK_JobSeekerNationalities_JobSeekerProfiles_JobSeekerProfileId",
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
                name: "JobSeekerResumes",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    JobSeekerProfileId = table.Column<Guid>(type: "uuid", nullable: false),
                    StoredFileId = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    Description = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    IsDefault = table.Column<bool>(type: "boolean", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    UpdatedByUserId = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_JobSeekerResumes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_JobSeekerResumes_JobSeekerProfiles_JobSeekerProfileId",
                        column: x => x.JobSeekerProfileId,
                        principalTable: "JobSeekerProfiles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_JobSeekerResumes_StoredFiles_StoredFileId",
                        column: x => x.StoredFileId,
                        principalTable: "StoredFiles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
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

            migrationBuilder.CreateTable(
                name: "ResumeAnalyses",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    JobSeekerResumeId = table.Column<Guid>(type: "uuid", nullable: false),
                    StoredFileId = table.Column<Guid>(type: "uuid", nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    HeadlineSuggestion = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: true),
                    SummaryExtract = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    SkillsJson = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: true),
                    ExperienceLevel = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    YearsExperience = table.Column<int>(type: "integer", nullable: true),
                    AnalysedUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    AnalysisError = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    CreatedUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    UpdatedByUserId = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ResumeAnalyses", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ResumeAnalyses_JobSeekerResumes_JobSeekerResumeId",
                        column: x => x.JobSeekerResumeId,
                        principalTable: "JobSeekerResumes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ResumeAnalyses_StoredFiles_StoredFileId",
                        column: x => x.StoredFileId,
                        principalTable: "StoredFiles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ActivityLogs",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    OrganisationId = table.Column<Guid>(type: "uuid", nullable: true),
                    EntityType = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    EntityId = table.Column<Guid>(type: "uuid", nullable: false),
                    Action = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Summary = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    Details = table.Column<string>(type: "character varying(8000)", maxLength: 8000, nullable: true),
                    PerformedByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    PerformedUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    UpdatedByUserId = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ActivityLogs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ActivityLogs_AspNetUsers_PerformedByUserId",
                        column: x => x.PerformedByUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "CreditConsumptionLogs",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    OrganisationJobCreditId = table.Column<Guid>(type: "uuid", nullable: false),
                    OrganisationId = table.Column<Guid>(type: "uuid", nullable: false),
                    JobId = table.Column<Guid>(type: "uuid", nullable: false),
                    ConsumedByUserId = table.Column<Guid>(type: "uuid", nullable: false),
                    ApprovedByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    QuantityConsumed = table.Column<int>(type: "integer", nullable: false),
                    ConsumedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    UpdatedByUserId = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CreditConsumptionLogs", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "JobApplicationAttachments",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    JobApplicationId = table.Column<Guid>(type: "uuid", nullable: false),
                    StoredFileId = table.Column<Guid>(type: "uuid", nullable: false),
                    Category = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Notes = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    CreatedUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedByUserId = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_JobApplicationAttachments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_JobApplicationAttachments_StoredFiles_StoredFileId",
                        column: x => x.StoredFileId,
                        principalTable: "StoredFiles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "JobApplicationComments",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    JobApplicationId = table.Column<Guid>(type: "uuid", nullable: false),
                    ParentCommentId = table.Column<Guid>(type: "uuid", nullable: true),
                    Content = table.Column<string>(type: "character varying(8000)", maxLength: 8000, nullable: false),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    DeletedUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DeletedByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    CreatedUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    UpdatedByUserId = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_JobApplicationComments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_JobApplicationComments_AspNetUsers_CreatedByUserId",
                        column: x => x.CreatedByUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_JobApplicationComments_AspNetUsers_DeletedByUserId",
                        column: x => x.DeletedByUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_JobApplicationComments_AspNetUsers_UpdatedByUserId",
                        column: x => x.UpdatedByUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_JobApplicationComments_JobApplicationComments_ParentComment~",
                        column: x => x.ParentCommentId,
                        principalTable: "JobApplicationComments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "JobApplicationInterviewInterviewers",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    JobApplicationInterviewId = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    RoleLabel = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    CreatedUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    UpdatedByUserId = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_JobApplicationInterviewInterviewers", x => x.Id);
                    table.ForeignKey(
                        name: "FK_JobApplicationInterviewInterviewers_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "JobApplicationInterviews",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    JobApplicationId = table.Column<Guid>(type: "uuid", nullable: false),
                    Type = table.Column<int>(type: "integer", nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    Title = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: true),
                    Location = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    MeetingUrl = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    Notes = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: true),
                    ScheduledStartUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ScheduledEndUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CompletedUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CancelledUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CancellationReason = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    CreatedUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    UpdatedByUserId = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_JobApplicationInterviews", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "JobApplicationNotes",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    JobApplicationId = table.Column<Guid>(type: "uuid", nullable: false),
                    Content = table.Column<string>(type: "character varying(8000)", maxLength: 8000, nullable: false),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    DeletedUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DeletedByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    CreatedUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    UpdatedByUserId = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_JobApplicationNotes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_JobApplicationNotes_AspNetUsers_CreatedByUserId",
                        column: x => x.CreatedByUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_JobApplicationNotes_AspNetUsers_DeletedByUserId",
                        column: x => x.DeletedByUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_JobApplicationNotes_AspNetUsers_UpdatedByUserId",
                        column: x => x.UpdatedByUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "JobApplications",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    JobId = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    StatusReason = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    ResumeFileId = table.Column<Guid>(type: "uuid", nullable: true),
                    CoverLetter = table.Column<string>(type: "character varying(20000)", maxLength: 20000, nullable: true),
                    AssignedRecruiterUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    AssignedRecruiterUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    AssignedToUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    SubmittedUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    LastStatusChangedUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    LastActivityUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    Source = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    SourceDetail = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: true),
                    SourceReference = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: true),
                    InternalSummaryNotes = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: true),
                    ScreeningSummary = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: true),
                    Rating = table.Column<decimal>(type: "numeric(5,2)", precision: 5, scale: 2, nullable: true),
                    Recommendation = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    Tags = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    CandidatePhoneNumber = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    CandidateLocationText = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: true),
                    AvailabilityText = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: true),
                    SalaryExpectation = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: true),
                    SalaryExpectationCurrency = table.Column<int>(type: "integer", nullable: true),
                    WillRelocate = table.Column<bool>(type: "boolean", nullable: true),
                    RequiresSponsorship = table.Column<bool>(type: "boolean", nullable: true),
                    HasWorkRights = table.Column<bool>(type: "boolean", nullable: true),
                    AcceptedPrivacyPolicy = table.Column<bool>(type: "boolean", nullable: false),
                    AcceptedPrivacyPolicyUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    IsWithdrawn = table.Column<bool>(type: "boolean", nullable: false),
                    WithdrawnUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    WithdrawalReason = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    WithdrawnByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    IsRejected = table.Column<bool>(type: "boolean", nullable: false),
                    RejectedUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    RejectionReason = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    RejectedByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    IsHired = table.Column<bool>(type: "boolean", nullable: false),
                    HiredUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    IsDuplicate = table.Column<bool>(type: "boolean", nullable: false),
                    DuplicateOfApplicationId = table.Column<Guid>(type: "uuid", nullable: true),
                    IsArchived = table.Column<bool>(type: "boolean", nullable: false),
                    ArchivedUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ExternalReference = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: true),
                    ScreeningAnswersJson = table.Column<string>(type: "text", nullable: true),
                    IsNotSuitable = table.Column<bool>(type: "boolean", nullable: false),
                    NotSuitableReasons = table.Column<string>(type: "text", nullable: true),
                    CreatedUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    UpdatedByUserId = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_JobApplications", x => x.Id);
                    table.ForeignKey(
                        name: "FK_JobApplications_AspNetUsers_AssignedRecruiterUserId",
                        column: x => x.AssignedRecruiterUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_JobApplications_AspNetUsers_AssignedToUserId",
                        column: x => x.AssignedToUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_JobApplications_AspNetUsers_RejectedByUserId",
                        column: x => x.RejectedByUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_JobApplications_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_JobApplications_AspNetUsers_WithdrawnByUserId",
                        column: x => x.WithdrawnByUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_JobApplications_JobApplications_DuplicateOfApplicationId",
                        column: x => x.DuplicateOfApplicationId,
                        principalTable: "JobApplications",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_JobApplications_StoredFiles_ResumeFileId",
                        column: x => x.ResumeFileId,
                        principalTable: "StoredFiles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "JobApplicationStatusHistory",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    JobApplicationId = table.Column<Guid>(type: "uuid", nullable: false),
                    FromStatus = table.Column<int>(type: "integer", nullable: true),
                    ToStatus = table.Column<int>(type: "integer", nullable: false),
                    Reason = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    Notes = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: true),
                    ChangedByUserId = table.Column<Guid>(type: "uuid", nullable: false),
                    ChangedUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    UpdatedByUserId = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_JobApplicationStatusHistory", x => x.Id);
                    table.ForeignKey(
                        name: "FK_JobApplicationStatusHistory_AspNetUsers_ChangedByUserId",
                        column: x => x.ChangedByUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_JobApplicationStatusHistory_JobApplications_JobApplicationId",
                        column: x => x.JobApplicationId,
                        principalTable: "JobApplications",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Jobs",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    OwnedByOrganisationId = table.Column<Guid>(type: "uuid", nullable: false),
                    ManagedByOrganisationId = table.Column<Guid>(type: "uuid", nullable: true),
                    OrganisationRecruitmentPartnershipId = table.Column<Guid>(type: "uuid", nullable: true),
                    CreatedByIdentityUserId = table.Column<Guid>(type: "uuid", nullable: false),
                    ManagedByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    CreatedByType = table.Column<int>(type: "integer", nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    StatusReason = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    Visibility = table.Column<int>(type: "integer", nullable: false),
                    Title = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: false),
                    ReferenceCode = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    ExternalReference = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: true),
                    Department = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: true),
                    LocationText = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: true),
                    LocationCity = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: true),
                    LocationState = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: true),
                    LocationCountry = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    LocationCountryCode = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: true),
                    LocationLatitude = table.Column<double>(type: "double precision", nullable: true),
                    LocationLongitude = table.Column<double>(type: "double precision", nullable: true),
                    LocationPlaceId = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    WorkplaceType = table.Column<int>(type: "integer", nullable: false),
                    EmploymentType = table.Column<int>(type: "integer", nullable: false),
                    Description = table.Column<string>(type: "character varying(20000)", maxLength: 20000, nullable: false),
                    Requirements = table.Column<string>(type: "character varying(12000)", maxLength: 12000, nullable: true),
                    Benefits = table.Column<string>(type: "character varying(8000)", maxLength: 8000, nullable: true),
                    Summary = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    SalaryFrom = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: true),
                    SalaryTo = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: true),
                    SalaryCurrency = table.Column<int>(type: "integer", nullable: true),
                    PublishedUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ApplyByUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ClosedUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    SubmittedForApprovalUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ApprovedByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    ApprovedUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ExternalApplicationUrl = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    ApplicationEmail = table.Column<string>(type: "character varying(320)", maxLength: 320, nullable: true),
                    CreatedForUnclaimedCompany = table.Column<bool>(type: "boolean", nullable: false),
                    Category = table.Column<int>(type: "integer", nullable: true),
                    Regions = table.Column<string>(type: "text", nullable: true),
                    Countries = table.Column<string>(type: "text", nullable: true),
                    PostingExpiresUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    PostingTier = table.Column<int>(type: "integer", nullable: false),
                    HasAiCandidateMatching = table.Column<bool>(type: "boolean", nullable: false),
                    IncludeCompanyLogo = table.Column<bool>(type: "boolean", nullable: false),
                    IsHighlighted = table.Column<bool>(type: "boolean", nullable: false),
                    HighlightColour = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    StickyUntilUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    BenefitsTags = table.Column<string>(type: "text", nullable: true),
                    ApplicationSpecialRequirements = table.Column<string>(type: "text", nullable: true),
                    HasCommission = table.Column<bool>(type: "boolean", nullable: false),
                    OteFrom = table.Column<decimal>(type: "numeric", nullable: true),
                    OteTo = table.Column<decimal>(type: "numeric", nullable: true),
                    IsImmediateStart = table.Column<bool>(type: "boolean", nullable: false),
                    IsSuitableForSchoolLeavers = table.Column<bool>(type: "boolean", nullable: false),
                    IsSchoolLeaverTargeted = table.Column<bool>(type: "boolean", nullable: false),
                    VideoYouTubeId = table.Column<string>(type: "text", nullable: true),
                    VideoVimeoId = table.Column<string>(type: "text", nullable: true),
                    ScreeningQuestionsJson = table.Column<string>(type: "text", nullable: true),
                    Keywords = table.Column<string>(type: "text", nullable: true),
                    PoNumber = table.Column<string>(type: "text", nullable: true),
                    IsImported = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    AllowAutoMatch = table.Column<bool>(type: "boolean", nullable: false),
                    ShortUrlCode = table.Column<string>(type: "text", nullable: true),
                    CreatedUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    UpdatedByUserId = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Jobs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Jobs_AspNetUsers_CreatedByIdentityUserId",
                        column: x => x.CreatedByIdentityUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Jobs_AspNetUsers_ManagedByUserId",
                        column: x => x.ManagedByUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "JobSyndicationRecords",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    JobId = table.Column<Guid>(type: "uuid", nullable: false),
                    Provider = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Status = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    ExternalRef = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    SubmittedUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    LastAttemptUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    LastErrorMessage = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    CreatedUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    UpdatedByUserId = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_JobSyndicationRecords", x => x.Id);
                    table.ForeignKey(
                        name: "FK_JobSyndicationRecords_Jobs_JobId",
                        column: x => x.JobId,
                        principalTable: "Jobs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "OrganisationClaimRequests",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    OrganisationId = table.Column<Guid>(type: "uuid", nullable: false),
                    RequestedByUserId = table.Column<Guid>(type: "uuid", nullable: false),
                    EmailUsed = table.Column<string>(type: "character varying(320)", maxLength: 320, nullable: false),
                    EmailDomain = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    VerificationMethod = table.Column<int>(type: "integer", nullable: false),
                    VerificationToken = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    VerifiedUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ApprovedByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    ApprovedUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    RejectionReason = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    CreatedUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    UpdatedByUserId = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OrganisationClaimRequests", x => x.Id);
                    table.ForeignKey(
                        name: "FK_OrganisationClaimRequests_AspNetUsers_RequestedByUserId",
                        column: x => x.RequestedByUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "OrganisationDomains",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    OrganisationId = table.Column<Guid>(type: "uuid", nullable: false),
                    Domain = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    NormalizedDomain = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    IsPrimary = table.Column<bool>(type: "boolean", nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    VerificationMethod = table.Column<int>(type: "integer", nullable: false),
                    TrustLevel = table.Column<int>(type: "integer", nullable: false),
                    VerificationToken = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    VerificationDnsRecordName = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    VerificationDnsRecordValue = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    VerificationEmailAddress = table.Column<string>(type: "character varying(320)", maxLength: 320, nullable: true),
                    VerificationRequestedUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    VerifiedUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    VerifiedByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    CreatedUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    UpdatedByUserId = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OrganisationDomains", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Organisations",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Type = table.Column<int>(type: "integer", nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    ClaimStatus = table.Column<int>(type: "integer", nullable: false),
                    Name = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: false),
                    NormalizedName = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: false),
                    LegalName = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: true),
                    WebsiteUrl = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    Slug = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: true),
                    LogoUrl = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    Summary = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: true),
                    PublicLocationText = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: true),
                    LocationCity = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: true),
                    LocationState = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: true),
                    LocationCountry = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    LocationCountryCode = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: true),
                    LocationLatitude = table.Column<double>(type: "double precision", nullable: true),
                    LocationLongitude = table.Column<double>(type: "double precision", nullable: true),
                    LocationPlaceId = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    PublicContactEmail = table.Column<string>(type: "character varying(320)", maxLength: 320, nullable: true),
                    PublicContactPhone = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    IsPublicProfileEnabled = table.Column<bool>(type: "boolean", nullable: false),
                    IsEqualOpportunityEmployer = table.Column<bool>(type: "boolean", nullable: false),
                    IsAccessibleWorkplace = table.Column<bool>(type: "boolean", nullable: false),
                    CompanySize = table.Column<int>(type: "integer", nullable: true),
                    Industry = table.Column<int>(type: "integer", nullable: true),
                    BannerImageUrl = table.Column<string>(type: "text", nullable: true),
                    LinkedInUrl = table.Column<string>(type: "text", nullable: true),
                    TwitterHandle = table.Column<string>(type: "text", nullable: true),
                    FacebookUrl = table.Column<string>(type: "text", nullable: true),
                    TikTokHandle = table.Column<string>(type: "text", nullable: true),
                    InstagramHandle = table.Column<string>(type: "text", nullable: true),
                    YouTubeUrl = table.Column<string>(type: "text", nullable: true),
                    PrimaryDomainId = table.Column<Guid>(type: "uuid", nullable: true),
                    IsProvisionedByRecruiter = table.Column<bool>(type: "boolean", nullable: false),
                    ClaimedByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    ClaimedUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    PrimaryContactName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    PrimaryContactEmail = table.Column<string>(type: "character varying(320)", maxLength: 320, nullable: true),
                    PrimaryContactPhoneDialCode = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: true),
                    PrimaryContactPhone = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    PublicContactPhoneDialCode = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: true),
                    RegisteredAddressLine1 = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    RegisteredAddressLine2 = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    RegisteredCity = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: true),
                    RegisteredState = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: true),
                    RegisteredPostcode = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    RegisteredCountry = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: true),
                    RegisteredCountryCode = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: true),
                    TaxRegistrationNumber = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    BusinessRegistrationNumber = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    VerificationTier = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    VerifiedUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    VerifiedByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    VerificationPaidAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    VerificationExpiresAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    VerificationStripeEventId = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    VerificationPendingTier = table.Column<int>(type: "integer", nullable: true),
                    StripeCustomerId = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    CreatedUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    UpdatedByUserId = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Organisations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Organisations_OrganisationDomains_PrimaryDomainId",
                        column: x => x.PrimaryDomainId,
                        principalTable: "OrganisationDomains",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "OrganisationInvitations",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Type = table.Column<int>(type: "integer", nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    OrganisationId = table.Column<Guid>(type: "uuid", nullable: false),
                    Email = table.Column<string>(type: "character varying(320)", maxLength: 320, nullable: false),
                    NormalizedEmail = table.Column<string>(type: "character varying(320)", maxLength: 320, nullable: false),
                    EmailDomain = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    Token = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    ExpiresUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CompanyRole = table.Column<int>(type: "integer", nullable: true),
                    RecruiterRole = table.Column<int>(type: "integer", nullable: true),
                    AllowClaimAsOwner = table.Column<bool>(type: "boolean", nullable: false),
                    AcceptedByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    AcceptedUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    UpdatedByUserId = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OrganisationInvitations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_OrganisationInvitations_Organisations_OrganisationId",
                        column: x => x.OrganisationId,
                        principalTable: "Organisations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "OrganisationMemberProfiles",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    OrganisationId = table.Column<Guid>(type: "uuid", nullable: false),
                    Slug = table.Column<string>(type: "character varying(60)", maxLength: 60, nullable: true),
                    JobTitle = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    Bio = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    ProfilePictureUrl = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    PublicEmail = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    PublicPhone = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    LinkedInUrl = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    IsPublicProfileEnabled = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    UpdatedByUserId = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OrganisationMemberProfiles", x => x.Id);
                    table.ForeignKey(
                        name: "FK_OrganisationMemberProfiles_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_OrganisationMemberProfiles_Organisations_OrganisationId",
                        column: x => x.OrganisationId,
                        principalTable: "Organisations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "OrganisationMemberships",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    OrganisationId = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    CompanyRole = table.Column<int>(type: "integer", nullable: true),
                    RecruiterRole = table.Column<int>(type: "integer", nullable: true),
                    IsOwner = table.Column<bool>(type: "boolean", nullable: false),
                    JoinedUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    LeftUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    UpdatedByUserId = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OrganisationMemberships", x => x.Id);
                    table.ForeignKey(
                        name: "FK_OrganisationMemberships_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_OrganisationMemberships_Organisations_OrganisationId",
                        column: x => x.OrganisationId,
                        principalTable: "Organisations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "OrganisationRecruitmentPartnerships",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    CompanyOrganisationId = table.Column<Guid>(type: "uuid", nullable: false),
                    RecruiterOrganisationId = table.Column<Guid>(type: "uuid", nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    Scope = table.Column<int>(type: "integer", nullable: false),
                    RecruiterCanCreateUnclaimedCompanyJobs = table.Column<bool>(type: "boolean", nullable: false),
                    RecruiterCanPublishJobs = table.Column<bool>(type: "boolean", nullable: false),
                    RecruiterCanManageCandidates = table.Column<bool>(type: "boolean", nullable: false),
                    RequestedByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    ApprovedByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    ApprovedUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    Notes = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: true),
                    CreatedUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    UpdatedByUserId = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OrganisationRecruitmentPartnerships", x => x.Id);
                    table.ForeignKey(
                        name: "FK_OrganisationRecruitmentPartnerships_Organisations_CompanyOr~",
                        column: x => x.CompanyOrganisationId,
                        principalTable: "Organisations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_OrganisationRecruitmentPartnerships_Organisations_Recruiter~",
                        column: x => x.RecruiterOrganisationId,
                        principalTable: "Organisations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "StripePaymentEvents",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    StripeEventId = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    EventType = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    AmountTotal = table.Column<long>(type: "bigint", nullable: true),
                    Currency = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: true),
                    CustomerEmail = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    PayloadJson = table.Column<string>(type: "text", nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    InternalNotes = table.Column<string>(type: "text", nullable: true),
                    CompletedByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    CompletedUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    OrganisationId = table.Column<Guid>(type: "uuid", nullable: true),
                    PurchaseType = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    ProductId = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    PriceId = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    PurchaseMetaJson = table.Column<string>(type: "text", nullable: true),
                    CreatedUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    UpdatedByUserId = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StripePaymentEvents", x => x.Id);
                    table.ForeignKey(
                        name: "FK_StripePaymentEvents_Organisations_OrganisationId",
                        column: x => x.OrganisationId,
                        principalTable: "Organisations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "WebhookSubscriptions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    OrganisationId = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    EndpointUrl = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: false),
                    Secret = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    EventsCsv = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedByUserId = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WebhookSubscriptions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_WebhookSubscriptions_Organisations_OrganisationId",
                        column: x => x.OrganisationId,
                        principalTable: "Organisations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "OrganisationJobCredits",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    OrganisationId = table.Column<Guid>(type: "uuid", nullable: false),
                    CreditType = table.Column<int>(type: "integer", nullable: false),
                    Source = table.Column<int>(type: "integer", nullable: false),
                    QuantityOriginal = table.Column<int>(type: "integer", nullable: false),
                    QuantityRemaining = table.Column<int>(type: "integer", nullable: false),
                    ExpiresAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ConvertedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    StripePaymentEventId = table.Column<Guid>(type: "uuid", nullable: true),
                    GrantedByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    GrantNote = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    CreatedUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    UpdatedByUserId = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OrganisationJobCredits", x => x.Id);
                    table.ForeignKey(
                        name: "FK_OrganisationJobCredits_Organisations_OrganisationId",
                        column: x => x.OrganisationId,
                        principalTable: "Organisations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_OrganisationJobCredits_StripePaymentEvents_StripePaymentEve~",
                        column: x => x.StripePaymentEventId,
                        principalTable: "StripePaymentEvents",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "WebhookDeliveries",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    WebhookSubscriptionId = table.Column<Guid>(type: "uuid", nullable: false),
                    EventType = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    PayloadJson = table.Column<string>(type: "text", nullable: false),
                    Status = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    AttemptCount = table.Column<int>(type: "integer", nullable: false),
                    CreatedUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    LastAttemptUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    LastError = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WebhookDeliveries", x => x.Id);
                    table.ForeignKey(
                        name: "FK_WebhookDeliveries_WebhookSubscriptions_WebhookSubscriptionId",
                        column: x => x.WebhookSubscriptionId,
                        principalTable: "WebhookSubscriptions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ActivityLogs_EntityType_EntityId",
                table: "ActivityLogs",
                columns: new[] { "EntityType", "EntityId" });

            migrationBuilder.CreateIndex(
                name: "IX_ActivityLogs_OrganisationId",
                table: "ActivityLogs",
                column: "OrganisationId");

            migrationBuilder.CreateIndex(
                name: "IX_ActivityLogs_PerformedByUserId",
                table: "ActivityLogs",
                column: "PerformedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_ActivityLogs_PerformedUtc",
                table: "ActivityLogs",
                column: "PerformedUtc");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetRoleClaims_RoleId",
                table: "AspNetRoleClaims",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "RoleNameIndex",
                table: "AspNetRoles",
                column: "NormalizedName",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUserClaims_UserId",
                table: "AspNetUserClaims",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUserLogins_UserId",
                table: "AspNetUserLogins",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUserRoles_RoleId",
                table: "AspNetUserRoles",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "EmailIndex",
                table: "AspNetUsers",
                column: "NormalizedEmail");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUsers_IsEnabled",
                table: "AspNetUsers",
                column: "IsEnabled");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUsers_IsIdentityVerified",
                table: "AspNetUsers",
                column: "IsIdentityVerified");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUsers_IsPhoneNumberVerified",
                table: "AspNetUsers",
                column: "IsPhoneNumberVerified");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUsers_UserType",
                table: "AspNetUsers",
                column: "UserType");

            migrationBuilder.CreateIndex(
                name: "UserNameIndex",
                table: "AspNetUsers",
                column: "NormalizedUserName",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_CreditConsumptionLogs_JobId",
                table: "CreditConsumptionLogs",
                column: "JobId");

            migrationBuilder.CreateIndex(
                name: "IX_CreditConsumptionLogs_OrganisationId",
                table: "CreditConsumptionLogs",
                column: "OrganisationId");

            migrationBuilder.CreateIndex(
                name: "IX_CreditConsumptionLogs_OrganisationJobCreditId",
                table: "CreditConsumptionLogs",
                column: "OrganisationJobCreditId");

            migrationBuilder.CreateIndex(
                name: "IX_IdentityVerificationRequests_ReviewedByUserId",
                table: "IdentityVerificationRequests",
                column: "ReviewedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_IdentityVerificationRequests_Status",
                table: "IdentityVerificationRequests",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_IdentityVerificationRequests_UserId",
                table: "IdentityVerificationRequests",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_IdentityVerificationRequests_UserId_Status",
                table: "IdentityVerificationRequests",
                columns: new[] { "UserId", "Status" },
                unique: true,
                filter: "\"Status\" = 1");

            migrationBuilder.CreateIndex(
                name: "IX_JobApplicationAttachments_JobApplicationId",
                table: "JobApplicationAttachments",
                column: "JobApplicationId");

            migrationBuilder.CreateIndex(
                name: "IX_JobApplicationAttachments_StoredFileId",
                table: "JobApplicationAttachments",
                column: "StoredFileId");

            migrationBuilder.CreateIndex(
                name: "IX_JobApplicationComments_CreatedByUserId",
                table: "JobApplicationComments",
                column: "CreatedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_JobApplicationComments_DeletedByUserId",
                table: "JobApplicationComments",
                column: "DeletedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_JobApplicationComments_JobApplicationId",
                table: "JobApplicationComments",
                column: "JobApplicationId");

            migrationBuilder.CreateIndex(
                name: "IX_JobApplicationComments_ParentCommentId",
                table: "JobApplicationComments",
                column: "ParentCommentId");

            migrationBuilder.CreateIndex(
                name: "IX_JobApplicationComments_UpdatedByUserId",
                table: "JobApplicationComments",
                column: "UpdatedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_JobApplicationInterviewInterviewers_JobApplicationInterview~",
                table: "JobApplicationInterviewInterviewers",
                columns: new[] { "JobApplicationInterviewId", "UserId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_JobApplicationInterviewInterviewers_UserId",
                table: "JobApplicationInterviewInterviewers",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_JobApplicationInterviews_JobApplicationId",
                table: "JobApplicationInterviews",
                column: "JobApplicationId");

            migrationBuilder.CreateIndex(
                name: "IX_JobApplicationInterviews_JobApplicationId_Status",
                table: "JobApplicationInterviews",
                columns: new[] { "JobApplicationId", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_JobApplicationInterviews_ScheduledStartUtc",
                table: "JobApplicationInterviews",
                column: "ScheduledStartUtc");

            migrationBuilder.CreateIndex(
                name: "IX_JobApplicationNotes_CreatedByUserId",
                table: "JobApplicationNotes",
                column: "CreatedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_JobApplicationNotes_DeletedByUserId",
                table: "JobApplicationNotes",
                column: "DeletedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_JobApplicationNotes_JobApplicationId",
                table: "JobApplicationNotes",
                column: "JobApplicationId");

            migrationBuilder.CreateIndex(
                name: "IX_JobApplicationNotes_UpdatedByUserId",
                table: "JobApplicationNotes",
                column: "UpdatedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_JobApplications_AssignedRecruiterUserId",
                table: "JobApplications",
                column: "AssignedRecruiterUserId");

            migrationBuilder.CreateIndex(
                name: "IX_JobApplications_AssignedToUserId",
                table: "JobApplications",
                column: "AssignedToUserId");

            migrationBuilder.CreateIndex(
                name: "IX_JobApplications_DuplicateOfApplicationId",
                table: "JobApplications",
                column: "DuplicateOfApplicationId");

            migrationBuilder.CreateIndex(
                name: "IX_JobApplications_IsArchived",
                table: "JobApplications",
                column: "IsArchived");

            migrationBuilder.CreateIndex(
                name: "IX_JobApplications_IsHired",
                table: "JobApplications",
                column: "IsHired");

            migrationBuilder.CreateIndex(
                name: "IX_JobApplications_IsRejected",
                table: "JobApplications",
                column: "IsRejected");

            migrationBuilder.CreateIndex(
                name: "IX_JobApplications_IsWithdrawn",
                table: "JobApplications",
                column: "IsWithdrawn");

            migrationBuilder.CreateIndex(
                name: "IX_JobApplications_JobId_Status",
                table: "JobApplications",
                columns: new[] { "JobId", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_JobApplications_JobId_Status_SubmittedUtc",
                table: "JobApplications",
                columns: new[] { "JobId", "Status", "SubmittedUtc" });

            migrationBuilder.CreateIndex(
                name: "IX_JobApplications_JobId_SubmittedUtc",
                table: "JobApplications",
                columns: new[] { "JobId", "SubmittedUtc" });

            migrationBuilder.CreateIndex(
                name: "IX_JobApplications_JobId_UserId",
                table: "JobApplications",
                columns: new[] { "JobId", "UserId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_JobApplications_LastActivityUtc",
                table: "JobApplications",
                column: "LastActivityUtc");

            migrationBuilder.CreateIndex(
                name: "IX_JobApplications_LastStatusChangedUtc",
                table: "JobApplications",
                column: "LastStatusChangedUtc");

            migrationBuilder.CreateIndex(
                name: "IX_JobApplications_RejectedByUserId",
                table: "JobApplications",
                column: "RejectedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_JobApplications_ResumeFileId",
                table: "JobApplications",
                column: "ResumeFileId");

            migrationBuilder.CreateIndex(
                name: "IX_JobApplications_SubmittedUtc",
                table: "JobApplications",
                column: "SubmittedUtc");

            migrationBuilder.CreateIndex(
                name: "IX_JobApplications_UserId",
                table: "JobApplications",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_JobApplications_UserId_Status_SubmittedUtc",
                table: "JobApplications",
                columns: new[] { "UserId", "Status", "SubmittedUtc" });

            migrationBuilder.CreateIndex(
                name: "IX_JobApplications_UserId_SubmittedUtc",
                table: "JobApplications",
                columns: new[] { "UserId", "SubmittedUtc" });

            migrationBuilder.CreateIndex(
                name: "IX_JobApplications_WithdrawnByUserId",
                table: "JobApplications",
                column: "WithdrawnByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_JobApplicationStatusHistory_ChangedByUserId",
                table: "JobApplicationStatusHistory",
                column: "ChangedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_JobApplicationStatusHistory_ChangedUtc",
                table: "JobApplicationStatusHistory",
                column: "ChangedUtc");

            migrationBuilder.CreateIndex(
                name: "IX_JobApplicationStatusHistory_JobApplicationId",
                table: "JobApplicationStatusHistory",
                column: "JobApplicationId");

            migrationBuilder.CreateIndex(
                name: "IX_JobApplicationStatusHistory_JobApplicationId_ChangedUtc",
                table: "JobApplicationStatusHistory",
                columns: new[] { "JobApplicationId", "ChangedUtc" });

            migrationBuilder.CreateIndex(
                name: "IX_Jobs_CreatedByIdentityUserId",
                table: "Jobs",
                column: "CreatedByIdentityUserId");

            migrationBuilder.CreateIndex(
                name: "IX_Jobs_ManagedByOrganisationId",
                table: "Jobs",
                column: "ManagedByOrganisationId");

            migrationBuilder.CreateIndex(
                name: "IX_Jobs_ManagedByUserId",
                table: "Jobs",
                column: "ManagedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_Jobs_OrganisationRecruitmentPartnershipId",
                table: "Jobs",
                column: "OrganisationRecruitmentPartnershipId");

            migrationBuilder.CreateIndex(
                name: "IX_Jobs_OwnedByOrganisationId",
                table: "Jobs",
                column: "OwnedByOrganisationId");

            migrationBuilder.CreateIndex(
                name: "IX_Jobs_OwnedByOrganisationId_Status",
                table: "Jobs",
                columns: new[] { "OwnedByOrganisationId", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_Jobs_OwnedByOrganisationId_Visibility",
                table: "Jobs",
                columns: new[] { "OwnedByOrganisationId", "Visibility" });

            migrationBuilder.CreateIndex(
                name: "IX_Jobs_PublishedUtc",
                table: "Jobs",
                column: "PublishedUtc");

            migrationBuilder.CreateIndex(
                name: "IX_Jobs_ReferenceCode",
                table: "Jobs",
                column: "ReferenceCode");

            migrationBuilder.CreateIndex(
                name: "IX_JobSeekerCertificates_JobSeekerProfileId",
                table: "JobSeekerCertificates",
                column: "JobSeekerProfileId");

            migrationBuilder.CreateIndex(
                name: "IX_JobSeekerLanguages_JobSeekerProfileId",
                table: "JobSeekerLanguages",
                column: "JobSeekerProfileId");

            migrationBuilder.CreateIndex(
                name: "IX_JobSeekerLanguages_JobSeekerProfileId_Name_AbilityType",
                table: "JobSeekerLanguages",
                columns: new[] { "JobSeekerProfileId", "Name", "AbilityType" });

            migrationBuilder.CreateIndex(
                name: "IX_JobSeekerNationalities_JobSeekerProfileId",
                table: "JobSeekerNationalities",
                column: "JobSeekerProfileId");

            migrationBuilder.CreateIndex(
                name: "IX_JobSeekerNationalities_JobSeekerProfileId_Name",
                table: "JobSeekerNationalities",
                columns: new[] { "JobSeekerProfileId", "Name" });

            migrationBuilder.CreateIndex(
                name: "IX_JobSeekerProfiles_IsPublicProfileEnabled",
                table: "JobSeekerProfiles",
                column: "IsPublicProfileEnabled");

            migrationBuilder.CreateIndex(
                name: "IX_JobSeekerProfiles_IsSearchable",
                table: "JobSeekerProfiles",
                column: "IsSearchable");

            migrationBuilder.CreateIndex(
                name: "IX_JobSeekerProfiles_OpenToWork",
                table: "JobSeekerProfiles",
                column: "OpenToWork");

            migrationBuilder.CreateIndex(
                name: "IX_JobSeekerProfiles_Slug",
                table: "JobSeekerProfiles",
                column: "Slug",
                unique: true,
                filter: "\"Slug\" IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_JobSeekerProfiles_UserId",
                table: "JobSeekerProfiles",
                column: "UserId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_JobSeekerQualifications_JobSeekerProfileId",
                table: "JobSeekerQualifications",
                column: "JobSeekerProfileId");

            migrationBuilder.CreateIndex(
                name: "IX_JobSeekerResumes_JobSeekerProfileId",
                table: "JobSeekerResumes",
                column: "JobSeekerProfileId");

            migrationBuilder.CreateIndex(
                name: "IX_JobSeekerResumes_JobSeekerProfileId_IsActive_IsDefault",
                table: "JobSeekerResumes",
                columns: new[] { "JobSeekerProfileId", "IsActive", "IsDefault" });

            migrationBuilder.CreateIndex(
                name: "IX_JobSeekerResumes_JobSeekerProfileId_IsDefault",
                table: "JobSeekerResumes",
                columns: new[] { "JobSeekerProfileId", "IsDefault" });

            migrationBuilder.CreateIndex(
                name: "IX_JobSeekerResumes_JobSeekerProfileId_StoredFileId_IsActive",
                table: "JobSeekerResumes",
                columns: new[] { "JobSeekerProfileId", "StoredFileId", "IsActive" });

            migrationBuilder.CreateIndex(
                name: "IX_JobSeekerResumes_StoredFileId",
                table: "JobSeekerResumes",
                column: "StoredFileId");

            migrationBuilder.CreateIndex(
                name: "IX_JobSeekerSkills_JobSeekerProfileId",
                table: "JobSeekerSkills",
                column: "JobSeekerProfileId");

            migrationBuilder.CreateIndex(
                name: "IX_JobSeekerWorkExperiences_JobSeekerProfileId",
                table: "JobSeekerWorkExperiences",
                column: "JobSeekerProfileId");

            migrationBuilder.CreateIndex(
                name: "IX_JobSyndicationRecords_JobId_Provider",
                table: "JobSyndicationRecords",
                columns: new[] { "JobId", "Provider" });

            migrationBuilder.CreateIndex(
                name: "IX_JobSyndicationRecords_SubmittedUtc",
                table: "JobSyndicationRecords",
                column: "SubmittedUtc");

            migrationBuilder.CreateIndex(
                name: "IX_Locations_DisplayName",
                table: "Locations",
                column: "DisplayName");

            migrationBuilder.CreateIndex(
                name: "IX_Locations_IsActive_SortOrder",
                table: "Locations",
                columns: new[] { "IsActive", "SortOrder" });

            migrationBuilder.CreateIndex(
                name: "IX_OrganisationClaimRequests_EmailDomain",
                table: "OrganisationClaimRequests",
                column: "EmailDomain");

            migrationBuilder.CreateIndex(
                name: "IX_OrganisationClaimRequests_OrganisationId",
                table: "OrganisationClaimRequests",
                column: "OrganisationId");

            migrationBuilder.CreateIndex(
                name: "IX_OrganisationClaimRequests_RequestedByUserId",
                table: "OrganisationClaimRequests",
                column: "RequestedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_OrganisationClaimRequests_Status",
                table: "OrganisationClaimRequests",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_OrganisationClaimRequests_VerificationToken",
                table: "OrganisationClaimRequests",
                column: "VerificationToken");

            migrationBuilder.CreateIndex(
                name: "IX_OrganisationDomains_NormalizedDomain",
                table: "OrganisationDomains",
                column: "NormalizedDomain",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_OrganisationDomains_OrganisationId",
                table: "OrganisationDomains",
                column: "OrganisationId");

            migrationBuilder.CreateIndex(
                name: "IX_OrganisationDomains_OrganisationId_IsPrimary",
                table: "OrganisationDomains",
                columns: new[] { "OrganisationId", "IsPrimary" });

            migrationBuilder.CreateIndex(
                name: "IX_OrganisationDomains_Status",
                table: "OrganisationDomains",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_OrganisationInvitations_EmailDomain",
                table: "OrganisationInvitations",
                column: "EmailDomain");

            migrationBuilder.CreateIndex(
                name: "IX_OrganisationInvitations_ExpiresUtc",
                table: "OrganisationInvitations",
                column: "ExpiresUtc");

            migrationBuilder.CreateIndex(
                name: "IX_OrganisationInvitations_NormalizedEmail",
                table: "OrganisationInvitations",
                column: "NormalizedEmail");

            migrationBuilder.CreateIndex(
                name: "IX_OrganisationInvitations_OrganisationId",
                table: "OrganisationInvitations",
                column: "OrganisationId");

            migrationBuilder.CreateIndex(
                name: "IX_OrganisationInvitations_Status",
                table: "OrganisationInvitations",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_OrganisationInvitations_Token",
                table: "OrganisationInvitations",
                column: "Token",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_OrganisationJobCredits_OrganisationId",
                table: "OrganisationJobCredits",
                column: "OrganisationId");

            migrationBuilder.CreateIndex(
                name: "IX_OrganisationJobCredits_OrganisationId_CreditType_QuantityRe~",
                table: "OrganisationJobCredits",
                columns: new[] { "OrganisationId", "CreditType", "QuantityRemaining" });

            migrationBuilder.CreateIndex(
                name: "IX_OrganisationJobCredits_StripePaymentEventId",
                table: "OrganisationJobCredits",
                column: "StripePaymentEventId");

            migrationBuilder.CreateIndex(
                name: "IX_OrganisationMemberProfiles_OrganisationId_IsPublicProfileEn~",
                table: "OrganisationMemberProfiles",
                columns: new[] { "OrganisationId", "IsPublicProfileEnabled" });

            migrationBuilder.CreateIndex(
                name: "IX_OrganisationMemberProfiles_OrganisationId_Slug",
                table: "OrganisationMemberProfiles",
                columns: new[] { "OrganisationId", "Slug" },
                unique: true,
                filter: "\"Slug\" IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_OrganisationMemberProfiles_OrganisationId_UserId",
                table: "OrganisationMemberProfiles",
                columns: new[] { "OrganisationId", "UserId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_OrganisationMemberProfiles_UserId",
                table: "OrganisationMemberProfiles",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_OrganisationMemberships_OrganisationId_Status",
                table: "OrganisationMemberships",
                columns: new[] { "OrganisationId", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_OrganisationMemberships_OrganisationId_UserId",
                table: "OrganisationMemberships",
                columns: new[] { "OrganisationId", "UserId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_OrganisationMemberships_UserId",
                table: "OrganisationMemberships",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_OrganisationMemberships_UserId_OrganisationId_Status",
                table: "OrganisationMemberships",
                columns: new[] { "UserId", "OrganisationId", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_OrganisationRecruitmentPartnerships_ApprovedByUserId",
                table: "OrganisationRecruitmentPartnerships",
                column: "ApprovedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_OrganisationRecruitmentPartnerships_CompanyOrganisationId_R~",
                table: "OrganisationRecruitmentPartnerships",
                columns: new[] { "CompanyOrganisationId", "RecruiterOrganisationId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_OrganisationRecruitmentPartnerships_RecruiterOrganisationId",
                table: "OrganisationRecruitmentPartnerships",
                column: "RecruiterOrganisationId");

            migrationBuilder.CreateIndex(
                name: "IX_OrganisationRecruitmentPartnerships_RequestedByUserId",
                table: "OrganisationRecruitmentPartnerships",
                column: "RequestedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_OrganisationRecruitmentPartnerships_Status",
                table: "OrganisationRecruitmentPartnerships",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_Organisations_NormalizedName",
                table: "Organisations",
                column: "NormalizedName");

            migrationBuilder.CreateIndex(
                name: "IX_Organisations_PrimaryDomainId",
                table: "Organisations",
                column: "PrimaryDomainId");

            migrationBuilder.CreateIndex(
                name: "IX_Organisations_Slug",
                table: "Organisations",
                column: "Slug",
                unique: true,
                filter: "\"Slug\" IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_Organisations_Type_Status",
                table: "Organisations",
                columns: new[] { "Type", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_Organisations_VerificationTier",
                table: "Organisations",
                column: "VerificationTier");

            migrationBuilder.CreateIndex(
                name: "IX_ResumeAnalyses_JobSeekerResumeId",
                table: "ResumeAnalyses",
                column: "JobSeekerResumeId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ResumeAnalyses_Status",
                table: "ResumeAnalyses",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_ResumeAnalyses_StoredFileId",
                table: "ResumeAnalyses",
                column: "StoredFileId");

            migrationBuilder.CreateIndex(
                name: "IX_StoredFiles_StoragePath",
                table: "StoredFiles",
                column: "StoragePath",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_StoredFiles_UploadedByUserId",
                table: "StoredFiles",
                column: "UploadedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_StoredFiles_UploadedByUserId_CreatedUtc",
                table: "StoredFiles",
                columns: new[] { "UploadedByUserId", "CreatedUtc" });

            migrationBuilder.CreateIndex(
                name: "IX_StripePaymentEvents_OrganisationId",
                table: "StripePaymentEvents",
                column: "OrganisationId");

            migrationBuilder.CreateIndex(
                name: "IX_StripePaymentEvents_StripeEventId",
                table: "StripePaymentEvents",
                column: "StripeEventId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_WebhookDeliveries_WebhookSubscriptionId",
                table: "WebhookDeliveries",
                column: "WebhookSubscriptionId");

            migrationBuilder.CreateIndex(
                name: "IX_WebhookSubscriptions_OrganisationId",
                table: "WebhookSubscriptions",
                column: "OrganisationId");

            migrationBuilder.AddForeignKey(
                name: "FK_ActivityLogs_Organisations_OrganisationId",
                table: "ActivityLogs",
                column: "OrganisationId",
                principalTable: "Organisations",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_CreditConsumptionLogs_Jobs_JobId",
                table: "CreditConsumptionLogs",
                column: "JobId",
                principalTable: "Jobs",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_CreditConsumptionLogs_OrganisationJobCredits_OrganisationJo~",
                table: "CreditConsumptionLogs",
                column: "OrganisationJobCreditId",
                principalTable: "OrganisationJobCredits",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_JobApplicationAttachments_JobApplications_JobApplicationId",
                table: "JobApplicationAttachments",
                column: "JobApplicationId",
                principalTable: "JobApplications",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_JobApplicationComments_JobApplications_JobApplicationId",
                table: "JobApplicationComments",
                column: "JobApplicationId",
                principalTable: "JobApplications",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_JobApplicationInterviewInterviewers_JobApplicationInterview~",
                table: "JobApplicationInterviewInterviewers",
                column: "JobApplicationInterviewId",
                principalTable: "JobApplicationInterviews",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_JobApplicationInterviews_JobApplications_JobApplicationId",
                table: "JobApplicationInterviews",
                column: "JobApplicationId",
                principalTable: "JobApplications",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_JobApplicationNotes_JobApplications_JobApplicationId",
                table: "JobApplicationNotes",
                column: "JobApplicationId",
                principalTable: "JobApplications",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_JobApplications_Jobs_JobId",
                table: "JobApplications",
                column: "JobId",
                principalTable: "Jobs",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Jobs_OrganisationRecruitmentPartnerships_OrganisationRecrui~",
                table: "Jobs",
                column: "OrganisationRecruitmentPartnershipId",
                principalTable: "OrganisationRecruitmentPartnerships",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Jobs_Organisations_ManagedByOrganisationId",
                table: "Jobs",
                column: "ManagedByOrganisationId",
                principalTable: "Organisations",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Jobs_Organisations_OwnedByOrganisationId",
                table: "Jobs",
                column: "OwnedByOrganisationId",
                principalTable: "Organisations",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_OrganisationClaimRequests_Organisations_OrganisationId",
                table: "OrganisationClaimRequests",
                column: "OrganisationId",
                principalTable: "Organisations",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_OrganisationDomains_Organisations_OrganisationId",
                table: "OrganisationDomains",
                column: "OrganisationId",
                principalTable: "Organisations",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_OrganisationDomains_Organisations_OrganisationId",
                table: "OrganisationDomains");

            migrationBuilder.DropTable(
                name: "ActivityLogs");

            migrationBuilder.DropTable(
                name: "AspNetRoleClaims");

            migrationBuilder.DropTable(
                name: "AspNetUserClaims");

            migrationBuilder.DropTable(
                name: "AspNetUserLogins");

            migrationBuilder.DropTable(
                name: "AspNetUserRoles");

            migrationBuilder.DropTable(
                name: "AspNetUserTokens");

            migrationBuilder.DropTable(
                name: "CreditConsumptionLogs");

            migrationBuilder.DropTable(
                name: "IdentityVerificationRequests");

            migrationBuilder.DropTable(
                name: "JobApplicationAttachments");

            migrationBuilder.DropTable(
                name: "JobApplicationComments");

            migrationBuilder.DropTable(
                name: "JobApplicationInterviewInterviewers");

            migrationBuilder.DropTable(
                name: "JobApplicationNotes");

            migrationBuilder.DropTable(
                name: "JobApplicationStatusHistory");

            migrationBuilder.DropTable(
                name: "JobSeekerCertificates");

            migrationBuilder.DropTable(
                name: "JobSeekerLanguages");

            migrationBuilder.DropTable(
                name: "JobSeekerNationalities");

            migrationBuilder.DropTable(
                name: "JobSeekerQualifications");

            migrationBuilder.DropTable(
                name: "JobSeekerSkills");

            migrationBuilder.DropTable(
                name: "JobSeekerWorkExperiences");

            migrationBuilder.DropTable(
                name: "JobSyndicationRecords");

            migrationBuilder.DropTable(
                name: "Locations");

            migrationBuilder.DropTable(
                name: "OrganisationClaimRequests");

            migrationBuilder.DropTable(
                name: "OrganisationInvitations");

            migrationBuilder.DropTable(
                name: "OrganisationMemberProfiles");

            migrationBuilder.DropTable(
                name: "OrganisationMemberships");

            migrationBuilder.DropTable(
                name: "ResumeAnalyses");

            migrationBuilder.DropTable(
                name: "SystemLogs");

            migrationBuilder.DropTable(
                name: "SystemSettings");

            migrationBuilder.DropTable(
                name: "WebhookDeliveries");

            migrationBuilder.DropTable(
                name: "AspNetRoles");

            migrationBuilder.DropTable(
                name: "OrganisationJobCredits");

            migrationBuilder.DropTable(
                name: "JobApplicationInterviews");

            migrationBuilder.DropTable(
                name: "JobSeekerResumes");

            migrationBuilder.DropTable(
                name: "WebhookSubscriptions");

            migrationBuilder.DropTable(
                name: "StripePaymentEvents");

            migrationBuilder.DropTable(
                name: "JobApplications");

            migrationBuilder.DropTable(
                name: "JobSeekerProfiles");

            migrationBuilder.DropTable(
                name: "Jobs");

            migrationBuilder.DropTable(
                name: "StoredFiles");

            migrationBuilder.DropTable(
                name: "AspNetUsers");

            migrationBuilder.DropTable(
                name: "OrganisationRecruitmentPartnerships");

            migrationBuilder.DropTable(
                name: "Organisations");

            migrationBuilder.DropTable(
                name: "OrganisationDomains");
        }
    }
}
