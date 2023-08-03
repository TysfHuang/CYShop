using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CYShop.Migrations
{
    public partial class ChangeProductOrder : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "ProductInfo",
                table: "ProductOrder",
                newName: "ReceiverPhone");

            migrationBuilder.RenameColumn(
                name: "DeliveryAddress",
                table: "ProductOrder",
                newName: "ReceiverName");

            migrationBuilder.AddColumn<string>(
                name: "ReceiverAddress",
                table: "ProductOrder",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "TotalPrice",
                table: "ProductOrder",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "OrderItem",
                columns: table => new
                {
                    ID = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ProductOrderID = table.Column<long>(type: "bigint", nullable: false),
                    ProductID = table.Column<long>(type: "bigint", nullable: false),
                    Quantity = table.Column<long>(type: "bigint", nullable: false),
                    Price = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OrderItem", x => x.ID);
                    table.ForeignKey(
                        name: "FK_OrderItem_Product_ProductID",
                        column: x => x.ProductID,
                        principalTable: "Product",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_OrderItem_ProductOrder_ProductOrderID",
                        column: x => x.ProductOrderID,
                        principalTable: "ProductOrder",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_OrderItem_ProductID",
                table: "OrderItem",
                column: "ProductID");

            migrationBuilder.CreateIndex(
                name: "IX_OrderItem_ProductOrderID",
                table: "OrderItem",
                column: "ProductOrderID");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "OrderItem");

            migrationBuilder.DropColumn(
                name: "ReceiverAddress",
                table: "ProductOrder");

            migrationBuilder.DropColumn(
                name: "TotalPrice",
                table: "ProductOrder");

            migrationBuilder.RenameColumn(
                name: "ReceiverPhone",
                table: "ProductOrder",
                newName: "ProductInfo");

            migrationBuilder.RenameColumn(
                name: "ReceiverName",
                table: "ProductOrder",
                newName: "DeliveryAddress");
        }
    }
}
