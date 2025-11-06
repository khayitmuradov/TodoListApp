using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TodoListApp.WebApi.Migrations
{
    internal partial class AddTags : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            _ = migrationBuilder.CreateTable(
                name: "Tags",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    ColorHex = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                },
                constraints: table =>
                {
                    _ = table.PrimaryKey("PK_Tags", x => x.Id);
                });

            _ = migrationBuilder.CreateTable(
                name: "TaskTags",
                columns: table => new
                {
                    TaskId = table.Column<int>(type: "int", nullable: false),
                    TagId = table.Column<int>(type: "int", nullable: false),
                    LinkedUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                },
                constraints: table =>
                {
                    _ = table.PrimaryKey("PK_TaskTags", x => new { x.TaskId, x.TagId });
                    _ = table.ForeignKey(
                            name: "FK_TaskTags_Tags_TagId",
                            column: x => x.TagId,
                            principalTable: "Tags",
                            principalColumn: "Id",
                            onDelete: ReferentialAction.Cascade);
                    _ = table.ForeignKey(
                            name: "FK_TaskTags_Tasks_TaskId",
                            column: x => x.TaskId,
                            principalTable: "Tasks",
                            principalColumn: "Id",
                            onDelete: ReferentialAction.Cascade);
                });

            _ = migrationBuilder.CreateIndex(
                    name: "IX_Tags_Name",
                    table: "Tags",
                    column: "Name");

            _ = migrationBuilder.CreateIndex(
                    name: "IX_TaskTags_TagId",
                    table: "TaskTags",
                    column: "TagId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            _ = migrationBuilder.DropTable(
                    name: "TaskTags");

            _ = migrationBuilder.DropTable(
                    name: "Tags");
        }
    }
}
