using System;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    public partial class somethingMigration : Migration
    {
        private readonly IDbContextSchema Ctx;

        public somethingMigration(IDbContextSchema ctx)
        {
            Ctx = ctx ?? throw new ArgumentNullException(nameof(ctx));
        }

        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsSomething",
                table: "Products",
                type: "bit",
                schema: Ctx.TenantId,
                nullable: false,
                defaultValue: false);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsSomething",
                schema: Ctx.TenantId,
                table: "Products");
        }
    }
}
