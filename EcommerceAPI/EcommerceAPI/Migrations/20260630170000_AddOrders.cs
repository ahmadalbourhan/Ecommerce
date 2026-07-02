using EcommerceAPI.Data;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EcommerceAPI.Migrations
{
    [DbContext(typeof(EcommerceDbContext))]
    [Migration("20260630170000_AddOrders")]
    public partial class AddOrders : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
IF OBJECT_ID(N'[order]', N'U') IS NULL
BEGIN
    CREATE TABLE [order] (
        [Id] int NOT NULL IDENTITY,
        [OrderNumber] nvarchar(50) NOT NULL,
        [UserId] int NOT NULL,
        [Status] nvarchar(50) NOT NULL CONSTRAINT [DF_order_Status] DEFAULT N'Pending',
        [Total] decimal(18,2) NOT NULL,
        [OrderedAt] datetime2 NOT NULL CONSTRAINT [DF_order_OrderedAt] DEFAULT GETUTCDATE(),
        [AcceptedAt] datetime2 NULL,
        [DeliveredAt] datetime2 NULL,
        [PaymentMethod] nvarchar(50) NOT NULL CONSTRAINT [DF_order_PaymentMethod] DEFAULT N'CashOnDelivery',
        CONSTRAINT [PK_order] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_order_user_UserId] FOREIGN KEY ([UserId]) REFERENCES [user] ([Id]) ON DELETE NO ACTION
    );

    CREATE UNIQUE INDEX [IX_order_OrderNumber] ON [order] ([OrderNumber]);
    CREATE INDEX [IX_order_UserId] ON [order] ([UserId]);
    CREATE INDEX [IX_order_Status] ON [order] ([Status]);
END

IF OBJECT_ID(N'[order_item]', N'U') IS NULL
BEGIN
    CREATE TABLE [order_item] (
        [Id] int NOT NULL IDENTITY,
        [OrderId] int NOT NULL,
        [ProductId] int NOT NULL,
        [Quantity] int NOT NULL,
        [UnitPrice] decimal(18,2) NOT NULL,
        [TotalPrice] decimal(18,2) NOT NULL,
        [CreatedAt] datetime2 NOT NULL CONSTRAINT [DF_order_item_CreatedAt] DEFAULT GETUTCDATE(),
        [UpdatedAt] datetime2 NOT NULL CONSTRAINT [DF_order_item_UpdatedAt] DEFAULT GETUTCDATE(),
        CONSTRAINT [PK_order_item] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_order_item_order_OrderId] FOREIGN KEY ([OrderId]) REFERENCES [order] ([Id]) ON DELETE CASCADE,
        CONSTRAINT [FK_order_item_product_ProductId] FOREIGN KEY ([ProductId]) REFERENCES [product] ([Id]) ON DELETE NO ACTION
    );

    CREATE INDEX [IX_order_item_OrderId] ON [order_item] ([OrderId]);
    CREATE INDEX [IX_order_item_ProductId] ON [order_item] ([ProductId]);
END
");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
IF OBJECT_ID(N'[order_item]', N'U') IS NOT NULL
BEGIN
    DROP TABLE [order_item];
END

IF OBJECT_ID(N'[order]', N'U') IS NOT NULL
BEGIN
    DROP TABLE [order];
END
");
        }
    }
}
