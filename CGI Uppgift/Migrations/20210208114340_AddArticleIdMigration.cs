using Microsoft.EntityFrameworkCore.Migrations;

namespace CGI_Uppgift.Migrations
{
    public partial class AddArticleIdMigration : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Orderrow_Article_ArticleID",
                table: "Orderrow");

            migrationBuilder.AlterColumn<int>(
                name: "ArticleID",
                table: "Orderrow",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Orderrow_Article_ArticleID",
                table: "Orderrow",
                column: "ArticleID",
                principalTable: "Article",
                principalColumn: "ID",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Orderrow_Article_ArticleID",
                table: "Orderrow");

            migrationBuilder.AlterColumn<int>(
                name: "ArticleID",
                table: "Orderrow",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AddForeignKey(
                name: "FK_Orderrow_Article_ArticleID",
                table: "Orderrow",
                column: "ArticleID",
                principalTable: "Article",
                principalColumn: "ID",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
