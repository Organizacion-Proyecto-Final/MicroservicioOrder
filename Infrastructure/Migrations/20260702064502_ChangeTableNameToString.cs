using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class ChangeTableNameToString : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Facturas_TableNumber",
                table: "Facturas");

            migrationBuilder.DropColumn(
                name: "TableNumber",
                table: "Facturas");

            migrationBuilder.AddColumn<string>(
                name: "TableName",
                table: "Facturas",
                type: "nvarchar(450)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateIndex(
                name: "IX_Facturas_TableName",
                table: "Facturas",
                column: "TableName");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Facturas_TableName",
                table: "Facturas");

            migrationBuilder.DropColumn(
                name: "TableName",
                table: "Facturas");

            migrationBuilder.AddColumn<int>(
                name: "TableNumber",
                table: "Facturas",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_Facturas_TableNumber",
                table: "Facturas",
                column: "TableNumber");
        }
    }
}
