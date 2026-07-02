using EcommerceAPI.Data;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EcommerceAPI.Migrations
{
    /// <inheritdoc />
    [DbContext(typeof(EcommerceDbContext))]
    [Migration("20260702120000_AddProductDescription")]
    public partial class AddProductDescription : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
IF COL_LENGTH('product', 'Description') IS NULL
BEGIN
    ALTER TABLE [product] ADD [Description] nvarchar(1000) NULL;
END
");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
IF COL_LENGTH('product', 'Description') IS NOT NULL
BEGIN
    ALTER TABLE [product] DROP COLUMN [Description];
END
");
        }
    }
}
