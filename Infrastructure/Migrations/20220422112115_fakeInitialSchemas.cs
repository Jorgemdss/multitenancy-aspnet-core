using System;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    public partial class fakeInitialSchemas : Migration
    {

        private readonly IDbContextSchema Ctx;
        public fakeInitialSchemas()
        {
            
        }

        public fakeInitialSchemas(IDbContextSchema ctx)
        {
            Ctx = ctx ?? throw new ArgumentNullException(nameof(ctx));
            Console.WriteLine("Migration schema: " + Ctx.TenantId);
            Console.WriteLine("***************");
        }

        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Products",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Rate = table.Column<int>(type: "int", nullable: false),
                    Cebola = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                schema: Ctx.TenantId,
                constraints: table =>
                {
                    table.PrimaryKey("PK_Products", x => x.Id);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Products",
                schema: Ctx.TenantId);
        }
    }
}
