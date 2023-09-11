using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CYShop.Migrations
{
    public partial class FixRelationShipsOfClass : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ProductSalesCount_Product_ProductID",
                table: "ProductSalesCount");

            migrationBuilder.AlterColumn<long>(
                name: "ProductID",
                table: "ProductSalesCount",
                type: "bigint",
                nullable: false,
                defaultValue: 0L,
                oldClrType: typeof(long),
                oldType: "bigint",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_ProductSalesCount_Product_ProductID",
                table: "ProductSalesCount",
                column: "ProductID",
                principalTable: "Product",
                principalColumn: "ID",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ProductSalesCount_Product_ProductID",
                table: "ProductSalesCount");

            migrationBuilder.AlterColumn<long>(
                name: "ProductID",
                table: "ProductSalesCount",
                type: "bigint",
                nullable: true,
                oldClrType: typeof(long),
                oldType: "bigint");

            migrationBuilder.AddForeignKey(
                name: "FK_ProductSalesCount_Product_ProductID",
                table: "ProductSalesCount",
                column: "ProductID",
                principalTable: "Product",
                principalColumn: "ID");
        }
    }
}
