using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ShipMvp.Application.Migrations
{
    /// <inheritdoc />
    public partial class UpdateIntegrationsCredential : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AccessToken",
                table: "IntegrationCredentials");

            migrationBuilder.DropColumn(
                name: "AccessTokenExpiresAt",
                table: "IntegrationCredentials");

            migrationBuilder.DropColumn(
                name: "ExpiresAt",
                table: "IntegrationCredentials");

            migrationBuilder.DropColumn(
                name: "RefreshToken",
                table: "IntegrationCredentials");

            migrationBuilder.DropColumn(
                name: "Scope",
                table: "IntegrationCredentials");

            migrationBuilder.DropColumn(
                name: "TokenType",
                table: "IntegrationCredentials");

            migrationBuilder.CreateTable(
                name: "CredentialFields",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    IntegrationCredentialId = table.Column<Guid>(type: "uuid", nullable: false),
                    Key = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Value = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: false),
                    IsEncrypted = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    UpdatedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    DeletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DeletedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    EntityVersion = table.Column<int>(type: "integer", nullable: false),
                    ExtraProperties = table.Column<Dictionary<string, object>>(type: "jsonb", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CredentialFields", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CredentialFields_IntegrationCredentials_IntegrationCredenti~",
                        column: x => x.IntegrationCredentialId,
                        principalTable: "IntegrationCredentials",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_CredentialFields_CreatedAt",
                table: "CredentialFields",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_CredentialFields_IntegrationCredentialId",
                table: "CredentialFields",
                column: "IntegrationCredentialId");

            migrationBuilder.CreateIndex(
                name: "IX_CredentialFields_Key",
                table: "CredentialFields",
                column: "Key");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CredentialFields");

            migrationBuilder.AddColumn<string>(
                name: "AccessToken",
                table: "IntegrationCredentials",
                type: "character varying(2000)",
                maxLength: 2000,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "AccessTokenExpiresAt",
                table: "IntegrationCredentials",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "ExpiresAt",
                table: "IntegrationCredentials",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "RefreshToken",
                table: "IntegrationCredentials",
                type: "character varying(2000)",
                maxLength: 2000,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Scope",
                table: "IntegrationCredentials",
                type: "character varying(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "TokenType",
                table: "IntegrationCredentials",
                type: "character varying(50)",
                maxLength: 50,
                nullable: true);
        }
    }
}
