using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SimpleService.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "SimpleAuthors",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PublicId = table.Column<Guid>(type: "uniqueidentifier", nullable: false, defaultValueSql: "NEWID()"),
                    Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    TenantId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    InstanceId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TraceId = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SimpleAuthors", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "SimpleBooks",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PublicId = table.Column<Guid>(type: "uniqueidentifier", nullable: false, defaultValueSql: "NEWID()"),
                    Title = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false),
                    ISBN = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    TraceId = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    TenantId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    InstanceId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    AuthorId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SimpleBooks", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SimpleBooks_SimpleAuthors_AuthorId",
                        column: x => x.AuthorId,
                        principalTable: "SimpleAuthors",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "SimpleChapters",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PublicId = table.Column<Guid>(type: "uniqueidentifier", nullable: false, defaultValueSql: "NEWID()"),
                    Title = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: false),
                    ChapterNumber = table.Column<int>(type: "int", nullable: false),
                    Text = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    TraceId = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    TenantId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    InstanceId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    BookId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SimpleChapters", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SimpleChapters_SimpleBooks_BookId",
                        column: x => x.BookId,
                        principalTable: "SimpleBooks",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_SimpleBooks_AuthorId",
                table: "SimpleBooks",
                column: "AuthorId");

            migrationBuilder.CreateIndex(
                name: "IX_SimpleChapters_BookId",
                table: "SimpleChapters",
                column: "BookId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "SimpleChapters");

            migrationBuilder.DropTable(
                name: "SimpleBooks");

            migrationBuilder.DropTable(
                name: "SimpleAuthors");
        }
    }
}
