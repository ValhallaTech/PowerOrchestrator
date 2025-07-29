using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PowerOrchestrator.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddGitHubIntegration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "github_repositories",
                schema: "powerorchestrator",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    owner = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    full_name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    description = table.Column<string>(type: "text", nullable: true),
                    is_private = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    default_branch = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false, defaultValue: "main"),
                    last_sync_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    status = table.Column<string>(type: "text", nullable: false, defaultValue: "Active"),
                    configuration = table.Column<string>(type: "jsonb", nullable: false, defaultValue: "{}"),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    created_by = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    updated_by = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    row_version = table.Column<byte[]>(type: "bytea", rowVersion: true, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_github_repositories", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "repository_scripts",
                schema: "powerorchestrator",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    repository_id = table.Column<Guid>(type: "uuid", nullable: false),
                    script_id = table.Column<Guid>(type: "uuid", nullable: false),
                    file_path = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    branch = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false, defaultValue: "main"),
                    sha = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                    metadata = table.Column<string>(type: "jsonb", nullable: false, defaultValue: "{}"),
                    security_analysis = table.Column<string>(type: "jsonb", nullable: false, defaultValue: "{}"),
                    last_modified = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    created_by = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    updated_by = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    row_version = table.Column<byte[]>(type: "bytea", rowVersion: true, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_repository_scripts", x => x.id);
                    table.ForeignKey(
                        name: "FK_repository_scripts_github_repositories_repository_id",
                        column: x => x.repository_id,
                        principalSchema: "powerorchestrator",
                        principalTable: "github_repositories",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_repository_scripts_scripts_script_id",
                        column: x => x.script_id,
                        principalSchema: "powerorchestrator",
                        principalTable: "scripts",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "sync_history",
                schema: "powerorchestrator",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    repository_id = table.Column<Guid>(type: "uuid", nullable: false),
                    sync_type = table.Column<string>(type: "text", nullable: false),
                    status = table.Column<string>(type: "text", nullable: false),
                    scripts_processed = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    scripts_added = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    scripts_updated = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    scripts_removed = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    duration_ms = table.Column<long>(type: "bigint", nullable: false, defaultValue: 0L),
                    error_message = table.Column<string>(type: "text", nullable: true),
                    started_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    completed_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    created_by = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    updated_by = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    row_version = table.Column<byte[]>(type: "bytea", rowVersion: true, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_sync_history", x => x.id);
                    table.ForeignKey(
                        name: "FK_sync_history_github_repositories_repository_id",
                        column: x => x.repository_id,
                        principalSchema: "powerorchestrator",
                        principalTable: "github_repositories",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "idx_github_repositories_created_at",
                schema: "powerorchestrator",
                table: "github_repositories",
                column: "created_at");

            migrationBuilder.CreateIndex(
                name: "idx_github_repositories_last_sync",
                schema: "powerorchestrator",
                table: "github_repositories",
                column: "last_sync_at");

            migrationBuilder.CreateIndex(
                name: "idx_github_repositories_owner",
                schema: "powerorchestrator",
                table: "github_repositories",
                column: "owner");

            migrationBuilder.CreateIndex(
                name: "idx_github_repositories_status",
                schema: "powerorchestrator",
                table: "github_repositories",
                column: "status");

            migrationBuilder.CreateIndex(
                name: "uk_github_repositories_full_name",
                schema: "powerorchestrator",
                table: "github_repositories",
                column: "full_name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "idx_repository_scripts_branch",
                schema: "powerorchestrator",
                table: "repository_scripts",
                column: "branch");

            migrationBuilder.CreateIndex(
                name: "idx_repository_scripts_modified",
                schema: "powerorchestrator",
                table: "repository_scripts",
                column: "last_modified");

            migrationBuilder.CreateIndex(
                name: "idx_repository_scripts_repository",
                schema: "powerorchestrator",
                table: "repository_scripts",
                column: "repository_id");

            migrationBuilder.CreateIndex(
                name: "idx_repository_scripts_script",
                schema: "powerorchestrator",
                table: "repository_scripts",
                column: "script_id");

            migrationBuilder.CreateIndex(
                name: "idx_repository_scripts_sha",
                schema: "powerorchestrator",
                table: "repository_scripts",
                column: "sha");

            migrationBuilder.CreateIndex(
                name: "uk_repository_scripts_path",
                schema: "powerorchestrator",
                table: "repository_scripts",
                columns: new[] { "repository_id", "file_path", "branch" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "idx_sync_history_repo_started",
                schema: "powerorchestrator",
                table: "sync_history",
                columns: new[] { "repository_id", "started_at" });

            migrationBuilder.CreateIndex(
                name: "idx_sync_history_repository",
                schema: "powerorchestrator",
                table: "sync_history",
                column: "repository_id");

            migrationBuilder.CreateIndex(
                name: "idx_sync_history_started",
                schema: "powerorchestrator",
                table: "sync_history",
                column: "started_at");

            migrationBuilder.CreateIndex(
                name: "idx_sync_history_status",
                schema: "powerorchestrator",
                table: "sync_history",
                column: "status");

            migrationBuilder.CreateIndex(
                name: "idx_sync_history_type",
                schema: "powerorchestrator",
                table: "sync_history",
                column: "sync_type");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "repository_scripts",
                schema: "powerorchestrator");

            migrationBuilder.DropTable(
                name: "sync_history",
                schema: "powerorchestrator");

            migrationBuilder.DropTable(
                name: "github_repositories",
                schema: "powerorchestrator");
        }
    }
}
