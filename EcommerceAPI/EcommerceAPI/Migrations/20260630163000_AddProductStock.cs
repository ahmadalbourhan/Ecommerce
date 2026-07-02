using EcommerceAPI.Data;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EcommerceAPI.Migrations
{
    /// <inheritdoc />
    [DbContext(typeof(EcommerceDbContext))]
    [Migration("20260630163000_AddProductStock")]
    public partial class AddProductStock : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
IF COL_LENGTH('product', 'Stock') IS NULL
BEGIN
    ALTER TABLE [product] ADD [Stock] int NOT NULL CONSTRAINT [DF_product_Stock] DEFAULT 0;
END
");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
IF COL_LENGTH('product', 'Stock') IS NOT NULL
BEGIN
    DECLARE @constraintName nvarchar(200);
    SELECT @constraintName = dc.name
    FROM sys.default_constraints dc
    INNER JOIN sys.columns c ON c.default_object_id = dc.object_id
    INNER JOIN sys.tables t ON t.object_id = c.object_id
    WHERE t.name = 'product' AND c.name = 'Stock';

    IF @constraintName IS NOT NULL
    BEGIN
        EXEC('ALTER TABLE [product] DROP CONSTRAINT [' + @constraintName + ']');
    END

    ALTER TABLE [product] DROP COLUMN [Stock];
END
");
        }
    }
}
