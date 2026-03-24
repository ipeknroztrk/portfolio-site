using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PortfolioSite.Application.Migrations
{
    /// <inheritdoc />
    public partial class AddScoreTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "PlayerName",
                table: "Scores",
                type: "character varying(100)",
                maxLength: 100,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.CreateIndex(
                name: "IX_Scores_Points",
                table: "Scores",
                column: "Points");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Scores_Points",
                table: "Scores");

            migrationBuilder.AlterColumn<string>(
                name: "PlayerName",
                table: "Scores",
                type: "text",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(100)",
                oldMaxLength: 100);
        }
    }
}
