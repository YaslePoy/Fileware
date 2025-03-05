using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FilewareApi.Migrations
{
    /// <inheritdoc />
    public partial class previewdata : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<byte[]>(
                name: "Preview",
                table: "FileData",
                type: "BLOB",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Preview",
                table: "FileData");
        }
    }
}
