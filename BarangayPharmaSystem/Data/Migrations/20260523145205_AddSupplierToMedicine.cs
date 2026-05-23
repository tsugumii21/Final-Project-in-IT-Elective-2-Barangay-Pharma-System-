using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BarangayPharmaSystem.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddSupplierToMedicine : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "SupplierId",
                table: "Medicines",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Medicines_SupplierId",
                table: "Medicines",
                column: "SupplierId");

            migrationBuilder.AddForeignKey(
                name: "FK_Medicines_Suppliers_SupplierId",
                table: "Medicines",
                column: "SupplierId",
                principalTable: "Suppliers",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Medicines_Suppliers_SupplierId",
                table: "Medicines");

            migrationBuilder.DropIndex(
                name: "IX_Medicines_SupplierId",
                table: "Medicines");

            migrationBuilder.DropColumn(
                name: "SupplierId",
                table: "Medicines");
        }
    }
}
