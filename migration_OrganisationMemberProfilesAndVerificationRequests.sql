-- ============================================================
-- Migration: OrganisationMemberProfilesAndVerificationRequests
-- Run this against your database before deploying the new build.
-- Safe to run multiple times (uses IF NOT EXISTS / IF EXISTS guards).
-- ============================================================

-- ============================================================
-- TABLE: OrganisationMemberProfiles
-- ============================================================
IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'OrganisationMemberProfiles')
BEGIN
    CREATE TABLE [OrganisationMemberProfiles] (
        [Id]                    UNIQUEIDENTIFIER    NOT NULL    DEFAULT NEWID(),
        [UserId]                UNIQUEIDENTIFIER    NOT NULL,
        [OrganisationId]        UNIQUEIDENTIFIER    NOT NULL,
        [Slug]                  NVARCHAR(60)        NULL,
        [JobTitle]              NVARCHAR(200)       NULL,
        [Bio]                   NVARCHAR(2000)      NULL,
        [ProfilePictureUrl]     NVARCHAR(1000)      NULL,
        [PublicEmail]           NVARCHAR(256)       NULL,
        [PublicPhone]           NVARCHAR(50)        NULL,
        [LinkedInUrl]           NVARCHAR(500)       NULL,
        [IsPublicProfileEnabled] BIT                NOT NULL    DEFAULT 0,
        [CreatedUtc]            DATETIME2           NOT NULL,
        [UpdatedUtc]            DATETIME2           NULL,
        [CreatedByUserId]       UNIQUEIDENTIFIER    NULL,
        [UpdatedByUserId]       UNIQUEIDENTIFIER    NULL,

        CONSTRAINT [PK_OrganisationMemberProfiles] PRIMARY KEY ([Id]),

        CONSTRAINT [FK_OrganisationMemberProfiles_AspNetUsers_UserId]
            FOREIGN KEY ([UserId]) REFERENCES [AspNetUsers] ([Id])
            ON DELETE NO ACTION,

        CONSTRAINT [FK_OrganisationMemberProfiles_Organisations_OrganisationId]
            FOREIGN KEY ([OrganisationId]) REFERENCES [Organisations] ([Id])
            ON DELETE CASCADE
    );

    -- One profile per user per organisation
    CREATE UNIQUE INDEX [IX_OrganisationMemberProfiles_OrganisationId_UserId]
        ON [OrganisationMemberProfiles] ([OrganisationId], [UserId]);

    -- Slug unique within an org (nulls excluded)
    CREATE UNIQUE INDEX [IX_OrganisationMemberProfiles_OrganisationId_Slug]
        ON [OrganisationMemberProfiles] ([OrganisationId], [Slug])
        WHERE [Slug] IS NOT NULL;

    -- Fast lookup for public team page
    CREATE INDEX [IX_OrganisationMemberProfiles_OrganisationId_IsPublicProfileEnabled]
        ON [OrganisationMemberProfiles] ([OrganisationId], [IsPublicProfileEnabled]);

    PRINT 'Created table OrganisationMemberProfiles';
END
ELSE
BEGIN
    PRINT 'Table OrganisationMemberProfiles already exists — skipped';
END
GO

-- ============================================================
-- TABLE: IdentityVerificationRequests
-- Status values: 1=Pending, 2=Approved, 3=Denied
-- ReviewerType values: 1=System, 2=OrgOwner, 3=Admin
-- ============================================================
IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'IdentityVerificationRequests')
BEGIN
    CREATE TABLE [IdentityVerificationRequests] (
        [Id]                UNIQUEIDENTIFIER    NOT NULL    DEFAULT NEWID(),
        [UserId]            UNIQUEIDENTIFIER    NOT NULL,
        [FullName]          NVARCHAR(300)       NOT NULL,
        [EmailAddress]      NVARCHAR(256)       NOT NULL,
        [PhoneNumber]       NVARCHAR(50)        NOT NULL,
        [AdditionalNotes]   NVARCHAR(2000)      NULL,
        [Status]            INT                 NOT NULL    DEFAULT 1,
        [ReviewedByUserId]  UNIQUEIDENTIFIER    NULL,
        [ReviewedUtc]       DATETIME2           NULL,
        [ReviewNotes]       NVARCHAR(2000)      NULL,
        [ReviewerType]      INT                 NULL,
        [CreatedUtc]        DATETIME2           NOT NULL,
        [UpdatedUtc]        DATETIME2           NULL,
        [CreatedByUserId]   UNIQUEIDENTIFIER    NULL,
        [UpdatedByUserId]   UNIQUEIDENTIFIER    NULL,

        CONSTRAINT [PK_IdentityVerificationRequests] PRIMARY KEY ([Id]),

        CONSTRAINT [FK_IdentityVerificationRequests_AspNetUsers_UserId]
            FOREIGN KEY ([UserId]) REFERENCES [AspNetUsers] ([Id])
            ON DELETE NO ACTION,

        CONSTRAINT [FK_IdentityVerificationRequests_AspNetUsers_ReviewedByUserId]
            FOREIGN KEY ([ReviewedByUserId]) REFERENCES [AspNetUsers] ([Id])
            ON DELETE SET NULL
    );

    -- One pending request per user at a time
    CREATE UNIQUE INDEX [IX_IdentityVerificationRequests_UserId_Status_Pending]
        ON [IdentityVerificationRequests] ([UserId], [Status])
        WHERE [Status] = 1;

    -- Fast lookup for admin queue
    CREATE INDEX [IX_IdentityVerificationRequests_Status]
        ON [IdentityVerificationRequests] ([Status]);

    -- Fast lookup by user
    CREATE INDEX [IX_IdentityVerificationRequests_UserId]
        ON [IdentityVerificationRequests] ([UserId]);

    PRINT 'Created table IdentityVerificationRequests';
END
ELSE
BEGIN
    PRINT 'Table IdentityVerificationRequests already exists — skipped';
END
GO
