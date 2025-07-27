using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PowerOrchestrator.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "powerorchestrator");

            migrationBuilder.CreateTable(
                name: "audit_logs",
                schema: "powerorchestrator",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    action = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    entity_type = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    entity_id = table.Column<Guid>(type: "uuid", nullable: true),
                    user_id = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    user_name = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    ip_address = table.Column<string>(type: "character varying(45)", maxLength: 45, nullable: true),
                    user_agent = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    details = table.Column<string>(type: "jsonb", nullable: true),
                    old_values = table.Column<string>(type: "jsonb", nullable: true),
                    new_values = table.Column<string>(type: "jsonb", nullable: true),
                    success = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    error_message = table.Column<string>(type: "text", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    created_by = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    updated_by = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    row_version = table.Column<byte[]>(type: "bytea", rowVersion: true, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_audit_logs", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "health_checks",
                schema: "powerorchestrator",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    service_name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    response_time_ms = table.Column<long>(type: "bigint", nullable: true),
                    details = table.Column<string>(type: "jsonb", nullable: true),
                    error_message = table.Column<string>(type: "text", nullable: true),
                    last_checked_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    endpoint = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    timeout_seconds = table.Column<int>(type: "integer", nullable: false, defaultValue: 30),
                    is_enabled = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    interval_minutes = table.Column<int>(type: "integer", nullable: false, defaultValue: 5),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    created_by = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    updated_by = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    row_version = table.Column<byte[]>(type: "bytea", rowVersion: true, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_health_checks", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "scripts",
                schema: "powerorchestrator",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    name = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    description = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    content = table.Column<string>(type: "text", nullable: false),
                    version = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    tags = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    is_active = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    timeout_seconds = table.Column<int>(type: "integer", nullable: false, defaultValue: 300),
                    required_powershell_version = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false, defaultValue: "5.1"),
                    parameters_schema = table.Column<string>(type: "jsonb", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    created_by = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    updated_by = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    row_version = table.Column<byte[]>(type: "bytea", rowVersion: true, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_scripts", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "executions",
                schema: "powerorchestrator",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    script_id = table.Column<Guid>(type: "uuid", nullable: false),
                    status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    started_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    completed_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    duration_ms = table.Column<long>(type: "bigint", nullable: true),
                    parameters = table.Column<string>(type: "jsonb", nullable: true),
                    output = table.Column<string>(type: "text", nullable: true),
                    error_output = table.Column<string>(type: "text", nullable: true),
                    exit_code = table.Column<int>(type: "integer", nullable: true),
                    executed_on = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    powershell_version = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    metadata = table.Column<string>(type: "jsonb", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    created_by = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    updated_by = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    row_version = table.Column<byte[]>(type: "bytea", rowVersion: true, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_executions", x => x.id);
                    table.ForeignKey(
                        name: "FK_executions_scripts_script_id",
                        column: x => x.script_id,
                        principalSchema: "powerorchestrator",
                        principalTable: "scripts",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "idx_audit_logs_action",
                schema: "powerorchestrator",
                table: "audit_logs",
                column: "action");

            migrationBuilder.CreateIndex(
                name: "idx_audit_logs_created_at",
                schema: "powerorchestrator",
                table: "audit_logs",
                column: "created_at");

            migrationBuilder.CreateIndex(
                name: "idx_audit_logs_entity_created",
                schema: "powerorchestrator",
                table: "audit_logs",
                columns: new[] { "entity_type", "entity_id", "created_at" });

            migrationBuilder.CreateIndex(
                name: "idx_audit_logs_entity_id",
                schema: "powerorchestrator",
                table: "audit_logs",
                column: "entity_id");

            migrationBuilder.CreateIndex(
                name: "idx_audit_logs_entity_type",
                schema: "powerorchestrator",
                table: "audit_logs",
                column: "entity_type");

            migrationBuilder.CreateIndex(
                name: "idx_audit_logs_user_id",
                schema: "powerorchestrator",
                table: "audit_logs",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "idx_executions_created_at",
                schema: "powerorchestrator",
                table: "executions",
                column: "created_at");

            migrationBuilder.CreateIndex(
                name: "idx_executions_script_id",
                schema: "powerorchestrator",
                table: "executions",
                column: "script_id");

            migrationBuilder.CreateIndex(
                name: "idx_executions_started_at",
                schema: "powerorchestrator",
                table: "executions",
                column: "started_at");

            migrationBuilder.CreateIndex(
                name: "idx_executions_status",
                schema: "powerorchestrator",
                table: "executions",
                column: "status");

            migrationBuilder.CreateIndex(
                name: "idx_health_checks_is_enabled",
                schema: "powerorchestrator",
                table: "health_checks",
                column: "is_enabled");

            migrationBuilder.CreateIndex(
                name: "idx_health_checks_last_checked",
                schema: "powerorchestrator",
                table: "health_checks",
                column: "last_checked_at");

            migrationBuilder.CreateIndex(
                name: "idx_health_checks_service_name",
                schema: "powerorchestrator",
                table: "health_checks",
                column: "service_name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "idx_health_checks_status",
                schema: "powerorchestrator",
                table: "health_checks",
                column: "status");

            migrationBuilder.CreateIndex(
                name: "idx_scripts_created_at",
                schema: "powerorchestrator",
                table: "scripts",
                column: "created_at");

            migrationBuilder.CreateIndex(
                name: "idx_scripts_is_active",
                schema: "powerorchestrator",
                table: "scripts",
                column: "is_active");

            migrationBuilder.CreateIndex(
                name: "idx_scripts_name",
                schema: "powerorchestrator",
                table: "scripts",
                column: "name");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "audit_logs",
                schema: "powerorchestrator");

            migrationBuilder.DropTable(
                name: "executions",
                schema: "powerorchestrator");

            migrationBuilder.DropTable(
                name: "health_checks",
                schema: "powerorchestrator");

            migrationBuilder.DropTable(
                name: "scripts",
                schema: "powerorchestrator");
        }
    }
}
