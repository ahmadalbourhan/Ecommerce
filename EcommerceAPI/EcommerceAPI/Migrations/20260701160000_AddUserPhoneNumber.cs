using EcommerceAPI.Data;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EcommerceAPI.Migrations
{
    [DbContext(typeof(EcommerceDbContext))]
    [Migration("20260701160000_AddUserPhoneNumber")]
    public partial class AddUserPhoneNumber : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
IF COL_LENGTH(N'[user]', N'PhoneNumber') IS NULL
BEGIN
    ALTER TABLE [user]
    ADD [PhoneNumber] nvarchar(50) NOT NULL CONSTRAINT [DF_user_PhoneNumber] DEFAULT N'';
END
");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
IF COL_LENGTH(N'[user]', N'PhoneNumber') IS NOT NULL
BEGIN
    DECLARE @constraintName nvarchar(128);

    SELECT @constraintName = dc.name
    FROM sys.default_constraints dc
    INNER JOIN sys.columns c ON c.default_object_id = dc.object_id
    INNER JOIN sys.tables t ON t.object_id = c.object_id
    WHERE t.name = N'user' AND c.name = N'PhoneNumber';

    IF @constraintName IS NOT NULL
    BEGIN
        EXEC(N'ALTER TABLE [user] DROP CONSTRAINT [' + @constraintName + N']');
    END

    ALTER TABLE [user] DROP COLUMN [PhoneNumber];
END
");
        }
    }
}
