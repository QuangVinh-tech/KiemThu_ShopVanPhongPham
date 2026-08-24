using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ShopVanPhongPham.Migrations
{
    /// <inheritdoc />
    public partial class UpdateImageUrl : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "Description", "ImageUrl" },
                values: new object[] { "", "https://placehold.co/300x300?text=But+Bi" });

            migrationBuilder.UpdateData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "Description", "ImageUrl" },
                values: new object[] { "", "https://placehold.co/300x300?text=But+Chi" });

            migrationBuilder.UpdateData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 3,
                columns: new[] { "Description", "ImageUrl" },
                values: new object[] { "", "https://placehold.co/300x300?text=So+Tay" });

            migrationBuilder.UpdateData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 4,
                columns: new[] { "Description", "ImageUrl" },
                values: new object[] { "", "https://placehold.co/300x300?text=Tap+200" });

            migrationBuilder.UpdateData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 5,
                columns: new[] { "Description", "ImageUrl" },
                values: new object[] { "", "https://placehold.co/300x300?text=Thuoc+Ke" });

            migrationBuilder.UpdateData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 6,
                columns: new[] { "Description", "ImageUrl" },
                values: new object[] { "", "https://placehold.co/300x300?text=Keo" });

            migrationBuilder.UpdateData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 7,
                columns: new[] { "Description", "ImageUrl" },
                values: new object[] { "", "https://placehold.co/300x300?text=Bam+Kim" });

            migrationBuilder.UpdateData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 8,
                columns: new[] { "Description", "ImageUrl" },
                values: new object[] { "", "https://placehold.co/300x300?text=Giay+A4" });

            migrationBuilder.UpdateData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 9,
                columns: new[] { "Description", "ImageUrl" },
                values: new object[] { "", "https://placehold.co/300x300?text=Bang+Keo" });

            migrationBuilder.UpdateData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 10,
                columns: new[] { "Description", "ImageUrl" },
                values: new object[] { "", "https://placehold.co/300x300?text=Hop+But" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "Description", "ImageUrl" },
                values: new object[] { "Bút bi xanh cao cấp", "/assets/images/p1.jpg" });

            migrationBuilder.UpdateData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "Description", "ImageUrl" },
                values: new object[] { "Bút chì vẽ kỹ thuật", "/assets/images/p2.jpg" });

            migrationBuilder.UpdateData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 3,
                columns: new[] { "Description", "ImageUrl" },
                values: new object[] { "Sổ tay bìa cứng 200 trang", "/assets/images/p3.jpg" });

            migrationBuilder.UpdateData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 4,
                columns: new[] { "Description", "ImageUrl" },
                values: new object[] { "Tập học sinh kẻ ngang", "/assets/images/p4.jpg" });

            migrationBuilder.UpdateData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 5,
                columns: new[] { "Description", "ImageUrl" },
                values: new object[] { "Thước nhựa trong suốt", "/assets/images/p5.jpg" });

            migrationBuilder.UpdateData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 6,
                columns: new[] { "Description", "ImageUrl" },
                values: new object[] { "Kéo cắt giấy inox", "/assets/images/p6.jpg" });

            migrationBuilder.UpdateData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 7,
                columns: new[] { "Description", "ImageUrl" },
                values: new object[] { "Bấm kim 24/6 tiêu chuẩn", "/assets/images/p7.jpg" });

            migrationBuilder.UpdateData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 8,
                columns: new[] { "Description", "ImageUrl" },
                values: new object[] { "Giấy in laser trắng sáng", "/assets/images/p8.jpg" });

            migrationBuilder.UpdateData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 9,
                columns: new[] { "Description", "ImageUrl" },
                values: new object[] { "Băng keo văn phòng 2cm", "/assets/images/p9.jpg" });

            migrationBuilder.UpdateData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 10,
                columns: new[] { "Description", "ImageUrl" },
                values: new object[] { "Hộp đựng bút nhựa ABS", "/assets/images/p10.jpg" });
        }
    }
}
