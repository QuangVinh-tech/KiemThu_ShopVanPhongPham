using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ShopVanPhongPham.Migrations
{
    /// <inheritdoc />
    public partial class FixMissingStatusColumn : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
                IF NOT EXISTS (
                    SELECT 1 FROM sys.columns 
                    WHERE Name = N'Status' AND Object_ID = Object_ID(N'Orders')
                )
                BEGIN
                    ALTER TABLE [Orders] ADD [Status] nvarchar(max) NULL;
                END
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
        }
    }
}