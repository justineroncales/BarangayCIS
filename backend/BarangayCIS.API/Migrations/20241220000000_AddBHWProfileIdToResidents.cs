using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BarangayCIS.API.Migrations
{
    /// <inheritdoc />
    public partial class AddBHWProfileIdToResidents : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "BHWProfileId",
                table: "Residents",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Residents_BHWProfileId",
                table: "Residents",
                column: "BHWProfileId");

            migrationBuilder.AddForeignKey(
                name: "FK_Residents_BHWProfiles_BHWProfileId",
                table: "Residents",
                column: "BHWProfileId",
                principalTable: "BHWProfiles",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Residents_BHWProfiles_BHWProfileId",
                table: "Residents");

            migrationBuilder.DropIndex(
                name: "IX_Residents_BHWProfileId",
                table: "Residents");

            migrationBuilder.DropColumn(
                name: "BHWProfileId",
                table: "Residents");
        }
    }
}

