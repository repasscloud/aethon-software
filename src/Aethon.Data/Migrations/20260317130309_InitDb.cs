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
                    DisplayName = table.Column<string>(type: "text", nullable: false),
                    IsEnabled = table.Column<bool>(type: "boolean", nullable: false),
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
                name: "tenants",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Slug = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    IsEnabled = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tenants", x => x.Id);
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
                name: "JobSeekerProfiles",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    Headline = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: true),
                    Summary = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: true),
                    CurrentLocation = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: true),
                    PreferredLocation = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: true),
                    LinkedInUrl = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    ResumeFileId = table.Column<Guid>(type: "uuid", nullable: true),
                    OpenToWork = table.Column<bool>(type: "boolean", nullable: false),
                    DesiredSalaryFrom = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: true),
                    DesiredSalaryTo = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: true),
                    DesiredSalaryCurrency = table.Column<int>(type: "integer", nullable: true),
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
                name: "user_tenant_memberships",
                columns: table => new
                {
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    RoleCode = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    IsDefault = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_user_tenant_memberships", x => new { x.UserId, x.TenantId });
                    table.ForeignKey(
                        name: "FK_user_tenant_memberships_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_user_tenant_memberships_tenants_TenantId",
                        column: x => x.TenantId,
                        principalTable: "tenants",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "CompanyRecruiterRelationships",
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
                    table.PrimaryKey("PK_CompanyRecruiterRelationships", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "JobApplications",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    JobId = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    ResumeFileId = table.Column<Guid>(type: "uuid", nullable: true),
                    CoverLetter = table.Column<string>(type: "character varying(20000)", maxLength: 20000, nullable: true),
                    AssignedRecruiterUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    AssignedRecruiterUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    SubmittedUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    LastStatusChangedUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    Source = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    Notes = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: true),
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
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_JobApplications_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Jobs",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    OwnedByOrganisationId = table.Column<Guid>(type: "uuid", nullable: false),
                    ManagedByOrganisationId = table.Column<Guid>(type: "uuid", nullable: false),
                    CompanyRecruiterRelationshipId = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedByIdentityUserId = table.Column<Guid>(type: "uuid", nullable: false),
                    ManagedByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    CreatedByType = table.Column<int>(type: "integer", nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    StatusReason = table.Column<string>(type: "text", nullable: true),
                    Visibility = table.Column<int>(type: "integer", nullable: false),
                    Title = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: false),
                    ReferenceCode = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    Department = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: true),
                    LocationText = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: true),
                    WorkplaceType = table.Column<int>(type: "integer", nullable: false),
                    EmploymentType = table.Column<int>(type: "integer", nullable: false),
                    Description = table.Column<string>(type: "character varying(20000)", maxLength: 20000, nullable: false),
                    Requirements = table.Column<string>(type: "character varying(20000)", maxLength: 20000, nullable: true),
                    Benefits = table.Column<string>(type: "character varying(20000)", maxLength: 20000, nullable: true),
                    SalaryFrom = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: true),
                    SalaryTo = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: true),
                    SalaryCurrency = table.Column<int>(type: "integer", nullable: true),
                    PublishedUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ClosedUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    SubmittedForApprovalUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ApprovedByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    ApprovedUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    Summary = table.Column<string>(type: "text", nullable: true),
                    CreatedForUnclaimedCompany = table.Column<bool>(type: "boolean", nullable: false),
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
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Jobs_CompanyRecruiterRelationships_CompanyRecruiterRelation~",
                        column: x => x.CompanyRecruiterRelationshipId,
                        principalTable: "CompanyRecruiterRelationships",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
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
                    OrganisationId = table.Column<Guid>(type: "uuid", maxLength: 64, nullable: false),
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
                    VerifiedByUserId = table.Column<Guid>(type: "uuid", maxLength: 64, nullable: true),
                    CreatedUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedByUserId = table.Column<Guid>(type: "uuid", maxLength: 64, nullable: true),
                    UpdatedByUserId = table.Column<Guid>(type: "uuid", maxLength: 64, nullable: true)
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
                    WebsiteUrl = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    Slug = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    LogoUrl = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    Summary = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: true),
                    PublicLocationText = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: true),
                    PublicContactEmail = table.Column<string>(type: "character varying(320)", maxLength: 320, nullable: true),
                    PublicContactPhone = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    IsPublicProfileEnabled = table.Column<bool>(type: "boolean", nullable: false),
                    PrimaryDomainId = table.Column<Guid>(type: "uuid", nullable: true),
                    IsProvisionedByRecruiter = table.Column<bool>(type: "boolean", nullable: false),
                    ClaimedByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    ClaimedUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    PrimaryContactName = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: true),
                    PrimaryContactEmail = table.Column<string>(type: "character varying(320)", maxLength: 320, nullable: true),
                    PrimaryContactPhone = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
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
                name: "UserNameIndex",
                table: "AspNetUsers",
                column: "NormalizedUserName",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_CompanyRecruiterRelationships_CompanyOrganisationId_Recruit~",
                table: "CompanyRecruiterRelationships",
                columns: new[] { "CompanyOrganisationId", "RecruiterOrganisationId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_CompanyRecruiterRelationships_RecruiterOrganisationId",
                table: "CompanyRecruiterRelationships",
                column: "RecruiterOrganisationId");

            migrationBuilder.CreateIndex(
                name: "IX_JobApplications_AssignedRecruiterUserId",
                table: "JobApplications",
                column: "AssignedRecruiterUserId");

            migrationBuilder.CreateIndex(
                name: "IX_JobApplications_JobId_Status",
                table: "JobApplications",
                columns: new[] { "JobId", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_JobApplications_JobId_UserId",
                table: "JobApplications",
                columns: new[] { "JobId", "UserId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_JobApplications_UserId",
                table: "JobApplications",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_Jobs_CompanyRecruiterRelationshipId",
                table: "Jobs",
                column: "CompanyRecruiterRelationshipId");

            migrationBuilder.CreateIndex(
                name: "IX_Jobs_CreatedByIdentityUserId",
                table: "Jobs",
                column: "CreatedByIdentityUserId");

            migrationBuilder.CreateIndex(
                name: "IX_Jobs_ManagedByOrganisationId_Status",
                table: "Jobs",
                columns: new[] { "ManagedByOrganisationId", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_Jobs_ManagedByUserId",
                table: "Jobs",
                column: "ManagedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_Jobs_OwnedByOrganisationId_Status",
                table: "Jobs",
                columns: new[] { "OwnedByOrganisationId", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_Jobs_ReferenceCode",
                table: "Jobs",
                column: "ReferenceCode");

            migrationBuilder.CreateIndex(
                name: "IX_Jobs_Visibility_Status",
                table: "Jobs",
                columns: new[] { "Visibility", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_JobSeekerProfiles_UserId",
                table: "JobSeekerProfiles",
                column: "UserId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_OrganisationClaimRequests_EmailDomain_Status",
                table: "OrganisationClaimRequests",
                columns: new[] { "EmailDomain", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_OrganisationClaimRequests_OrganisationId_Status",
                table: "OrganisationClaimRequests",
                columns: new[] { "OrganisationId", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_OrganisationClaimRequests_RequestedByUserId",
                table: "OrganisationClaimRequests",
                column: "RequestedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_OrganisationDomains_NormalizedDomain",
                table: "OrganisationDomains",
                column: "NormalizedDomain",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_OrganisationDomains_OrganisationId_IsPrimary",
                table: "OrganisationDomains",
                columns: new[] { "OrganisationId", "IsPrimary" });

            migrationBuilder.CreateIndex(
                name: "IX_OrganisationInvitations_OrganisationId_NormalizedEmail_Type~",
                table: "OrganisationInvitations",
                columns: new[] { "OrganisationId", "NormalizedEmail", "Type", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_OrganisationInvitations_Token",
                table: "OrganisationInvitations",
                column: "Token",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_OrganisationMemberships_OrganisationId_IsOwner",
                table: "OrganisationMemberships",
                columns: new[] { "OrganisationId", "IsOwner" });

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
                name: "IX_Organisations_PrimaryDomainId",
                table: "Organisations",
                column: "PrimaryDomainId");

            migrationBuilder.CreateIndex(
                name: "IX_Organisations_Slug",
                table: "Organisations",
                column: "Slug",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Organisations_Type_NormalizedName",
                table: "Organisations",
                columns: new[] { "Type", "NormalizedName" });

            migrationBuilder.CreateIndex(
                name: "IX_StoredFiles_UploadedByUserId",
                table: "StoredFiles",
                column: "UploadedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_tenants_Slug",
                table: "tenants",
                column: "Slug",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_user_tenant_memberships_TenantId",
                table: "user_tenant_memberships",
                column: "TenantId");

            migrationBuilder.AddForeignKey(
                name: "FK_CompanyRecruiterRelationships_Organisations_CompanyOrganisa~",
                table: "CompanyRecruiterRelationships",
                column: "CompanyOrganisationId",
                principalTable: "Organisations",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_CompanyRecruiterRelationships_Organisations_RecruiterOrgani~",
                table: "CompanyRecruiterRelationships",
                column: "RecruiterOrganisationId",
                principalTable: "Organisations",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_JobApplications_Jobs_JobId",
                table: "JobApplications",
                column: "JobId",
                principalTable: "Jobs",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

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
                onDelete: ReferentialAction.Restrict);

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
                name: "JobApplications");

            migrationBuilder.DropTable(
                name: "JobSeekerProfiles");

            migrationBuilder.DropTable(
                name: "OrganisationClaimRequests");

            migrationBuilder.DropTable(
                name: "OrganisationInvitations");

            migrationBuilder.DropTable(
                name: "OrganisationMemberships");

            migrationBuilder.DropTable(
                name: "StoredFiles");

            migrationBuilder.DropTable(
                name: "user_tenant_memberships");

            migrationBuilder.DropTable(
                name: "AspNetRoles");

            migrationBuilder.DropTable(
                name: "Jobs");

            migrationBuilder.DropTable(
                name: "tenants");

            migrationBuilder.DropTable(
                name: "AspNetUsers");

            migrationBuilder.DropTable(
                name: "CompanyRecruiterRelationships");

            migrationBuilder.DropTable(
                name: "Organisations");

            migrationBuilder.DropTable(
                name: "OrganisationDomains");
        }
    }
}
