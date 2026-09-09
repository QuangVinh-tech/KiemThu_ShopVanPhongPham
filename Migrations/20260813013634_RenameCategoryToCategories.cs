using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ShopVanPhongPham.Migrations
{
    /// <inheritdoc />
    public partial class RenameCategoryToCategories : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // 0. Tạo bảng Category nếu chưa tồn tại (bù cho việc trước đây tạo thủ công, không qua migration)
            migrationBuilder.Sql(@"
                IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'Category')
                BEGIN
                    CREATE TABLE [Category] (
                        [Id] int NOT NULL IDENTITY(1,1),
                        [Name] nvarchar(max) NOT NULL,
                        CONSTRAINT [PK_Category] PRIMARY KEY ([Id])
                    );
                END
            ");

            // 0.1. Thêm cột CategoryId cho Products nếu chưa có (vì migration tạo cột này cũng bị thiếu)
            migrationBuilder.Sql(@"
                IF NOT EXISTS (
                    SELECT 1 FROM sys.columns
                    WHERE object_id = OBJECT_ID('Products') AND name = 'CategoryId'
                )
                BEGIN
                    ALTER TABLE [Products] ADD [CategoryId] int NULL;
                END
            ");

            // 1. Xóa Foreign Key cũ nếu tồn tại
            migrationBuilder.Sql(@"
                IF EXISTS (
                    SELECT 1 FROM sys.foreign_keys 
                    WHERE name = 'FK_Products_Category_CategoryId'
                )
                BEGIN
                    ALTER TABLE [Products] DROP CONSTRAINT [FK_Products_Category_CategoryId];
                END
            ");

            // 2. Xóa Primary Key cũ nếu tồn tại
            migrationBuilder.Sql(@"
                IF EXISTS (
                    SELECT 1 FROM sys.key_constraints 
                    WHERE name = 'PK_Category' AND type = 'PK'
                )
                BEGIN
                    ALTER TABLE [Category] DROP CONSTRAINT [PK_Category];
                END
            ");

            // 3. Đổi tên bảng
            migrationBuilder.RenameTable(
                name: "Category",
                newName: "Categories");

            // 4. Tạo Primary Key mới nếu chưa tồn tại
            migrationBuilder.Sql(@"
                IF NOT EXISTS (
                    SELECT 1 FROM sys.key_constraints 
                    WHERE name = 'PK_Categories' AND type = 'PK'
                )
                BEGIN
                    ALTER TABLE [Categories] ADD CONSTRAINT [PK_Categories] PRIMARY KEY ([Id]);
                END
            ");

            // 5. Tạo Foreign Key mới nếu chưa tồn tại
            migrationBuilder.Sql(@"
                IF NOT EXISTS (
                    SELECT 1 FROM sys.foreign_keys 
                    WHERE name = 'FK_Products_Categories_CategoryId'
                )
                BEGIN
                    ALTER TABLE [Products] ADD CONSTRAINT [FK_Products_Categories_CategoryId] 
                    FOREIGN KEY ([CategoryId]) REFERENCES [Categories] ([Id]) ON DELETE CASCADE;
                END
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // 1. Xóa Foreign Key mới nếu tồn tại
            migrationBuilder.Sql(@"
                IF EXISTS (
                    SELECT 1 FROM sys.foreign_keys 
                    WHERE name = 'FK_Products_Categories_CategoryId'
                )
                BEGIN
                    ALTER TABLE [Products] DROP CONSTRAINT [FK_Products_Categories_CategoryId];
                END
            ");

            // 2. Xóa Primary Key mới nếu tồn tại
            migrationBuilder.Sql(@"
                IF EXISTS (
                    SELECT 1 FROM sys.key_constraints 
                    WHERE name = 'PK_Categories' AND type = 'PK'
                )
                BEGIN
                    ALTER TABLE [Categories] DROP CONSTRAINT [PK_Categories];
                END
            ");

            // 3. Đổi lại tên bảng cũ
            migrationBuilder.RenameTable(
                name: "Categories",
                newName: "Category");

            // 4. Khôi phục Primary Key cũ nếu chưa tồn tại
            migrationBuilder.Sql(@"
                IF NOT EXISTS (
                    SELECT 1 FROM sys.key_constraints 
                    WHERE name = 'PK_Category' AND type = 'PK'
                )
                BEGIN
                    ALTER TABLE [Category] ADD CONSTRAINT [PK_Category] PRIMARY KEY ([Id]);
                END
            ");

            // 5. Khôi phục Foreign Key cũ nếu chưa tồn tại
            migrationBuilder.Sql(@"
                IF NOT EXISTS (
                    SELECT 1 FROM sys.foreign_keys 
                    WHERE name = 'FK_Products_Category_CategoryId'
                )
                BEGIN
                    ALTER TABLE [Products] ADD CONSTRAINT [FK_Products_Category_CategoryId] 
                    FOREIGN KEY ([CategoryId]) REFERENCES [Category] ([Id]) ON DELETE CASCADE;
                END
            ");
        }
    }
}