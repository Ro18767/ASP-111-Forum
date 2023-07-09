using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ASP_111.Migrations
{
    /// <inheritdoc />
    public partial class changedschematoasp111forum : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "asp111forum");

            migrationBuilder.RenameTable(
                name: "Visits",
                schema: "asp111",
                newName: "Visits",
                newSchema: "asp111forum");

            migrationBuilder.RenameTable(
                name: "Users",
                schema: "asp111",
                newName: "Users",
                newSchema: "asp111forum");

            migrationBuilder.RenameTable(
                name: "Topics",
                schema: "asp111",
                newName: "Topics",
                newSchema: "asp111forum");

            migrationBuilder.RenameTable(
                name: "Themes",
                schema: "asp111",
                newName: "Themes",
                newSchema: "asp111forum");

            migrationBuilder.RenameTable(
                name: "Sections",
                schema: "asp111",
                newName: "Sections",
                newSchema: "asp111forum");

            migrationBuilder.RenameTable(
                name: "Rates",
                schema: "asp111",
                newName: "Rates",
                newSchema: "asp111forum");

            migrationBuilder.RenameTable(
                name: "Comments",
                schema: "asp111",
                newName: "Comments",
                newSchema: "asp111forum");

            migrationBuilder.CreateIndex(
                name: "IX_Sections_AuthorId",
                schema: "asp111forum",
                table: "Sections",
                column: "AuthorId");

            migrationBuilder.AddForeignKey(
                name: "FK_Sections_Users_AuthorId",
                schema: "asp111forum",
                table: "Sections",
                column: "AuthorId",
                principalSchema: "asp111forum",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Sections_Users_AuthorId",
                schema: "asp111forum",
                table: "Sections");

            migrationBuilder.DropIndex(
                name: "IX_Sections_AuthorId",
                schema: "asp111forum",
                table: "Sections");

            migrationBuilder.EnsureSchema(
                name: "asp111");

            migrationBuilder.RenameTable(
                name: "Visits",
                schema: "asp111forum",
                newName: "Visits",
                newSchema: "asp111");

            migrationBuilder.RenameTable(
                name: "Users",
                schema: "asp111forum",
                newName: "Users",
                newSchema: "asp111");

            migrationBuilder.RenameTable(
                name: "Topics",
                schema: "asp111forum",
                newName: "Topics",
                newSchema: "asp111");

            migrationBuilder.RenameTable(
                name: "Themes",
                schema: "asp111forum",
                newName: "Themes",
                newSchema: "asp111");

            migrationBuilder.RenameTable(
                name: "Sections",
                schema: "asp111forum",
                newName: "Sections",
                newSchema: "asp111");

            migrationBuilder.RenameTable(
                name: "Rates",
                schema: "asp111forum",
                newName: "Rates",
                newSchema: "asp111");

            migrationBuilder.RenameTable(
                name: "Comments",
                schema: "asp111forum",
                newName: "Comments",
                newSchema: "asp111");
        }
    }
}
