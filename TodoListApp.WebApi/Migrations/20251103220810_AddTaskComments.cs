using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TodoListApp.WebApi.Migrations
{
    internal partial class AddTaskComments : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            _ = migrationBuilder.CreateTable(
                name: "TaskComments",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TaskId = table.Column<int>(type: "int", nullable: false),
                    Text = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: false),
                    CreatedByUserId = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                },
                constraints: table =>
                {
                    _ = table.PrimaryKey("PK_TaskComments", x => x.Id);
                    _ = table.ForeignKey(
                            name: "FK_TaskComments_Tasks_TaskId",
                            column: x => x.TaskId,
                            principalTable: "Tasks",
                            principalColumn: "Id",
                            onDelete: ReferentialAction.Cascade);
                });

            _ = migrationBuilder.CreateIndex(
                    name: "IX_TaskComments_TaskId",
                    table: "TaskComments",
                    column: "TaskId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            _ = migrationBuilder.DropTable(
                    name: "TaskComments");
        }
    }
}
