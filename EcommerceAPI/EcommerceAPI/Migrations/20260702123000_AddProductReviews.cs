using EcommerceAPI.Data;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EcommerceAPI.Migrations
{
    /// <inheritdoc />
    [DbContext(typeof(EcommerceDbContext))]
    [Migration("20260702123000_AddProductReviews")]
    public partial class AddProductReviews : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
IF OBJECT_ID('product_review', 'U') IS NULL
BEGIN
    CREATE TABLE [product_review] (
        [Id] int NOT NULL IDENTITY,
        [product_id] int NOT NULL,
        [user_id] int NOT NULL,
        [order_id] int NOT NULL,
        [Rating] int NOT NULL,
        [Comment] nvarchar(1000) NULL,
        [image_url] nvarchar(1000) NULL,
        [created_at] datetime2 NOT NULL CONSTRAINT [DF_product_review_created_at] DEFAULT GETUTCDATE(),
        CONSTRAINT [PK_product_review] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_product_review_product_product_id] FOREIGN KEY ([product_id]) REFERENCES [product] ([Id]) ON DELETE CASCADE,
        CONSTRAINT [FK_product_review_user_user_id] FOREIGN KEY ([user_id]) REFERENCES [user] ([Id]),
        CONSTRAINT [FK_product_review_order_order_id] FOREIGN KEY ([order_id]) REFERENCES [order] ([Id])
    );

    CREATE INDEX [IX_product_review_product_id] ON [product_review] ([product_id]);
    CREATE INDEX [IX_product_review_user_id] ON [product_review] ([user_id]);
    CREATE INDEX [IX_product_review_order_id] ON [product_review] ([order_id]);
    CREATE UNIQUE INDEX [IX_product_review_product_id_user_id_order_id] ON [product_review] ([product_id], [user_id], [order_id]);
END
");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
IF OBJECT_ID('product_review', 'U') IS NOT NULL
BEGIN
    DROP TABLE [product_review];
END
");
        }
    }
}
