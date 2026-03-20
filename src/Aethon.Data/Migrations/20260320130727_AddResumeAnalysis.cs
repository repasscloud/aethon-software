using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Aethon.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddResumeAnalysis : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
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
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ResumeAnalyses");
        }
    }
}
