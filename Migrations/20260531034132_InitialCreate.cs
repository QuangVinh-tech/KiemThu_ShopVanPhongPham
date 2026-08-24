using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace ShopVanPhongPham.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Products",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Price = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    ImageUrl = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Category = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Products", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ShoppingCartItems",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ProductId = table.Column<int>(type: "int", nullable: false),
                    Quantity = table.Column<int>(type: "int", nullable: false),
                    CartId = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ShoppingCartItems", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ShoppingCartItems_Products_ProductId",
                        column: x => x.ProductId,
                        principalTable: "Products",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "Products",
                columns: new[] { "Id", "Category", "Description", "ImageUrl", "Name", "Price" },
                values: new object[,]
                {
                    { 1, "But", "Bút bi xanh cao cấp", "/assets/images/p1.jpg", "Bút bi Thiên Long", 5000m },
                    { 2, "But", "Bút chì vẽ kỹ thuật", "/assets/images/p2.jpg", "Bút chì 2B", 4000m },
                    { 3, "So", "Sổ tay bìa cứng 200 trang", "/assets/images/p3.jpg", "Sổ tay A5", 35000m },
                    { 4, "So", "Tập học sinh kẻ ngang", "/assets/images/p4.jpg", "Tập 200 trang", 15000m },
                    { 5, "DungCu", "Thước nhựa trong suốt", "/assets/images/p5.jpg", "Thước kẻ 30cm", 8000m },
                    { 6, "DungCu", "Kéo cắt giấy inox", "/assets/images/p6.jpg", "Kéo văn phòng", 25000m },
                    { 7, "DungCu", "Bấm kim 24/6 tiêu chuẩn", "/assets/images/p7.jpg", "Bấm kim", 30000m },
                    { 8, "Giay", "Giấy in laser trắng sáng", "/assets/images/p8.jpg", "Giấy A4 500 tờ", 85000m },
                    { 9, "DungCu", "Băng keo văn phòng 2cm", "/assets/images/p9.jpg", "Băng keo trong", 10000m },
                    { 10, "DungCu", "Hộp đựng bút nhựa ABS", "/assets/images/p10.jpg", "Hộp bút để bàn", 45000m }
                });

            migrationBuilder.CreateIndex(
                name: "IX_ShoppingCartItems_ProductId",
                table: "ShoppingCartItems",
                column: "ProductId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ShoppingCartItems");

            migrationBuilder.DropTable(
                name: "Products");
        }
    }
}
