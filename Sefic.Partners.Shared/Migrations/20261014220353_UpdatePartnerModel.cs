using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Sefic.Partners.Shared.Migrations
{
    /// <inheritdoc />
    public partial class UpdatePartnerModel : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_Partners_CreatedAt",
                table: "Partners",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_Partners_IsActive_CreatedAt",
                table: "Partners",
                columns: new[] { "IsActive", "CreatedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_Partners_Name",
                table: "Partners",
                column: "Name");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Partners_CreatedAt",
                table: "Partners");

            migrationBuilder.DropIndex(
                name: "IX_Partners_IsActive_CreatedAt",
                table: "Partners");

            migrationBuilder.DropIndex(
                name: "IX_Partners_Name",
                table: "Partners");
        }
    }
}
