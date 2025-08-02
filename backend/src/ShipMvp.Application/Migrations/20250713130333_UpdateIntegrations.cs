using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ShipMvp.Application.Migrations
{
    /// <inheritdoc />
    public partial class UpdateIntegrations : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "UserInfo",
                table: "Integrations");

            migrationBuilder.RenameColumn(
                name: "IdentityUserId",
                table: "IntegrationCredentials",
                newName: "UserId");

            migrationBuilder.AddColumn<string>(
                name: "UserInfo",
                table: "IntegrationCredentials",
                type: "character varying(500)",
                maxLength: 500,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddForeignKey(
                name: "FK_IntegrationCredentials_Integrations_IntegrationId",
                table: "IntegrationCredentials",
                column: "IntegrationId",
                principalTable: "Integrations",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_IntegrationCredentials_Integrations_IntegrationId",
                table: "IntegrationCredentials");

            migrationBuilder.DropColumn(
                name: "UserInfo",
                table: "IntegrationCredentials");

            migrationBuilder.RenameColumn(
                name: "UserId",
                table: "IntegrationCredentials",
                newName: "IdentityUserId");

            migrationBuilder.AddColumn<string>(
                name: "UserInfo",
                table: "Integrations",
                type: "character varying(500)",
                maxLength: 500,
                nullable: true);
        }
    }
}
