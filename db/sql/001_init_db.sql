CREATE TABLE IF NOT EXISTS "__EFMigrationsHistory" (
    "MigrationId" character varying(150) NOT NULL,
    "ProductVersion" character varying(32) NOT NULL,
    CONSTRAINT "PK___EFMigrationsHistory" PRIMARY KEY ("MigrationId")
);

START TRANSACTION;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260416132819_InitDb') THEN
        IF NOT EXISTS(SELECT 1 FROM pg_namespace WHERE nspname = 'aethon') THEN
            CREATE SCHEMA aethon;
        END IF;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260416132819_InitDb') THEN
    CREATE TABLE aethon."AspNetRoles" (
        "Id" uuid NOT NULL,
        "Name" character varying(256),
        "NormalizedName" character varying(256),
        "ConcurrencyStamp" text,
        CONSTRAINT "PK_AspNetRoles" PRIMARY KEY ("Id")
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260416132819_InitDb') THEN
    CREATE TABLE aethon."AspNetUsers" (
        "Id" uuid NOT NULL,
        "DisplayName" character varying(200) NOT NULL,
        "IsEnabled" boolean NOT NULL,
        "UserType" integer NOT NULL,
        "IsIdentityVerified" boolean NOT NULL,
        "IdentityVerifiedUtc" timestamp with time zone,
        "IdentityVerificationNotes" character varying(2000),
        "IsPhoneNumberVerified" boolean NOT NULL,
        "PhoneNumberVerifiedUtc" timestamp with time zone,
        "MustChangePassword" boolean NOT NULL,
        "MustEnableMfa" boolean NOT NULL,
        "LastLoginUtc" timestamp with time zone,
        "UserName" character varying(256),
        "NormalizedUserName" character varying(256),
        "Email" character varying(256),
        "NormalizedEmail" character varying(256),
        "EmailConfirmed" boolean NOT NULL,
        "PasswordHash" text,
        "SecurityStamp" text,
        "ConcurrencyStamp" text,
        "PhoneNumber" text,
        "PhoneNumberConfirmed" boolean NOT NULL,
        "TwoFactorEnabled" boolean NOT NULL,
        "LockoutEnd" timestamp with time zone,
        "LockoutEnabled" boolean NOT NULL,
        "AccessFailedCount" integer NOT NULL,
        CONSTRAINT "PK_AspNetUsers" PRIMARY KEY ("Id")
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260416132819_InitDb') THEN
    CREATE TABLE aethon."Locations" (
        "Id" uuid NOT NULL,
        "DisplayName" character varying(300) NOT NULL,
        "City" character varying(150),
        "State" character varying(150),
        "Country" character varying(100),
        "CountryCode" character varying(10),
        "Latitude" double precision NOT NULL,
        "Longitude" double precision NOT NULL,
        "IsActive" boolean NOT NULL,
        "SortOrder" integer NOT NULL,
        "CreatedUtc" timestamp with time zone NOT NULL,
        CONSTRAINT "PK_Locations" PRIMARY KEY ("Id")
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260416132819_InitDb') THEN
    CREATE TABLE aethon."StoredFiles" (
        "Id" uuid NOT NULL,
        "FileName" character varying(260) NOT NULL,
        "OriginalFileName" character varying(260) NOT NULL,
        "ContentType" character varying(200) NOT NULL,
        "LengthBytes" bigint NOT NULL,
        "StorageProvider" character varying(50) NOT NULL,
        "StoragePath" character varying(1000) NOT NULL,
        "UploadedByUserId" uuid NOT NULL,
        "CreatedUtc" timestamp with time zone NOT NULL,
        "UpdatedUtc" timestamp with time zone,
        "CreatedByUserId" uuid,
        "UpdatedByUserId" uuid,
        CONSTRAINT "PK_StoredFiles" PRIMARY KEY ("Id")
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260416132819_InitDb') THEN
    CREATE TABLE aethon."SystemLogs" (
        "Id" uuid NOT NULL,
        "TimestampUtc" timestamp with time zone NOT NULL,
        "Level" integer NOT NULL,
        "Category" text NOT NULL,
        "Message" text NOT NULL,
        "Details" text,
        "ExceptionType" text,
        "ExceptionMessage" text,
        "UserId" uuid,
        "RequestPath" text,
        CONSTRAINT "PK_SystemLogs" PRIMARY KEY ("Id")
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260416132819_InitDb') THEN
    CREATE TABLE aethon."SystemSettings" (
        "Key" character varying(200) NOT NULL,
        "Value" character varying(8000) NOT NULL,
        "Description" character varying(500),
        "UpdatedUtc" timestamp with time zone NOT NULL,
        "UpdatedByUserId" uuid,
        CONSTRAINT "PK_SystemSettings" PRIMARY KEY ("Key")
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260416132819_InitDb') THEN
    CREATE TABLE aethon."AspNetRoleClaims" (
        "Id" integer GENERATED BY DEFAULT AS IDENTITY,
        "RoleId" uuid NOT NULL,
        "ClaimType" text,
        "ClaimValue" text,
        CONSTRAINT "PK_AspNetRoleClaims" PRIMARY KEY ("Id"),
        CONSTRAINT "FK_AspNetRoleClaims_AspNetRoles_RoleId" FOREIGN KEY ("RoleId") REFERENCES aethon."AspNetRoles" ("Id") ON DELETE CASCADE
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260416132819_InitDb') THEN
    CREATE TABLE aethon."AspNetUserClaims" (
        "Id" integer GENERATED BY DEFAULT AS IDENTITY,
        "UserId" uuid NOT NULL,
        "ClaimType" text,
        "ClaimValue" text,
        CONSTRAINT "PK_AspNetUserClaims" PRIMARY KEY ("Id"),
        CONSTRAINT "FK_AspNetUserClaims_AspNetUsers_UserId" FOREIGN KEY ("UserId") REFERENCES aethon."AspNetUsers" ("Id") ON DELETE CASCADE
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260416132819_InitDb') THEN
    CREATE TABLE aethon."AspNetUserLogins" (
        "LoginProvider" text NOT NULL,
        "ProviderKey" text NOT NULL,
        "ProviderDisplayName" text,
        "UserId" uuid NOT NULL,
        CONSTRAINT "PK_AspNetUserLogins" PRIMARY KEY ("LoginProvider", "ProviderKey"),
        CONSTRAINT "FK_AspNetUserLogins_AspNetUsers_UserId" FOREIGN KEY ("UserId") REFERENCES aethon."AspNetUsers" ("Id") ON DELETE CASCADE
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260416132819_InitDb') THEN
    CREATE TABLE aethon."AspNetUserRoles" (
        "UserId" uuid NOT NULL,
        "RoleId" uuid NOT NULL,
        CONSTRAINT "PK_AspNetUserRoles" PRIMARY KEY ("UserId", "RoleId"),
        CONSTRAINT "FK_AspNetUserRoles_AspNetRoles_RoleId" FOREIGN KEY ("RoleId") REFERENCES aethon."AspNetRoles" ("Id") ON DELETE CASCADE,
        CONSTRAINT "FK_AspNetUserRoles_AspNetUsers_UserId" FOREIGN KEY ("UserId") REFERENCES aethon."AspNetUsers" ("Id") ON DELETE CASCADE
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260416132819_InitDb') THEN
    CREATE TABLE aethon."AspNetUserTokens" (
        "UserId" uuid NOT NULL,
        "LoginProvider" text NOT NULL,
        "Name" text NOT NULL,
        "Value" text,
        CONSTRAINT "PK_AspNetUserTokens" PRIMARY KEY ("UserId", "LoginProvider", "Name"),
        CONSTRAINT "FK_AspNetUserTokens_AspNetUsers_UserId" FOREIGN KEY ("UserId") REFERENCES aethon."AspNetUsers" ("Id") ON DELETE CASCADE
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260416132819_InitDb') THEN
    CREATE TABLE aethon."IdentityVerificationRequests" (
        "Id" uuid NOT NULL,
        "UserId" uuid NOT NULL,
        "FullName" character varying(300) NOT NULL,
        "EmailAddress" character varying(256) NOT NULL,
        "PhoneNumber" character varying(50) NOT NULL,
        "AdditionalNotes" character varying(2000),
        "Status" integer NOT NULL,
        "ReviewedByUserId" uuid,
        "ReviewedUtc" timestamp with time zone,
        "ReviewNotes" character varying(2000),
        "ReviewerType" integer,
        "CreatedUtc" timestamp with time zone NOT NULL,
        "UpdatedUtc" timestamp with time zone,
        "CreatedByUserId" uuid,
        "UpdatedByUserId" uuid,
        CONSTRAINT "PK_IdentityVerificationRequests" PRIMARY KEY ("Id"),
        CONSTRAINT "FK_IdentityVerificationRequests_AspNetUsers_ReviewedByUserId" FOREIGN KEY ("ReviewedByUserId") REFERENCES aethon."AspNetUsers" ("Id") ON DELETE SET NULL,
        CONSTRAINT "FK_IdentityVerificationRequests_AspNetUsers_UserId" FOREIGN KEY ("UserId") REFERENCES aethon."AspNetUsers" ("Id") ON DELETE RESTRICT
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260416132819_InitDb') THEN
    CREATE TABLE aethon."JobSeekerProfiles" (
        "Id" uuid NOT NULL,
        "UserId" uuid NOT NULL,
        "FirstName" character varying(100),
        "MiddleName" character varying(100),
        "LastName" character varying(100),
        "AgeGroup" integer NOT NULL,
        "BirthMonth" integer,
        "BirthYear" integer,
        "AgeConfirmedUtc" timestamp with time zone,
        "PhoneNumber" character varying(50),
        "WhatsAppNumber" character varying(50),
        "Headline" character varying(250),
        "Summary" character varying(4000),
        "CurrentLocation" character varying(250),
        "PreferredLocation" character varying(250),
        "LinkedInUrl" character varying(1000),
        "OpenToWork" boolean NOT NULL,
        "DesiredSalaryFrom" numeric(18,2),
        "DesiredSalaryTo" numeric(18,2),
        "DesiredSalaryCurrency" integer,
        "WillRelocate" boolean,
        "RequiresSponsorship" boolean,
        "HasWorkRights" boolean,
        "AvailabilityText" character varying(250),
        "IsPublicProfileEnabled" boolean NOT NULL,
        "IsSearchable" boolean NOT NULL,
        "Slug" character varying(150),
        "AboutMe" character varying(2000),
        "ProfileVisibility" integer NOT NULL DEFAULT 1,
        "LinkedInId" character varying(100),
        "LinkedInVerifiedAt" timestamp with time zone,
        "ProfilePictureStoredFileId" uuid,
        "IsIdVerified" boolean NOT NULL,
        "IsNameLocked" boolean NOT NULL,
        "LastProfileUpdatedUtc" timestamp with time zone,
        "CreatedUtc" timestamp with time zone NOT NULL,
        "UpdatedUtc" timestamp with time zone,
        "CreatedByUserId" uuid,
        "UpdatedByUserId" uuid,
        CONSTRAINT "PK_JobSeekerProfiles" PRIMARY KEY ("Id"),
        CONSTRAINT "FK_JobSeekerProfiles_AspNetUsers_UserId" FOREIGN KEY ("UserId") REFERENCES aethon."AspNetUsers" ("Id") ON DELETE CASCADE
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260416132819_InitDb') THEN
    CREATE TABLE aethon."JobSeekerCertificates" (
        "Id" uuid NOT NULL,
        "JobSeekerProfileId" uuid NOT NULL,
        "Name" character varying(200) NOT NULL,
        "IssuingOrganisation" character varying(200),
        "IssuedMonth" integer,
        "IssuedYear" integer,
        "ExpiryYear" integer,
        "CredentialId" character varying(200),
        "CredentialUrl" character varying(1000),
        "SortOrder" integer NOT NULL,
        "CreatedUtc" timestamp with time zone NOT NULL,
        "UpdatedUtc" timestamp with time zone,
        "CreatedByUserId" uuid,
        "UpdatedByUserId" uuid,
        CONSTRAINT "PK_JobSeekerCertificates" PRIMARY KEY ("Id"),
        CONSTRAINT "FK_JobSeekerCertificates_JobSeekerProfiles_JobSeekerProfileId" FOREIGN KEY ("JobSeekerProfileId") REFERENCES aethon."JobSeekerProfiles" ("Id") ON DELETE CASCADE
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260416132819_InitDb') THEN
    CREATE TABLE aethon."JobSeekerLanguages" (
        "Id" uuid NOT NULL,
        "JobSeekerProfileId" uuid NOT NULL,
        "Name" character varying(100) NOT NULL,
        "AbilityType" integer NOT NULL,
        "ProficiencyLevel" integer,
        "IsVerified" boolean NOT NULL,
        "VerifiedUtc" timestamp with time zone,
        "VerificationNotes" character varying(1000),
        "CreatedUtc" timestamp with time zone NOT NULL,
        "UpdatedUtc" timestamp with time zone,
        "CreatedByUserId" uuid,
        "UpdatedByUserId" uuid,
        CONSTRAINT "PK_JobSeekerLanguages" PRIMARY KEY ("Id"),
        CONSTRAINT "FK_JobSeekerLanguages_JobSeekerProfiles_JobSeekerProfileId" FOREIGN KEY ("JobSeekerProfileId") REFERENCES aethon."JobSeekerProfiles" ("Id") ON DELETE CASCADE
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260416132819_InitDb') THEN
    CREATE TABLE aethon."JobSeekerNationalities" (
        "Id" uuid NOT NULL,
        "JobSeekerProfileId" uuid NOT NULL,
        "Name" character varying(100) NOT NULL,
        "IsVerified" boolean NOT NULL,
        "VerifiedUtc" timestamp with time zone,
        "VerificationNotes" character varying(1000),
        "CreatedUtc" timestamp with time zone NOT NULL,
        "UpdatedUtc" timestamp with time zone,
        "CreatedByUserId" uuid,
        "UpdatedByUserId" uuid,
        CONSTRAINT "PK_JobSeekerNationalities" PRIMARY KEY ("Id"),
        CONSTRAINT "FK_JobSeekerNationalities_JobSeekerProfiles_JobSeekerProfileId" FOREIGN KEY ("JobSeekerProfileId") REFERENCES aethon."JobSeekerProfiles" ("Id") ON DELETE CASCADE
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260416132819_InitDb') THEN
    CREATE TABLE aethon."JobSeekerQualifications" (
        "Id" uuid NOT NULL,
        "JobSeekerProfileId" uuid NOT NULL,
        "Title" character varying(200) NOT NULL,
        "Institution" character varying(200),
        "CompletedYear" integer,
        "Description" character varying(2000),
        "SortOrder" integer NOT NULL,
        "CreatedUtc" timestamp with time zone NOT NULL,
        "UpdatedUtc" timestamp with time zone,
        "CreatedByUserId" uuid,
        "UpdatedByUserId" uuid,
        CONSTRAINT "PK_JobSeekerQualifications" PRIMARY KEY ("Id"),
        CONSTRAINT "FK_JobSeekerQualifications_JobSeekerProfiles_JobSeekerProfileId" FOREIGN KEY ("JobSeekerProfileId") REFERENCES aethon."JobSeekerProfiles" ("Id") ON DELETE CASCADE
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260416132819_InitDb') THEN
    CREATE TABLE aethon."JobSeekerResumes" (
        "Id" uuid NOT NULL,
        "JobSeekerProfileId" uuid NOT NULL,
        "StoredFileId" uuid NOT NULL,
        "Name" character varying(150) NOT NULL,
        "Description" character varying(1000),
        "IsDefault" boolean NOT NULL,
        "IsActive" boolean NOT NULL,
        "CreatedUtc" timestamp with time zone NOT NULL,
        "UpdatedUtc" timestamp with time zone,
        "CreatedByUserId" uuid,
        "UpdatedByUserId" uuid,
        CONSTRAINT "PK_JobSeekerResumes" PRIMARY KEY ("Id"),
        CONSTRAINT "FK_JobSeekerResumes_JobSeekerProfiles_JobSeekerProfileId" FOREIGN KEY ("JobSeekerProfileId") REFERENCES aethon."JobSeekerProfiles" ("Id") ON DELETE CASCADE,
        CONSTRAINT "FK_JobSeekerResumes_StoredFiles_StoredFileId" FOREIGN KEY ("StoredFileId") REFERENCES aethon."StoredFiles" ("Id") ON DELETE RESTRICT
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260416132819_InitDb') THEN
    CREATE TABLE aethon."JobSeekerSkills" (
        "Id" uuid NOT NULL,
        "JobSeekerProfileId" uuid NOT NULL,
        "Name" character varying(100) NOT NULL,
        "SkillLevel" integer,
        "SortOrder" integer NOT NULL,
        "CreatedUtc" timestamp with time zone NOT NULL,
        "UpdatedUtc" timestamp with time zone,
        "CreatedByUserId" uuid,
        "UpdatedByUserId" uuid,
        CONSTRAINT "PK_JobSeekerSkills" PRIMARY KEY ("Id"),
        CONSTRAINT "FK_JobSeekerSkills_JobSeekerProfiles_JobSeekerProfileId" FOREIGN KEY ("JobSeekerProfileId") REFERENCES aethon."JobSeekerProfiles" ("Id") ON DELETE CASCADE
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260416132819_InitDb') THEN
    CREATE TABLE aethon."JobSeekerWorkExperiences" (
        "Id" uuid NOT NULL,
        "JobSeekerProfileId" uuid NOT NULL,
        "JobTitle" character varying(200) NOT NULL,
        "EmployerName" character varying(200) NOT NULL,
        "StartMonth" integer NOT NULL,
        "StartYear" integer NOT NULL,
        "EndMonth" integer,
        "EndYear" integer,
        "IsCurrent" boolean NOT NULL,
        "Description" character varying(4000),
        "SortOrder" integer NOT NULL,
        "CreatedUtc" timestamp with time zone NOT NULL,
        "UpdatedUtc" timestamp with time zone,
        "CreatedByUserId" uuid,
        "UpdatedByUserId" uuid,
        CONSTRAINT "PK_JobSeekerWorkExperiences" PRIMARY KEY ("Id"),
        CONSTRAINT "FK_JobSeekerWorkExperiences_JobSeekerProfiles_JobSeekerProfile~" FOREIGN KEY ("JobSeekerProfileId") REFERENCES aethon."JobSeekerProfiles" ("Id") ON DELETE CASCADE
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260416132819_InitDb') THEN
    CREATE TABLE aethon."ResumeAnalyses" (
        "Id" uuid NOT NULL,
        "JobSeekerResumeId" uuid NOT NULL,
        "StoredFileId" uuid NOT NULL,
        "Status" integer NOT NULL,
        "HeadlineSuggestion" character varying(300),
        "SummaryExtract" character varying(2000),
        "SkillsJson" character varying(4000),
        "ExperienceLevel" character varying(50),
        "YearsExperience" integer,
        "AnalysedUtc" timestamp with time zone,
        "AnalysisError" character varying(2000),
        "CreatedUtc" timestamp with time zone NOT NULL,
        "UpdatedUtc" timestamp with time zone,
        "CreatedByUserId" uuid,
        "UpdatedByUserId" uuid,
        CONSTRAINT "PK_ResumeAnalyses" PRIMARY KEY ("Id"),
        CONSTRAINT "FK_ResumeAnalyses_JobSeekerResumes_JobSeekerResumeId" FOREIGN KEY ("JobSeekerResumeId") REFERENCES aethon."JobSeekerResumes" ("Id") ON DELETE CASCADE,
        CONSTRAINT "FK_ResumeAnalyses_StoredFiles_StoredFileId" FOREIGN KEY ("StoredFileId") REFERENCES aethon."StoredFiles" ("Id") ON DELETE RESTRICT
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260416132819_InitDb') THEN
    CREATE TABLE aethon."ActivityLogs" (
        "Id" uuid NOT NULL,
        "OrganisationId" uuid,
        "EntityType" character varying(100) NOT NULL,
        "EntityId" uuid NOT NULL,
        "Action" character varying(100) NOT NULL,
        "Summary" character varying(500),
        "Details" character varying(8000),
        "PerformedByUserId" uuid,
        "PerformedUtc" timestamp with time zone NOT NULL,
        "CreatedUtc" timestamp with time zone NOT NULL,
        "UpdatedUtc" timestamp with time zone,
        "CreatedByUserId" uuid,
        "UpdatedByUserId" uuid,
        CONSTRAINT "PK_ActivityLogs" PRIMARY KEY ("Id"),
        CONSTRAINT "FK_ActivityLogs_AspNetUsers_PerformedByUserId" FOREIGN KEY ("PerformedByUserId") REFERENCES aethon."AspNetUsers" ("Id") ON DELETE RESTRICT
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260416132819_InitDb') THEN
    CREATE TABLE aethon."AtsMatchQueue" (
        "Id" uuid NOT NULL,
        "JobApplicationId" uuid NOT NULL,
        "JobId" uuid NOT NULL,
        "CandidateUserId" uuid NOT NULL,
        "Provider" integer NOT NULL,
        "Priority" integer NOT NULL,
        "Status" integer NOT NULL,
        "Attempts" integer NOT NULL,
        "LastAttemptUtc" timestamp with time zone,
        "ErrorMessage" text,
        "PayloadJson" text NOT NULL,
        "ProcessedUtc" timestamp with time zone,
        "CreatedUtc" timestamp with time zone NOT NULL,
        "UpdatedUtc" timestamp with time zone,
        "CreatedByUserId" uuid,
        "UpdatedByUserId" uuid,
        CONSTRAINT "PK_AtsMatchQueue" PRIMARY KEY ("Id")
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260416132819_InitDb') THEN
    CREATE TABLE aethon."AtsMatchResults" (
        "Id" uuid NOT NULL,
        "AtsMatchQueueItemId" uuid NOT NULL,
        "JobApplicationId" uuid NOT NULL,
        "JobId" uuid NOT NULL,
        "CandidateUserId" uuid NOT NULL,
        "Provider" integer NOT NULL,
        "ModelUsed" text NOT NULL,
        "OverallScore" integer NOT NULL,
        "SkillsScore" integer,
        "ExperienceScore" integer,
        "LocationScore" integer,
        "SalaryScore" integer,
        "QualificationsScore" integer,
        "WorkRightsScore" integer,
        "Recommendation" integer NOT NULL,
        "Strengths" text,
        "Gaps" text,
        "Summary" text,
        "Confidence" real,
        "RawResponseJson" text,
        "TokensUsed" integer,
        "ProcessedUtc" timestamp with time zone NOT NULL,
        "CreatedUtc" timestamp with time zone NOT NULL,
        "UpdatedUtc" timestamp with time zone,
        "CreatedByUserId" uuid,
        "UpdatedByUserId" uuid,
        CONSTRAINT "PK_AtsMatchResults" PRIMARY KEY ("Id"),
        CONSTRAINT "FK_AtsMatchResults_AtsMatchQueue_AtsMatchQueueItemId" FOREIGN KEY ("AtsMatchQueueItemId") REFERENCES aethon."AtsMatchQueue" ("Id") ON DELETE CASCADE
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260416132819_InitDb') THEN
    CREATE TABLE aethon."CreditConsumptionLogs" (
        "Id" uuid NOT NULL,
        "OrganisationJobCreditId" uuid NOT NULL,
        "OrganisationId" uuid NOT NULL,
        "JobId" uuid NOT NULL,
        "ConsumedByUserId" uuid NOT NULL,
        "ApprovedByUserId" uuid,
        "QuantityConsumed" integer NOT NULL,
        "ConsumedAt" timestamp with time zone NOT NULL,
        "CreatedUtc" timestamp with time zone NOT NULL,
        "UpdatedUtc" timestamp with time zone,
        "CreatedByUserId" uuid,
        "UpdatedByUserId" uuid,
        CONSTRAINT "PK_CreditConsumptionLogs" PRIMARY KEY ("Id")
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260416132819_InitDb') THEN
    CREATE TABLE aethon."JobApplicationAttachments" (
        "Id" uuid NOT NULL,
        "JobApplicationId" uuid NOT NULL,
        "StoredFileId" uuid NOT NULL,
        "Category" character varying(100) NOT NULL,
        "Notes" character varying(2000),
        "CreatedUtc" timestamp with time zone NOT NULL,
        "CreatedByUserId" uuid,
        CONSTRAINT "PK_JobApplicationAttachments" PRIMARY KEY ("Id"),
        CONSTRAINT "FK_JobApplicationAttachments_StoredFiles_StoredFileId" FOREIGN KEY ("StoredFileId") REFERENCES aethon."StoredFiles" ("Id") ON DELETE RESTRICT
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260416132819_InitDb') THEN
    CREATE TABLE aethon."JobApplicationComments" (
        "Id" uuid NOT NULL,
        "JobApplicationId" uuid NOT NULL,
        "ParentCommentId" uuid,
        "Content" character varying(8000) NOT NULL,
        "IsDeleted" boolean NOT NULL,
        "DeletedUtc" timestamp with time zone,
        "DeletedByUserId" uuid,
        "CreatedUtc" timestamp with time zone NOT NULL,
        "UpdatedUtc" timestamp with time zone,
        "CreatedByUserId" uuid,
        "UpdatedByUserId" uuid,
        CONSTRAINT "PK_JobApplicationComments" PRIMARY KEY ("Id"),
        CONSTRAINT "FK_JobApplicationComments_AspNetUsers_CreatedByUserId" FOREIGN KEY ("CreatedByUserId") REFERENCES aethon."AspNetUsers" ("Id") ON DELETE RESTRICT,
        CONSTRAINT "FK_JobApplicationComments_AspNetUsers_DeletedByUserId" FOREIGN KEY ("DeletedByUserId") REFERENCES aethon."AspNetUsers" ("Id") ON DELETE RESTRICT,
        CONSTRAINT "FK_JobApplicationComments_AspNetUsers_UpdatedByUserId" FOREIGN KEY ("UpdatedByUserId") REFERENCES aethon."AspNetUsers" ("Id") ON DELETE RESTRICT,
        CONSTRAINT "FK_JobApplicationComments_JobApplicationComments_ParentComment~" FOREIGN KEY ("ParentCommentId") REFERENCES aethon."JobApplicationComments" ("Id") ON DELETE RESTRICT
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260416132819_InitDb') THEN
    CREATE TABLE aethon."JobApplicationInterviewInterviewers" (
        "Id" uuid NOT NULL,
        "JobApplicationInterviewId" uuid NOT NULL,
        "UserId" uuid NOT NULL,
        "RoleLabel" character varying(100),
        "CreatedUtc" timestamp with time zone NOT NULL,
        "UpdatedUtc" timestamp with time zone,
        "CreatedByUserId" uuid,
        "UpdatedByUserId" uuid,
        CONSTRAINT "PK_JobApplicationInterviewInterviewers" PRIMARY KEY ("Id"),
        CONSTRAINT "FK_JobApplicationInterviewInterviewers_AspNetUsers_UserId" FOREIGN KEY ("UserId") REFERENCES aethon."AspNetUsers" ("Id") ON DELETE RESTRICT
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260416132819_InitDb') THEN
    CREATE TABLE aethon."JobApplicationInterviews" (
        "Id" uuid NOT NULL,
        "JobApplicationId" uuid NOT NULL,
        "Type" integer NOT NULL,
        "Status" integer NOT NULL,
        "Title" character varying(250),
        "Location" character varying(500),
        "MeetingUrl" character varying(1000),
        "Notes" character varying(4000),
        "ScheduledStartUtc" timestamp with time zone NOT NULL,
        "ScheduledEndUtc" timestamp with time zone NOT NULL,
        "CompletedUtc" timestamp with time zone,
        "CancelledUtc" timestamp with time zone,
        "CancellationReason" character varying(1000),
        "CreatedUtc" timestamp with time zone NOT NULL,
        "UpdatedUtc" timestamp with time zone,
        "CreatedByUserId" uuid,
        "UpdatedByUserId" uuid,
        CONSTRAINT "PK_JobApplicationInterviews" PRIMARY KEY ("Id")
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260416132819_InitDb') THEN
    CREATE TABLE aethon."JobApplicationNotes" (
        "Id" uuid NOT NULL,
        "JobApplicationId" uuid NOT NULL,
        "Content" character varying(8000) NOT NULL,
        "IsDeleted" boolean NOT NULL,
        "DeletedUtc" timestamp with time zone,
        "DeletedByUserId" uuid,
        "CreatedUtc" timestamp with time zone NOT NULL,
        "UpdatedUtc" timestamp with time zone,
        "CreatedByUserId" uuid,
        "UpdatedByUserId" uuid,
        CONSTRAINT "PK_JobApplicationNotes" PRIMARY KEY ("Id"),
        CONSTRAINT "FK_JobApplicationNotes_AspNetUsers_CreatedByUserId" FOREIGN KEY ("CreatedByUserId") REFERENCES aethon."AspNetUsers" ("Id") ON DELETE RESTRICT,
        CONSTRAINT "FK_JobApplicationNotes_AspNetUsers_DeletedByUserId" FOREIGN KEY ("DeletedByUserId") REFERENCES aethon."AspNetUsers" ("Id") ON DELETE RESTRICT,
        CONSTRAINT "FK_JobApplicationNotes_AspNetUsers_UpdatedByUserId" FOREIGN KEY ("UpdatedByUserId") REFERENCES aethon."AspNetUsers" ("Id") ON DELETE RESTRICT
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260416132819_InitDb') THEN
    CREATE TABLE aethon."JobApplications" (
        "Id" uuid NOT NULL,
        "JobId" uuid NOT NULL,
        "UserId" uuid NOT NULL,
        "Status" integer NOT NULL,
        "StatusReason" character varying(1000),
        "ResumeFileId" uuid,
        "CoverLetter" character varying(20000),
        "AssignedRecruiterUserId" uuid,
        "AssignedRecruiterUtc" timestamp with time zone,
        "AssignedToUserId" uuid,
        "SubmittedUtc" timestamp with time zone NOT NULL,
        "LastStatusChangedUtc" timestamp with time zone,
        "LastActivityUtc" timestamp with time zone,
        "Source" character varying(100),
        "SourceDetail" character varying(250),
        "SourceReference" character varying(150),
        "InternalSummaryNotes" character varying(4000),
        "ScreeningSummary" character varying(4000),
        "Rating" numeric(5,2),
        "Recommendation" character varying(100),
        "Tags" character varying(1000),
        "CandidatePhoneNumber" character varying(50),
        "CandidateLocationText" character varying(250),
        "AvailabilityText" character varying(250),
        "SalaryExpectation" numeric(18,2),
        "SalaryExpectationCurrency" integer,
        "WillRelocate" boolean,
        "RequiresSponsorship" boolean,
        "HasWorkRights" boolean,
        "AcceptedPrivacyPolicy" boolean NOT NULL,
        "AcceptedPrivacyPolicyUtc" timestamp with time zone,
        "IsWithdrawn" boolean NOT NULL,
        "WithdrawnUtc" timestamp with time zone,
        "WithdrawalReason" character varying(1000),
        "WithdrawnByUserId" uuid,
        "IsRejected" boolean NOT NULL,
        "RejectedUtc" timestamp with time zone,
        "RejectionReason" character varying(1000),
        "RejectedByUserId" uuid,
        "IsHired" boolean NOT NULL,
        "HiredUtc" timestamp with time zone,
        "IsDuplicate" boolean NOT NULL,
        "DuplicateOfApplicationId" uuid,
        "IsArchived" boolean NOT NULL,
        "ArchivedUtc" timestamp with time zone,
        "ExternalReference" character varying(150),
        "ScreeningAnswersJson" text,
        "IsNotSuitable" boolean NOT NULL,
        "NotSuitableReasons" text,
        "CreatedUtc" timestamp with time zone NOT NULL,
        "UpdatedUtc" timestamp with time zone,
        "CreatedByUserId" uuid,
        "UpdatedByUserId" uuid,
        CONSTRAINT "PK_JobApplications" PRIMARY KEY ("Id"),
        CONSTRAINT "FK_JobApplications_AspNetUsers_AssignedRecruiterUserId" FOREIGN KEY ("AssignedRecruiterUserId") REFERENCES aethon."AspNetUsers" ("Id") ON DELETE RESTRICT,
        CONSTRAINT "FK_JobApplications_AspNetUsers_AssignedToUserId" FOREIGN KEY ("AssignedToUserId") REFERENCES aethon."AspNetUsers" ("Id") ON DELETE RESTRICT,
        CONSTRAINT "FK_JobApplications_AspNetUsers_RejectedByUserId" FOREIGN KEY ("RejectedByUserId") REFERENCES aethon."AspNetUsers" ("Id") ON DELETE RESTRICT,
        CONSTRAINT "FK_JobApplications_AspNetUsers_UserId" FOREIGN KEY ("UserId") REFERENCES aethon."AspNetUsers" ("Id") ON DELETE RESTRICT,
        CONSTRAINT "FK_JobApplications_AspNetUsers_WithdrawnByUserId" FOREIGN KEY ("WithdrawnByUserId") REFERENCES aethon."AspNetUsers" ("Id") ON DELETE RESTRICT,
        CONSTRAINT "FK_JobApplications_JobApplications_DuplicateOfApplicationId" FOREIGN KEY ("DuplicateOfApplicationId") REFERENCES aethon."JobApplications" ("Id") ON DELETE RESTRICT,
        CONSTRAINT "FK_JobApplications_StoredFiles_ResumeFileId" FOREIGN KEY ("ResumeFileId") REFERENCES aethon."StoredFiles" ("Id") ON DELETE SET NULL
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260416132819_InitDb') THEN
    CREATE TABLE aethon."JobApplicationStatusHistory" (
        "Id" uuid NOT NULL,
        "JobApplicationId" uuid NOT NULL,
        "FromStatus" integer,
        "ToStatus" integer NOT NULL,
        "Reason" character varying(1000),
        "Notes" character varying(4000),
        "ChangedByUserId" uuid NOT NULL,
        "ChangedUtc" timestamp with time zone NOT NULL,
        "CreatedUtc" timestamp with time zone NOT NULL,
        "UpdatedUtc" timestamp with time zone,
        "CreatedByUserId" uuid,
        "UpdatedByUserId" uuid,
        CONSTRAINT "PK_JobApplicationStatusHistory" PRIMARY KEY ("Id"),
        CONSTRAINT "FK_JobApplicationStatusHistory_AspNetUsers_ChangedByUserId" FOREIGN KEY ("ChangedByUserId") REFERENCES aethon."AspNetUsers" ("Id") ON DELETE RESTRICT,
        CONSTRAINT "FK_JobApplicationStatusHistory_JobApplications_JobApplicationId" FOREIGN KEY ("JobApplicationId") REFERENCES aethon."JobApplications" ("Id") ON DELETE CASCADE
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260416132819_InitDb') THEN
    CREATE TABLE aethon."Jobs" (
        "Id" uuid NOT NULL,
        "OwnedByOrganisationId" uuid NOT NULL,
        "ManagedByOrganisationId" uuid,
        "OrganisationRecruitmentPartnershipId" uuid,
        "CreatedByIdentityUserId" uuid NOT NULL,
        "ManagedByUserId" uuid,
        "CreatedByType" integer NOT NULL,
        "Status" integer NOT NULL,
        "StatusReason" character varying(1000),
        "Visibility" integer NOT NULL,
        "Title" character varying(250) NOT NULL,
        "ReferenceCode" character varying(100),
        "ExternalReference" character varying(150),
        "Department" character varying(150),
        "LocationText" character varying(250),
        "LocationCity" character varying(150),
        "LocationState" character varying(150),
        "LocationCountry" character varying(100),
        "LocationCountryCode" character varying(10),
        "LocationLatitude" double precision,
        "LocationLongitude" double precision,
        "LocationPlaceId" character varying(500),
        "WorkplaceType" integer NOT NULL,
        "EmploymentType" integer NOT NULL,
        "Description" character varying(20000) NOT NULL,
        "Requirements" character varying(12000),
        "Benefits" character varying(8000),
        "Summary" character varying(2000),
        "SalaryFrom" numeric(18,2),
        "SalaryTo" numeric(18,2),
        "SalaryCurrency" integer,
        "PublishedUtc" timestamp with time zone,
        "ApplyByUtc" timestamp with time zone,
        "ClosedUtc" timestamp with time zone,
        "SubmittedForApprovalUtc" timestamp with time zone,
        "ApprovedByUserId" uuid,
        "ApprovedUtc" timestamp with time zone,
        "ExternalApplicationUrl" character varying(1000),
        "ApplicationEmail" character varying(320),
        "CreatedForUnclaimedCompany" boolean NOT NULL,
        "Category" integer,
        "Regions" text,
        "Countries" text,
        "PostingExpiresUtc" timestamp with time zone,
        "PostingTier" integer NOT NULL,
        "HasAiCandidateMatching" boolean NOT NULL,
        "IncludeCompanyLogo" boolean NOT NULL,
        "IsHighlighted" boolean NOT NULL,
        "HighlightColour" character varying(20),
        "StickyUntilUtc" timestamp with time zone,
        "BenefitsTags" text,
        "ApplicationSpecialRequirements" text,
        "HasCommission" boolean NOT NULL,
        "OteFrom" numeric,
        "OteTo" numeric,
        "IsImmediateStart" boolean NOT NULL,
        "IsSuitableForSchoolLeavers" boolean NOT NULL,
        "IsSchoolLeaverTargeted" boolean NOT NULL,
        "VideoYouTubeId" text,
        "VideoVimeoId" text,
        "ScreeningQuestionsJson" text,
        "Keywords" text,
        "PoNumber" text,
        "IsImported" boolean NOT NULL DEFAULT FALSE,
        "AllowAutoMatch" boolean NOT NULL,
        "ShortUrlCode" text,
        "CreatedUtc" timestamp with time zone NOT NULL,
        "UpdatedUtc" timestamp with time zone,
        "CreatedByUserId" uuid,
        "UpdatedByUserId" uuid,
        CONSTRAINT "PK_Jobs" PRIMARY KEY ("Id"),
        CONSTRAINT "FK_Jobs_AspNetUsers_CreatedByIdentityUserId" FOREIGN KEY ("CreatedByIdentityUserId") REFERENCES aethon."AspNetUsers" ("Id") ON DELETE RESTRICT,
        CONSTRAINT "FK_Jobs_AspNetUsers_ManagedByUserId" FOREIGN KEY ("ManagedByUserId") REFERENCES aethon."AspNetUsers" ("Id") ON DELETE RESTRICT
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260416132819_InitDb') THEN
    CREATE TABLE aethon."JobSyndicationRecords" (
        "Id" uuid NOT NULL,
        "JobId" uuid NOT NULL,
        "Provider" character varying(100) NOT NULL,
        "Status" character varying(50) NOT NULL,
        "ExternalRef" character varying(500),
        "SubmittedUtc" timestamp with time zone NOT NULL,
        "LastAttemptUtc" timestamp with time zone,
        "LastErrorMessage" character varying(2000),
        "CreatedUtc" timestamp with time zone NOT NULL,
        "UpdatedUtc" timestamp with time zone,
        "CreatedByUserId" uuid,
        "UpdatedByUserId" uuid,
        CONSTRAINT "PK_JobSyndicationRecords" PRIMARY KEY ("Id"),
        CONSTRAINT "FK_JobSyndicationRecords_Jobs_JobId" FOREIGN KEY ("JobId") REFERENCES aethon."Jobs" ("Id") ON DELETE CASCADE
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260416132819_InitDb') THEN
    CREATE TABLE aethon."OrganisationClaimRequests" (
        "Id" uuid NOT NULL,
        "OrganisationId" uuid NOT NULL,
        "RequestedByUserId" uuid NOT NULL,
        "EmailUsed" character varying(320) NOT NULL,
        "EmailDomain" character varying(255) NOT NULL,
        "Status" integer NOT NULL,
        "VerificationMethod" integer NOT NULL,
        "VerificationToken" character varying(200),
        "VerifiedUtc" timestamp with time zone,
        "ApprovedByUserId" uuid,
        "ApprovedUtc" timestamp with time zone,
        "RejectionReason" character varying(1000),
        "CreatedUtc" timestamp with time zone NOT NULL,
        "UpdatedUtc" timestamp with time zone,
        "CreatedByUserId" uuid,
        "UpdatedByUserId" uuid,
        CONSTRAINT "PK_OrganisationClaimRequests" PRIMARY KEY ("Id"),
        CONSTRAINT "FK_OrganisationClaimRequests_AspNetUsers_RequestedByUserId" FOREIGN KEY ("RequestedByUserId") REFERENCES aethon."AspNetUsers" ("Id") ON DELETE RESTRICT
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260416132819_InitDb') THEN
    CREATE TABLE aethon."OrganisationDomains" (
        "Id" uuid NOT NULL,
        "OrganisationId" uuid NOT NULL,
        "Domain" character varying(255) NOT NULL,
        "NormalizedDomain" character varying(255) NOT NULL,
        "IsPrimary" boolean NOT NULL,
        "Status" integer NOT NULL,
        "VerificationMethod" integer NOT NULL,
        "TrustLevel" integer NOT NULL,
        "VerificationToken" character varying(200),
        "VerificationDnsRecordName" character varying(255),
        "VerificationDnsRecordValue" character varying(500),
        "VerificationEmailAddress" character varying(320),
        "VerificationRequestedUtc" timestamp with time zone,
        "VerifiedUtc" timestamp with time zone,
        "VerifiedByUserId" uuid,
        "CreatedUtc" timestamp with time zone NOT NULL,
        "UpdatedUtc" timestamp with time zone,
        "CreatedByUserId" uuid,
        "UpdatedByUserId" uuid,
        CONSTRAINT "PK_OrganisationDomains" PRIMARY KEY ("Id")
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260416132819_InitDb') THEN
    CREATE TABLE aethon."Organisations" (
        "Id" uuid NOT NULL,
        "Type" integer NOT NULL,
        "Status" integer NOT NULL,
        "ClaimStatus" integer NOT NULL,
        "Name" character varying(250) NOT NULL,
        "NormalizedName" character varying(250) NOT NULL,
        "LegalName" character varying(250),
        "WebsiteUrl" character varying(1000),
        "Slug" character varying(150),
        "LogoUrl" character varying(1000),
        "Summary" character varying(4000),
        "PublicLocationText" character varying(250),
        "LocationCity" character varying(150),
        "LocationState" character varying(150),
        "LocationCountry" character varying(100),
        "LocationCountryCode" character varying(10),
        "LocationLatitude" double precision,
        "LocationLongitude" double precision,
        "LocationPlaceId" character varying(500),
        "PublicContactEmail" character varying(320),
        "PublicContactPhone" character varying(50),
        "IsPublicProfileEnabled" boolean NOT NULL,
        "IsEqualOpportunityEmployer" boolean NOT NULL,
        "IsAccessibleWorkplace" boolean NOT NULL,
        "CompanySize" integer,
        "Industry" integer,
        "BannerImageUrl" text,
        "LinkedInUrl" text,
        "TwitterHandle" text,
        "FacebookUrl" text,
        "TikTokHandle" text,
        "InstagramHandle" text,
        "YouTubeUrl" text,
        "PrimaryDomainId" uuid,
        "IsProvisionedByRecruiter" boolean NOT NULL,
        "ClaimedByUserId" uuid,
        "ClaimedUtc" timestamp with time zone,
        "PrimaryContactName" character varying(200),
        "PrimaryContactEmail" character varying(320),
        "PrimaryContactPhoneDialCode" character varying(10),
        "PrimaryContactPhone" character varying(50),
        "PublicContactPhoneDialCode" character varying(10),
        "RegisteredAddressLine1" character varying(500),
        "RegisteredAddressLine2" character varying(500),
        "RegisteredCity" character varying(150),
        "RegisteredState" character varying(150),
        "RegisteredPostcode" character varying(20),
        "RegisteredCountry" character varying(150),
        "RegisteredCountryCode" character varying(10),
        "TaxRegistrationNumber" character varying(100),
        "BusinessRegistrationNumber" character varying(100),
        "VerificationTier" integer NOT NULL DEFAULT 0,
        "VerifiedUtc" timestamp with time zone,
        "VerifiedByUserId" uuid,
        "VerificationPaidAt" timestamp with time zone,
        "VerificationExpiresAt" timestamp with time zone,
        "VerificationStripeEventId" character varying(255),
        "VerificationPendingTier" integer,
        "StripeCustomerId" character varying(255),
        "CreatedUtc" timestamp with time zone NOT NULL,
        "UpdatedUtc" timestamp with time zone,
        "CreatedByUserId" uuid,
        "UpdatedByUserId" uuid,
        CONSTRAINT "PK_Organisations" PRIMARY KEY ("Id"),
        CONSTRAINT "FK_Organisations_OrganisationDomains_PrimaryDomainId" FOREIGN KEY ("PrimaryDomainId") REFERENCES aethon."OrganisationDomains" ("Id") ON DELETE RESTRICT
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260416132819_InitDb') THEN
    CREATE TABLE aethon."OrganisationInvitations" (
        "Id" uuid NOT NULL,
        "Type" integer NOT NULL,
        "Status" integer NOT NULL,
        "OrganisationId" uuid NOT NULL,
        "Email" character varying(320) NOT NULL,
        "NormalizedEmail" character varying(320) NOT NULL,
        "EmailDomain" character varying(255) NOT NULL,
        "Token" character varying(200) NOT NULL,
        "ExpiresUtc" timestamp with time zone NOT NULL,
        "CompanyRole" integer,
        "RecruiterRole" integer,
        "AllowClaimAsOwner" boolean NOT NULL,
        "AcceptedByUserId" uuid,
        "AcceptedUtc" timestamp with time zone,
        "CreatedUtc" timestamp with time zone NOT NULL,
        "UpdatedUtc" timestamp with time zone,
        "CreatedByUserId" uuid,
        "UpdatedByUserId" uuid,
        CONSTRAINT "PK_OrganisationInvitations" PRIMARY KEY ("Id"),
        CONSTRAINT "FK_OrganisationInvitations_Organisations_OrganisationId" FOREIGN KEY ("OrganisationId") REFERENCES aethon."Organisations" ("Id") ON DELETE CASCADE
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260416132819_InitDb') THEN
    CREATE TABLE aethon."OrganisationMemberProfiles" (
        "Id" uuid NOT NULL,
        "UserId" uuid NOT NULL,
        "OrganisationId" uuid NOT NULL,
        "Slug" character varying(60),
        "JobTitle" character varying(200),
        "Bio" character varying(2000),
        "ProfilePictureUrl" character varying(1000),
        "PublicEmail" character varying(256),
        "PublicPhone" character varying(50),
        "LinkedInUrl" character varying(500),
        "IsPublicProfileEnabled" boolean NOT NULL,
        "CreatedUtc" timestamp with time zone NOT NULL,
        "UpdatedUtc" timestamp with time zone,
        "CreatedByUserId" uuid,
        "UpdatedByUserId" uuid,
        CONSTRAINT "PK_OrganisationMemberProfiles" PRIMARY KEY ("Id"),
        CONSTRAINT "FK_OrganisationMemberProfiles_AspNetUsers_UserId" FOREIGN KEY ("UserId") REFERENCES aethon."AspNetUsers" ("Id") ON DELETE RESTRICT,
        CONSTRAINT "FK_OrganisationMemberProfiles_Organisations_OrganisationId" FOREIGN KEY ("OrganisationId") REFERENCES aethon."Organisations" ("Id") ON DELETE CASCADE
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260416132819_InitDb') THEN
    CREATE TABLE aethon."OrganisationMemberships" (
        "Id" uuid NOT NULL,
        "OrganisationId" uuid NOT NULL,
        "UserId" uuid NOT NULL,
        "Status" integer NOT NULL,
        "CompanyRole" integer,
        "RecruiterRole" integer,
        "IsOwner" boolean NOT NULL,
        "JoinedUtc" timestamp with time zone NOT NULL,
        "LeftUtc" timestamp with time zone,
        "CreatedUtc" timestamp with time zone NOT NULL,
        "UpdatedUtc" timestamp with time zone,
        "CreatedByUserId" uuid,
        "UpdatedByUserId" uuid,
        CONSTRAINT "PK_OrganisationMemberships" PRIMARY KEY ("Id"),
        CONSTRAINT "FK_OrganisationMemberships_AspNetUsers_UserId" FOREIGN KEY ("UserId") REFERENCES aethon."AspNetUsers" ("Id") ON DELETE RESTRICT,
        CONSTRAINT "FK_OrganisationMemberships_Organisations_OrganisationId" FOREIGN KEY ("OrganisationId") REFERENCES aethon."Organisations" ("Id") ON DELETE CASCADE
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260416132819_InitDb') THEN
    CREATE TABLE aethon."OrganisationRecruitmentPartnerships" (
        "Id" uuid NOT NULL,
        "CompanyOrganisationId" uuid NOT NULL,
        "RecruiterOrganisationId" uuid NOT NULL,
        "Status" integer NOT NULL,
        "Scope" integer NOT NULL,
        "RecruiterCanCreateUnclaimedCompanyJobs" boolean NOT NULL,
        "RecruiterCanPublishJobs" boolean NOT NULL,
        "RecruiterCanManageCandidates" boolean NOT NULL,
        "RequestedByUserId" uuid,
        "ApprovedByUserId" uuid,
        "ApprovedUtc" timestamp with time zone,
        "Notes" character varying(4000),
        "CreatedUtc" timestamp with time zone NOT NULL,
        "UpdatedUtc" timestamp with time zone,
        "CreatedByUserId" uuid,
        "UpdatedByUserId" uuid,
        CONSTRAINT "PK_OrganisationRecruitmentPartnerships" PRIMARY KEY ("Id"),
        CONSTRAINT "FK_OrganisationRecruitmentPartnerships_Organisations_CompanyOr~" FOREIGN KEY ("CompanyOrganisationId") REFERENCES aethon."Organisations" ("Id") ON DELETE RESTRICT,
        CONSTRAINT "FK_OrganisationRecruitmentPartnerships_Organisations_Recruiter~" FOREIGN KEY ("RecruiterOrganisationId") REFERENCES aethon."Organisations" ("Id") ON DELETE RESTRICT
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260416132819_InitDb') THEN
    CREATE TABLE aethon."StripePaymentEvents" (
        "Id" uuid NOT NULL,
        "StripeEventId" character varying(255) NOT NULL,
        "EventType" character varying(100) NOT NULL,
        "AmountTotal" bigint,
        "Currency" character varying(10),
        "CustomerEmail" character varying(255),
        "PayloadJson" text NOT NULL,
        "Status" integer NOT NULL,
        "InternalNotes" text,
        "CompletedByUserId" uuid,
        "CompletedUtc" timestamp with time zone,
        "OrganisationId" uuid,
        "PurchaseType" character varying(50),
        "ProductId" character varying(255),
        "PriceId" character varying(255),
        "PurchaseMetaJson" text,
        "CreatedUtc" timestamp with time zone NOT NULL,
        "UpdatedUtc" timestamp with time zone,
        "CreatedByUserId" uuid,
        "UpdatedByUserId" uuid,
        CONSTRAINT "PK_StripePaymentEvents" PRIMARY KEY ("Id"),
        CONSTRAINT "FK_StripePaymentEvents_Organisations_OrganisationId" FOREIGN KEY ("OrganisationId") REFERENCES aethon."Organisations" ("Id") ON DELETE SET NULL
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260416132819_InitDb') THEN
    CREATE TABLE aethon."WebhookSubscriptions" (
        "Id" uuid NOT NULL,
        "OrganisationId" uuid NOT NULL,
        "Name" character varying(150) NOT NULL,
        "EndpointUrl" character varying(2000) NOT NULL,
        "Secret" character varying(200) NOT NULL,
        "EventsCsv" character varying(1000) NOT NULL,
        "IsActive" boolean NOT NULL,
        "CreatedUtc" timestamp with time zone NOT NULL,
        "CreatedByUserId" uuid,
        CONSTRAINT "PK_WebhookSubscriptions" PRIMARY KEY ("Id"),
        CONSTRAINT "FK_WebhookSubscriptions_Organisations_OrganisationId" FOREIGN KEY ("OrganisationId") REFERENCES aethon."Organisations" ("Id") ON DELETE CASCADE
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260416132819_InitDb') THEN
    CREATE TABLE aethon."OrganisationJobCredits" (
        "Id" uuid NOT NULL,
        "OrganisationId" uuid NOT NULL,
        "CreditType" integer NOT NULL,
        "Source" integer NOT NULL,
        "QuantityOriginal" integer NOT NULL,
        "QuantityRemaining" integer NOT NULL,
        "ExpiresAt" timestamp with time zone,
        "ConvertedAt" timestamp with time zone,
        "StripePaymentEventId" uuid,
        "GrantedByUserId" uuid,
        "GrantNote" character varying(500),
        "CreatedUtc" timestamp with time zone NOT NULL,
        "UpdatedUtc" timestamp with time zone,
        "CreatedByUserId" uuid,
        "UpdatedByUserId" uuid,
        CONSTRAINT "PK_OrganisationJobCredits" PRIMARY KEY ("Id"),
        CONSTRAINT "FK_OrganisationJobCredits_Organisations_OrganisationId" FOREIGN KEY ("OrganisationId") REFERENCES aethon."Organisations" ("Id") ON DELETE RESTRICT,
        CONSTRAINT "FK_OrganisationJobCredits_StripePaymentEvents_StripePaymentEve~" FOREIGN KEY ("StripePaymentEventId") REFERENCES aethon."StripePaymentEvents" ("Id") ON DELETE SET NULL
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260416132819_InitDb') THEN
    CREATE TABLE aethon."WebhookDeliveries" (
        "Id" uuid NOT NULL,
        "WebhookSubscriptionId" uuid NOT NULL,
        "EventType" character varying(100) NOT NULL,
        "PayloadJson" text NOT NULL,
        "Status" character varying(50) NOT NULL,
        "AttemptCount" integer NOT NULL,
        "CreatedUtc" timestamp with time zone NOT NULL,
        "LastAttemptUtc" timestamp with time zone,
        "LastError" character varying(4000),
        CONSTRAINT "PK_WebhookDeliveries" PRIMARY KEY ("Id"),
        CONSTRAINT "FK_WebhookDeliveries_WebhookSubscriptions_WebhookSubscriptionId" FOREIGN KEY ("WebhookSubscriptionId") REFERENCES aethon."WebhookSubscriptions" ("Id") ON DELETE CASCADE
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260416132819_InitDb') THEN
    CREATE INDEX "IX_ActivityLogs_EntityType_EntityId" ON aethon."ActivityLogs" ("EntityType", "EntityId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260416132819_InitDb') THEN
    CREATE INDEX "IX_ActivityLogs_OrganisationId" ON aethon."ActivityLogs" ("OrganisationId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260416132819_InitDb') THEN
    CREATE INDEX "IX_ActivityLogs_PerformedByUserId" ON aethon."ActivityLogs" ("PerformedByUserId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260416132819_InitDb') THEN
    CREATE INDEX "IX_ActivityLogs_PerformedUtc" ON aethon."ActivityLogs" ("PerformedUtc");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260416132819_InitDb') THEN
    CREATE INDEX "IX_AspNetRoleClaims_RoleId" ON aethon."AspNetRoleClaims" ("RoleId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260416132819_InitDb') THEN
    CREATE UNIQUE INDEX "RoleNameIndex" ON aethon."AspNetRoles" ("NormalizedName");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260416132819_InitDb') THEN
    CREATE INDEX "IX_AspNetUserClaims_UserId" ON aethon."AspNetUserClaims" ("UserId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260416132819_InitDb') THEN
    CREATE INDEX "IX_AspNetUserLogins_UserId" ON aethon."AspNetUserLogins" ("UserId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260416132819_InitDb') THEN
    CREATE INDEX "IX_AspNetUserRoles_RoleId" ON aethon."AspNetUserRoles" ("RoleId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260416132819_InitDb') THEN
    CREATE INDEX "EmailIndex" ON aethon."AspNetUsers" ("NormalizedEmail");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260416132819_InitDb') THEN
    CREATE INDEX "IX_AspNetUsers_IsEnabled" ON aethon."AspNetUsers" ("IsEnabled");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260416132819_InitDb') THEN
    CREATE INDEX "IX_AspNetUsers_IsIdentityVerified" ON aethon."AspNetUsers" ("IsIdentityVerified");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260416132819_InitDb') THEN
    CREATE INDEX "IX_AspNetUsers_IsPhoneNumberVerified" ON aethon."AspNetUsers" ("IsPhoneNumberVerified");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260416132819_InitDb') THEN
    CREATE INDEX "IX_AspNetUsers_UserType" ON aethon."AspNetUsers" ("UserType");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260416132819_InitDb') THEN
    CREATE UNIQUE INDEX "UserNameIndex" ON aethon."AspNetUsers" ("NormalizedUserName");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260416132819_InitDb') THEN
    CREATE INDEX "IX_AtsMatchQueue_JobApplicationId" ON aethon."AtsMatchQueue" ("JobApplicationId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260416132819_InitDb') THEN
    CREATE UNIQUE INDEX "IX_AtsMatchResults_AtsMatchQueueItemId" ON aethon."AtsMatchResults" ("AtsMatchQueueItemId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260416132819_InitDb') THEN
    CREATE INDEX "IX_AtsMatchResults_JobApplicationId" ON aethon."AtsMatchResults" ("JobApplicationId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260416132819_InitDb') THEN
    CREATE INDEX "IX_CreditConsumptionLogs_JobId" ON aethon."CreditConsumptionLogs" ("JobId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260416132819_InitDb') THEN
    CREATE INDEX "IX_CreditConsumptionLogs_OrganisationId" ON aethon."CreditConsumptionLogs" ("OrganisationId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260416132819_InitDb') THEN
    CREATE INDEX "IX_CreditConsumptionLogs_OrganisationJobCreditId" ON aethon."CreditConsumptionLogs" ("OrganisationJobCreditId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260416132819_InitDb') THEN
    CREATE INDEX "IX_IdentityVerificationRequests_ReviewedByUserId" ON aethon."IdentityVerificationRequests" ("ReviewedByUserId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260416132819_InitDb') THEN
    CREATE INDEX "IX_IdentityVerificationRequests_Status" ON aethon."IdentityVerificationRequests" ("Status");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260416132819_InitDb') THEN
    CREATE INDEX "IX_IdentityVerificationRequests_UserId" ON aethon."IdentityVerificationRequests" ("UserId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260416132819_InitDb') THEN
    CREATE UNIQUE INDEX "IX_IdentityVerificationRequests_UserId_Status" ON aethon."IdentityVerificationRequests" ("UserId", "Status") WHERE "Status" = 1;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260416132819_InitDb') THEN
    CREATE INDEX "IX_JobApplicationAttachments_JobApplicationId" ON aethon."JobApplicationAttachments" ("JobApplicationId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260416132819_InitDb') THEN
    CREATE INDEX "IX_JobApplicationAttachments_StoredFileId" ON aethon."JobApplicationAttachments" ("StoredFileId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260416132819_InitDb') THEN
    CREATE INDEX "IX_JobApplicationComments_CreatedByUserId" ON aethon."JobApplicationComments" ("CreatedByUserId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260416132819_InitDb') THEN
    CREATE INDEX "IX_JobApplicationComments_DeletedByUserId" ON aethon."JobApplicationComments" ("DeletedByUserId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260416132819_InitDb') THEN
    CREATE INDEX "IX_JobApplicationComments_JobApplicationId" ON aethon."JobApplicationComments" ("JobApplicationId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260416132819_InitDb') THEN
    CREATE INDEX "IX_JobApplicationComments_ParentCommentId" ON aethon."JobApplicationComments" ("ParentCommentId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260416132819_InitDb') THEN
    CREATE INDEX "IX_JobApplicationComments_UpdatedByUserId" ON aethon."JobApplicationComments" ("UpdatedByUserId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260416132819_InitDb') THEN
    CREATE UNIQUE INDEX "IX_JobApplicationInterviewInterviewers_JobApplicationInterview~" ON aethon."JobApplicationInterviewInterviewers" ("JobApplicationInterviewId", "UserId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260416132819_InitDb') THEN
    CREATE INDEX "IX_JobApplicationInterviewInterviewers_UserId" ON aethon."JobApplicationInterviewInterviewers" ("UserId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260416132819_InitDb') THEN
    CREATE INDEX "IX_JobApplicationInterviews_JobApplicationId" ON aethon."JobApplicationInterviews" ("JobApplicationId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260416132819_InitDb') THEN
    CREATE INDEX "IX_JobApplicationInterviews_JobApplicationId_Status" ON aethon."JobApplicationInterviews" ("JobApplicationId", "Status");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260416132819_InitDb') THEN
    CREATE INDEX "IX_JobApplicationInterviews_ScheduledStartUtc" ON aethon."JobApplicationInterviews" ("ScheduledStartUtc");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260416132819_InitDb') THEN
    CREATE INDEX "IX_JobApplicationNotes_CreatedByUserId" ON aethon."JobApplicationNotes" ("CreatedByUserId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260416132819_InitDb') THEN
    CREATE INDEX "IX_JobApplicationNotes_DeletedByUserId" ON aethon."JobApplicationNotes" ("DeletedByUserId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260416132819_InitDb') THEN
    CREATE INDEX "IX_JobApplicationNotes_JobApplicationId" ON aethon."JobApplicationNotes" ("JobApplicationId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260416132819_InitDb') THEN
    CREATE INDEX "IX_JobApplicationNotes_UpdatedByUserId" ON aethon."JobApplicationNotes" ("UpdatedByUserId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260416132819_InitDb') THEN
    CREATE INDEX "IX_JobApplications_AssignedRecruiterUserId" ON aethon."JobApplications" ("AssignedRecruiterUserId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260416132819_InitDb') THEN
    CREATE INDEX "IX_JobApplications_AssignedToUserId" ON aethon."JobApplications" ("AssignedToUserId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260416132819_InitDb') THEN
    CREATE INDEX "IX_JobApplications_DuplicateOfApplicationId" ON aethon."JobApplications" ("DuplicateOfApplicationId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260416132819_InitDb') THEN
    CREATE INDEX "IX_JobApplications_IsArchived" ON aethon."JobApplications" ("IsArchived");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260416132819_InitDb') THEN
    CREATE INDEX "IX_JobApplications_IsHired" ON aethon."JobApplications" ("IsHired");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260416132819_InitDb') THEN
    CREATE INDEX "IX_JobApplications_IsRejected" ON aethon."JobApplications" ("IsRejected");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260416132819_InitDb') THEN
    CREATE INDEX "IX_JobApplications_IsWithdrawn" ON aethon."JobApplications" ("IsWithdrawn");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260416132819_InitDb') THEN
    CREATE INDEX "IX_JobApplications_JobId_Status" ON aethon."JobApplications" ("JobId", "Status");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260416132819_InitDb') THEN
    CREATE INDEX "IX_JobApplications_JobId_Status_SubmittedUtc" ON aethon."JobApplications" ("JobId", "Status", "SubmittedUtc");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260416132819_InitDb') THEN
    CREATE INDEX "IX_JobApplications_JobId_SubmittedUtc" ON aethon."JobApplications" ("JobId", "SubmittedUtc");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260416132819_InitDb') THEN
    CREATE UNIQUE INDEX "IX_JobApplications_JobId_UserId" ON aethon."JobApplications" ("JobId", "UserId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260416132819_InitDb') THEN
    CREATE INDEX "IX_JobApplications_LastActivityUtc" ON aethon."JobApplications" ("LastActivityUtc");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260416132819_InitDb') THEN
    CREATE INDEX "IX_JobApplications_LastStatusChangedUtc" ON aethon."JobApplications" ("LastStatusChangedUtc");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260416132819_InitDb') THEN
    CREATE INDEX "IX_JobApplications_RejectedByUserId" ON aethon."JobApplications" ("RejectedByUserId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260416132819_InitDb') THEN
    CREATE INDEX "IX_JobApplications_ResumeFileId" ON aethon."JobApplications" ("ResumeFileId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260416132819_InitDb') THEN
    CREATE INDEX "IX_JobApplications_SubmittedUtc" ON aethon."JobApplications" ("SubmittedUtc");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260416132819_InitDb') THEN
    CREATE INDEX "IX_JobApplications_UserId" ON aethon."JobApplications" ("UserId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260416132819_InitDb') THEN
    CREATE INDEX "IX_JobApplications_UserId_Status_SubmittedUtc" ON aethon."JobApplications" ("UserId", "Status", "SubmittedUtc");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260416132819_InitDb') THEN
    CREATE INDEX "IX_JobApplications_UserId_SubmittedUtc" ON aethon."JobApplications" ("UserId", "SubmittedUtc");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260416132819_InitDb') THEN
    CREATE INDEX "IX_JobApplications_WithdrawnByUserId" ON aethon."JobApplications" ("WithdrawnByUserId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260416132819_InitDb') THEN
    CREATE INDEX "IX_JobApplicationStatusHistory_ChangedByUserId" ON aethon."JobApplicationStatusHistory" ("ChangedByUserId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260416132819_InitDb') THEN
    CREATE INDEX "IX_JobApplicationStatusHistory_ChangedUtc" ON aethon."JobApplicationStatusHistory" ("ChangedUtc");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260416132819_InitDb') THEN
    CREATE INDEX "IX_JobApplicationStatusHistory_JobApplicationId" ON aethon."JobApplicationStatusHistory" ("JobApplicationId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260416132819_InitDb') THEN
    CREATE INDEX "IX_JobApplicationStatusHistory_JobApplicationId_ChangedUtc" ON aethon."JobApplicationStatusHistory" ("JobApplicationId", "ChangedUtc");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260416132819_InitDb') THEN
    CREATE INDEX "IX_Jobs_CreatedByIdentityUserId" ON aethon."Jobs" ("CreatedByIdentityUserId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260416132819_InitDb') THEN
    CREATE INDEX "IX_Jobs_ManagedByOrganisationId" ON aethon."Jobs" ("ManagedByOrganisationId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260416132819_InitDb') THEN
    CREATE INDEX "IX_Jobs_ManagedByUserId" ON aethon."Jobs" ("ManagedByUserId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260416132819_InitDb') THEN
    CREATE INDEX "IX_Jobs_OrganisationRecruitmentPartnershipId" ON aethon."Jobs" ("OrganisationRecruitmentPartnershipId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260416132819_InitDb') THEN
    CREATE INDEX "IX_Jobs_OwnedByOrganisationId" ON aethon."Jobs" ("OwnedByOrganisationId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260416132819_InitDb') THEN
    CREATE INDEX "IX_Jobs_OwnedByOrganisationId_Status" ON aethon."Jobs" ("OwnedByOrganisationId", "Status");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260416132819_InitDb') THEN
    CREATE INDEX "IX_Jobs_OwnedByOrganisationId_Visibility" ON aethon."Jobs" ("OwnedByOrganisationId", "Visibility");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260416132819_InitDb') THEN
    CREATE INDEX "IX_Jobs_PublishedUtc" ON aethon."Jobs" ("PublishedUtc");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260416132819_InitDb') THEN
    CREATE INDEX "IX_Jobs_ReferenceCode" ON aethon."Jobs" ("ReferenceCode");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260416132819_InitDb') THEN
    CREATE INDEX "IX_JobSeekerCertificates_JobSeekerProfileId" ON aethon."JobSeekerCertificates" ("JobSeekerProfileId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260416132819_InitDb') THEN
    CREATE INDEX "IX_JobSeekerLanguages_JobSeekerProfileId" ON aethon."JobSeekerLanguages" ("JobSeekerProfileId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260416132819_InitDb') THEN
    CREATE INDEX "IX_JobSeekerLanguages_JobSeekerProfileId_Name_AbilityType" ON aethon."JobSeekerLanguages" ("JobSeekerProfileId", "Name", "AbilityType");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260416132819_InitDb') THEN
    CREATE INDEX "IX_JobSeekerNationalities_JobSeekerProfileId" ON aethon."JobSeekerNationalities" ("JobSeekerProfileId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260416132819_InitDb') THEN
    CREATE INDEX "IX_JobSeekerNationalities_JobSeekerProfileId_Name" ON aethon."JobSeekerNationalities" ("JobSeekerProfileId", "Name");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260416132819_InitDb') THEN
    CREATE INDEX "IX_JobSeekerProfiles_IsPublicProfileEnabled" ON aethon."JobSeekerProfiles" ("IsPublicProfileEnabled");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260416132819_InitDb') THEN
    CREATE INDEX "IX_JobSeekerProfiles_IsSearchable" ON aethon."JobSeekerProfiles" ("IsSearchable");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260416132819_InitDb') THEN
    CREATE INDEX "IX_JobSeekerProfiles_OpenToWork" ON aethon."JobSeekerProfiles" ("OpenToWork");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260416132819_InitDb') THEN
    CREATE UNIQUE INDEX "IX_JobSeekerProfiles_Slug" ON aethon."JobSeekerProfiles" ("Slug") WHERE "Slug" IS NOT NULL;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260416132819_InitDb') THEN
    CREATE UNIQUE INDEX "IX_JobSeekerProfiles_UserId" ON aethon."JobSeekerProfiles" ("UserId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260416132819_InitDb') THEN
    CREATE INDEX "IX_JobSeekerQualifications_JobSeekerProfileId" ON aethon."JobSeekerQualifications" ("JobSeekerProfileId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260416132819_InitDb') THEN
    CREATE INDEX "IX_JobSeekerResumes_JobSeekerProfileId" ON aethon."JobSeekerResumes" ("JobSeekerProfileId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260416132819_InitDb') THEN
    CREATE INDEX "IX_JobSeekerResumes_JobSeekerProfileId_IsActive_IsDefault" ON aethon."JobSeekerResumes" ("JobSeekerProfileId", "IsActive", "IsDefault");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260416132819_InitDb') THEN
    CREATE INDEX "IX_JobSeekerResumes_JobSeekerProfileId_IsDefault" ON aethon."JobSeekerResumes" ("JobSeekerProfileId", "IsDefault");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260416132819_InitDb') THEN
    CREATE INDEX "IX_JobSeekerResumes_JobSeekerProfileId_StoredFileId_IsActive" ON aethon."JobSeekerResumes" ("JobSeekerProfileId", "StoredFileId", "IsActive");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260416132819_InitDb') THEN
    CREATE INDEX "IX_JobSeekerResumes_StoredFileId" ON aethon."JobSeekerResumes" ("StoredFileId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260416132819_InitDb') THEN
    CREATE INDEX "IX_JobSeekerSkills_JobSeekerProfileId" ON aethon."JobSeekerSkills" ("JobSeekerProfileId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260416132819_InitDb') THEN
    CREATE INDEX "IX_JobSeekerWorkExperiences_JobSeekerProfileId" ON aethon."JobSeekerWorkExperiences" ("JobSeekerProfileId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260416132819_InitDb') THEN
    CREATE INDEX "IX_JobSyndicationRecords_JobId_Provider" ON aethon."JobSyndicationRecords" ("JobId", "Provider");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260416132819_InitDb') THEN
    CREATE INDEX "IX_JobSyndicationRecords_SubmittedUtc" ON aethon."JobSyndicationRecords" ("SubmittedUtc");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260416132819_InitDb') THEN
    CREATE INDEX "IX_Locations_DisplayName" ON aethon."Locations" ("DisplayName");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260416132819_InitDb') THEN
    CREATE INDEX "IX_Locations_IsActive_SortOrder" ON aethon."Locations" ("IsActive", "SortOrder");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260416132819_InitDb') THEN
    CREATE INDEX "IX_OrganisationClaimRequests_EmailDomain" ON aethon."OrganisationClaimRequests" ("EmailDomain");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260416132819_InitDb') THEN
    CREATE INDEX "IX_OrganisationClaimRequests_OrganisationId" ON aethon."OrganisationClaimRequests" ("OrganisationId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260416132819_InitDb') THEN
    CREATE INDEX "IX_OrganisationClaimRequests_RequestedByUserId" ON aethon."OrganisationClaimRequests" ("RequestedByUserId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260416132819_InitDb') THEN
    CREATE INDEX "IX_OrganisationClaimRequests_Status" ON aethon."OrganisationClaimRequests" ("Status");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260416132819_InitDb') THEN
    CREATE INDEX "IX_OrganisationClaimRequests_VerificationToken" ON aethon."OrganisationClaimRequests" ("VerificationToken");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260416132819_InitDb') THEN
    CREATE UNIQUE INDEX "IX_OrganisationDomains_NormalizedDomain" ON aethon."OrganisationDomains" ("NormalizedDomain");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260416132819_InitDb') THEN
    CREATE INDEX "IX_OrganisationDomains_OrganisationId" ON aethon."OrganisationDomains" ("OrganisationId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260416132819_InitDb') THEN
    CREATE INDEX "IX_OrganisationDomains_OrganisationId_IsPrimary" ON aethon."OrganisationDomains" ("OrganisationId", "IsPrimary");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260416132819_InitDb') THEN
    CREATE INDEX "IX_OrganisationDomains_Status" ON aethon."OrganisationDomains" ("Status");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260416132819_InitDb') THEN
    CREATE INDEX "IX_OrganisationInvitations_EmailDomain" ON aethon."OrganisationInvitations" ("EmailDomain");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260416132819_InitDb') THEN
    CREATE INDEX "IX_OrganisationInvitations_ExpiresUtc" ON aethon."OrganisationInvitations" ("ExpiresUtc");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260416132819_InitDb') THEN
    CREATE INDEX "IX_OrganisationInvitations_NormalizedEmail" ON aethon."OrganisationInvitations" ("NormalizedEmail");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260416132819_InitDb') THEN
    CREATE INDEX "IX_OrganisationInvitations_OrganisationId" ON aethon."OrganisationInvitations" ("OrganisationId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260416132819_InitDb') THEN
    CREATE INDEX "IX_OrganisationInvitations_Status" ON aethon."OrganisationInvitations" ("Status");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260416132819_InitDb') THEN
    CREATE UNIQUE INDEX "IX_OrganisationInvitations_Token" ON aethon."OrganisationInvitations" ("Token");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260416132819_InitDb') THEN
    CREATE INDEX "IX_OrganisationJobCredits_OrganisationId" ON aethon."OrganisationJobCredits" ("OrganisationId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260416132819_InitDb') THEN
    CREATE INDEX "IX_OrganisationJobCredits_OrganisationId_CreditType_QuantityRe~" ON aethon."OrganisationJobCredits" ("OrganisationId", "CreditType", "QuantityRemaining");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260416132819_InitDb') THEN
    CREATE INDEX "IX_OrganisationJobCredits_StripePaymentEventId" ON aethon."OrganisationJobCredits" ("StripePaymentEventId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260416132819_InitDb') THEN
    CREATE INDEX "IX_OrganisationMemberProfiles_OrganisationId_IsPublicProfileEn~" ON aethon."OrganisationMemberProfiles" ("OrganisationId", "IsPublicProfileEnabled");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260416132819_InitDb') THEN
    CREATE UNIQUE INDEX "IX_OrganisationMemberProfiles_OrganisationId_Slug" ON aethon."OrganisationMemberProfiles" ("OrganisationId", "Slug") WHERE "Slug" IS NOT NULL;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260416132819_InitDb') THEN
    CREATE UNIQUE INDEX "IX_OrganisationMemberProfiles_OrganisationId_UserId" ON aethon."OrganisationMemberProfiles" ("OrganisationId", "UserId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260416132819_InitDb') THEN
    CREATE INDEX "IX_OrganisationMemberProfiles_UserId" ON aethon."OrganisationMemberProfiles" ("UserId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260416132819_InitDb') THEN
    CREATE INDEX "IX_OrganisationMemberships_OrganisationId_Status" ON aethon."OrganisationMemberships" ("OrganisationId", "Status");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260416132819_InitDb') THEN
    CREATE UNIQUE INDEX "IX_OrganisationMemberships_OrganisationId_UserId" ON aethon."OrganisationMemberships" ("OrganisationId", "UserId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260416132819_InitDb') THEN
    CREATE INDEX "IX_OrganisationMemberships_UserId" ON aethon."OrganisationMemberships" ("UserId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260416132819_InitDb') THEN
    CREATE INDEX "IX_OrganisationMemberships_UserId_OrganisationId_Status" ON aethon."OrganisationMemberships" ("UserId", "OrganisationId", "Status");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260416132819_InitDb') THEN
    CREATE INDEX "IX_OrganisationRecruitmentPartnerships_ApprovedByUserId" ON aethon."OrganisationRecruitmentPartnerships" ("ApprovedByUserId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260416132819_InitDb') THEN
    CREATE UNIQUE INDEX "IX_OrganisationRecruitmentPartnerships_CompanyOrganisationId_R~" ON aethon."OrganisationRecruitmentPartnerships" ("CompanyOrganisationId", "RecruiterOrganisationId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260416132819_InitDb') THEN
    CREATE INDEX "IX_OrganisationRecruitmentPartnerships_RecruiterOrganisationId" ON aethon."OrganisationRecruitmentPartnerships" ("RecruiterOrganisationId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260416132819_InitDb') THEN
    CREATE INDEX "IX_OrganisationRecruitmentPartnerships_RequestedByUserId" ON aethon."OrganisationRecruitmentPartnerships" ("RequestedByUserId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260416132819_InitDb') THEN
    CREATE INDEX "IX_OrganisationRecruitmentPartnerships_Status" ON aethon."OrganisationRecruitmentPartnerships" ("Status");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260416132819_InitDb') THEN
    CREATE INDEX "IX_Organisations_NormalizedName" ON aethon."Organisations" ("NormalizedName");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260416132819_InitDb') THEN
    CREATE INDEX "IX_Organisations_PrimaryDomainId" ON aethon."Organisations" ("PrimaryDomainId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260416132819_InitDb') THEN
    CREATE UNIQUE INDEX "IX_Organisations_Slug" ON aethon."Organisations" ("Slug") WHERE "Slug" IS NOT NULL;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260416132819_InitDb') THEN
    CREATE INDEX "IX_Organisations_Type_Status" ON aethon."Organisations" ("Type", "Status");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260416132819_InitDb') THEN
    CREATE INDEX "IX_Organisations_VerificationTier" ON aethon."Organisations" ("VerificationTier");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260416132819_InitDb') THEN
    CREATE UNIQUE INDEX "IX_ResumeAnalyses_JobSeekerResumeId" ON aethon."ResumeAnalyses" ("JobSeekerResumeId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260416132819_InitDb') THEN
    CREATE INDEX "IX_ResumeAnalyses_Status" ON aethon."ResumeAnalyses" ("Status");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260416132819_InitDb') THEN
    CREATE INDEX "IX_ResumeAnalyses_StoredFileId" ON aethon."ResumeAnalyses" ("StoredFileId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260416132819_InitDb') THEN
    CREATE UNIQUE INDEX "IX_StoredFiles_StoragePath" ON aethon."StoredFiles" ("StoragePath");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260416132819_InitDb') THEN
    CREATE INDEX "IX_StoredFiles_UploadedByUserId" ON aethon."StoredFiles" ("UploadedByUserId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260416132819_InitDb') THEN
    CREATE INDEX "IX_StoredFiles_UploadedByUserId_CreatedUtc" ON aethon."StoredFiles" ("UploadedByUserId", "CreatedUtc");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260416132819_InitDb') THEN
    CREATE INDEX "IX_StripePaymentEvents_OrganisationId" ON aethon."StripePaymentEvents" ("OrganisationId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260416132819_InitDb') THEN
    CREATE UNIQUE INDEX "IX_StripePaymentEvents_StripeEventId" ON aethon."StripePaymentEvents" ("StripeEventId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260416132819_InitDb') THEN
    CREATE INDEX "IX_WebhookDeliveries_WebhookSubscriptionId" ON aethon."WebhookDeliveries" ("WebhookSubscriptionId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260416132819_InitDb') THEN
    CREATE INDEX "IX_WebhookSubscriptions_OrganisationId" ON aethon."WebhookSubscriptions" ("OrganisationId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260416132819_InitDb') THEN
    ALTER TABLE aethon."ActivityLogs" ADD CONSTRAINT "FK_ActivityLogs_Organisations_OrganisationId" FOREIGN KEY ("OrganisationId") REFERENCES aethon."Organisations" ("Id") ON DELETE RESTRICT;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260416132819_InitDb') THEN
    ALTER TABLE aethon."AtsMatchQueue" ADD CONSTRAINT "FK_AtsMatchQueue_JobApplications_JobApplicationId" FOREIGN KEY ("JobApplicationId") REFERENCES aethon."JobApplications" ("Id") ON DELETE CASCADE;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260416132819_InitDb') THEN
    ALTER TABLE aethon."AtsMatchResults" ADD CONSTRAINT "FK_AtsMatchResults_JobApplications_JobApplicationId" FOREIGN KEY ("JobApplicationId") REFERENCES aethon."JobApplications" ("Id") ON DELETE CASCADE;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260416132819_InitDb') THEN
    ALTER TABLE aethon."CreditConsumptionLogs" ADD CONSTRAINT "FK_CreditConsumptionLogs_Jobs_JobId" FOREIGN KEY ("JobId") REFERENCES aethon."Jobs" ("Id") ON DELETE RESTRICT;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260416132819_InitDb') THEN
    ALTER TABLE aethon."CreditConsumptionLogs" ADD CONSTRAINT "FK_CreditConsumptionLogs_OrganisationJobCredits_OrganisationJo~" FOREIGN KEY ("OrganisationJobCreditId") REFERENCES aethon."OrganisationJobCredits" ("Id") ON DELETE RESTRICT;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260416132819_InitDb') THEN
    ALTER TABLE aethon."JobApplicationAttachments" ADD CONSTRAINT "FK_JobApplicationAttachments_JobApplications_JobApplicationId" FOREIGN KEY ("JobApplicationId") REFERENCES aethon."JobApplications" ("Id") ON DELETE CASCADE;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260416132819_InitDb') THEN
    ALTER TABLE aethon."JobApplicationComments" ADD CONSTRAINT "FK_JobApplicationComments_JobApplications_JobApplicationId" FOREIGN KEY ("JobApplicationId") REFERENCES aethon."JobApplications" ("Id") ON DELETE CASCADE;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260416132819_InitDb') THEN
    ALTER TABLE aethon."JobApplicationInterviewInterviewers" ADD CONSTRAINT "FK_JobApplicationInterviewInterviewers_JobApplicationInterview~" FOREIGN KEY ("JobApplicationInterviewId") REFERENCES aethon."JobApplicationInterviews" ("Id") ON DELETE CASCADE;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260416132819_InitDb') THEN
    ALTER TABLE aethon."JobApplicationInterviews" ADD CONSTRAINT "FK_JobApplicationInterviews_JobApplications_JobApplicationId" FOREIGN KEY ("JobApplicationId") REFERENCES aethon."JobApplications" ("Id") ON DELETE CASCADE;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260416132819_InitDb') THEN
    ALTER TABLE aethon."JobApplicationNotes" ADD CONSTRAINT "FK_JobApplicationNotes_JobApplications_JobApplicationId" FOREIGN KEY ("JobApplicationId") REFERENCES aethon."JobApplications" ("Id") ON DELETE CASCADE;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260416132819_InitDb') THEN
    ALTER TABLE aethon."JobApplications" ADD CONSTRAINT "FK_JobApplications_Jobs_JobId" FOREIGN KEY ("JobId") REFERENCES aethon."Jobs" ("Id") ON DELETE CASCADE;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260416132819_InitDb') THEN
    ALTER TABLE aethon."Jobs" ADD CONSTRAINT "FK_Jobs_OrganisationRecruitmentPartnerships_OrganisationRecrui~" FOREIGN KEY ("OrganisationRecruitmentPartnershipId") REFERENCES aethon."OrganisationRecruitmentPartnerships" ("Id") ON DELETE RESTRICT;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260416132819_InitDb') THEN
    ALTER TABLE aethon."Jobs" ADD CONSTRAINT "FK_Jobs_Organisations_ManagedByOrganisationId" FOREIGN KEY ("ManagedByOrganisationId") REFERENCES aethon."Organisations" ("Id") ON DELETE RESTRICT;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260416132819_InitDb') THEN
    ALTER TABLE aethon."Jobs" ADD CONSTRAINT "FK_Jobs_Organisations_OwnedByOrganisationId" FOREIGN KEY ("OwnedByOrganisationId") REFERENCES aethon."Organisations" ("Id") ON DELETE RESTRICT;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260416132819_InitDb') THEN
    ALTER TABLE aethon."OrganisationClaimRequests" ADD CONSTRAINT "FK_OrganisationClaimRequests_Organisations_OrganisationId" FOREIGN KEY ("OrganisationId") REFERENCES aethon."Organisations" ("Id") ON DELETE CASCADE;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260416132819_InitDb') THEN
    ALTER TABLE aethon."OrganisationDomains" ADD CONSTRAINT "FK_OrganisationDomains_Organisations_OrganisationId" FOREIGN KEY ("OrganisationId") REFERENCES aethon."Organisations" ("Id") ON DELETE CASCADE;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260416132819_InitDb') THEN
    INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
    VALUES ('20260416132819_InitDb', '10.0.5');
    END IF;
END $EF$;
COMMIT;

