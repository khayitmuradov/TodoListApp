using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TodoListApp.WebApi.Migrations
{
    public partial class AddSearchIndexes : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_Tasks_CreatedDate",
                table: "Tasks",
                column: "CreatedDate");

            migrationBuilder.CreateIndex(
                name: "IX_Tasks_DueDate",
                table: "Tasks",
                column: "DueDate");

            migrationBuilder.CreateIndex(
                name: "IX_Tasks_Title",
                table: "Tasks",
                column: "Title");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Tasks_CreatedDate",
                table: "Tasks");

            migrationBuilder.DropIndex(
                name: "IX_Tasks_DueDate",
                table: "Tasks");

            migrationBuilder.DropIndex(
                name: "IX_Tasks_Title",
                table: "Tasks");
        }
    }
}
