using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TodoListApp.WebApi.Migrations
{
    internal partial class AddSearchIndexes : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            _ = migrationBuilder.CreateIndex(
                name: "IX_Tasks_CreatedDate",
                table: "Tasks",
                column: "CreatedDate");

            _ = migrationBuilder.CreateIndex(
                name: "IX_Tasks_DueDate",
                table: "Tasks",
                column: "DueDate");

            _ = migrationBuilder.CreateIndex(
                name: "IX_Tasks_Title",
                table: "Tasks",
                column: "Title");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            _ = migrationBuilder.DropIndex(
                name: "IX_Tasks_CreatedDate",
                table: "Tasks");

            _ = migrationBuilder.DropIndex(
                name: "IX_Tasks_DueDate",
                table: "Tasks");

            _ = migrationBuilder.DropIndex(
                name: "IX_Tasks_Title",
                table: "Tasks");
        }
    }
}
