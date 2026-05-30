using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BloomyBE.Migrations
{
    /// <inheritdoc />
    public partial class AddMultiTenantShop : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Shops",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    LogoUrl = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    Address = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    PhoneNumber = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    OwnerId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Shops", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Shops_Users_OwnerId",
                        column: x => x.OwnerId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Shops_OwnerId",
                table: "Shops",
                column: "OwnerId",
                unique: true);

            // Tạo shop cho mỗi ShopOwner hiện có
            migrationBuilder.Sql(@"
                INSERT INTO Shops (Id, Name, Description, LogoUrl, Address, PhoneNumber, OwnerId, CreatedAt)
                SELECT NEWID(), N'Bloomy Decor - ' + u.FullName, N'Shop trang trí sự kiện', '', N'Hà Nội', u.PhoneNumber, u.Id, GETUTCDATE()
                FROM Users u
                WHERE u.Role = 1 AND NOT EXISTS (SELECT 1 FROM Shops s WHERE s.OwnerId = u.Id);
            ");

            migrationBuilder.AddColumn<Guid>(
                name: "ShopId",
                table: "Orders",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "ShopId",
                table: "ChatConversations",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "ShopId",
                table: "Concepts",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "ShopId",
                table: "PortfolioItems",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "ShopId",
                table: "ServicePackages",
                type: "uniqueidentifier",
                nullable: true);

            // Migrate Orders: ShopOwnerId -> ShopId
            migrationBuilder.Sql(@"
                UPDATE o SET o.ShopId = s.Id
                FROM Orders o
                INNER JOIN Shops s ON s.OwnerId = o.ShopOwnerId
                WHERE o.ShopOwnerId IS NOT NULL;
            ");

            // Migrate ChatConversations
            migrationBuilder.Sql(@"
                UPDATE c SET c.ShopId = s.Id
                FROM ChatConversations c
                INNER JOIN Shops s ON s.OwnerId = c.ShopOwnerId;
            ");

            // Gán portfolio orphan cho shop đầu tiên
            migrationBuilder.Sql(@"
                DECLARE @DefaultShopId uniqueidentifier = (SELECT TOP 1 Id FROM Shops ORDER BY CreatedAt);
                IF @DefaultShopId IS NOT NULL
                    UPDATE PortfolioItems SET ShopId = @DefaultShopId WHERE ShopId IS NULL;
            ");

            migrationBuilder.DropForeignKey(
                name: "FK_ChatConversations_Users_ShopOwnerId",
                table: "ChatConversations");

            migrationBuilder.DropForeignKey(
                name: "FK_Orders_Users_ShopOwnerId",
                table: "Orders");

            migrationBuilder.DropIndex(
                name: "IX_ChatConversations_ShopOwnerId",
                table: "ChatConversations");

            migrationBuilder.DropIndex(
                name: "IX_Orders_ShopOwnerId",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "ShopOwnerId",
                table: "ChatConversations");

            migrationBuilder.DropColumn(
                name: "ShopOwnerId",
                table: "Orders");

            migrationBuilder.CreateIndex(
                name: "IX_Orders_ShopId",
                table: "Orders",
                column: "ShopId");

            migrationBuilder.CreateIndex(
                name: "IX_ChatConversations_ShopId",
                table: "ChatConversations",
                column: "ShopId");

            migrationBuilder.CreateIndex(
                name: "IX_Concepts_ShopId",
                table: "Concepts",
                column: "ShopId");

            migrationBuilder.CreateIndex(
                name: "IX_PortfolioItems_ShopId",
                table: "PortfolioItems",
                column: "ShopId");

            migrationBuilder.CreateIndex(
                name: "IX_ServicePackages_ShopId",
                table: "ServicePackages",
                column: "ShopId");

            migrationBuilder.AddForeignKey(
                name: "FK_Orders_Shops_ShopId",
                table: "Orders",
                column: "ShopId",
                principalTable: "Shops",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_ChatConversations_Shops_ShopId",
                table: "ChatConversations",
                column: "ShopId",
                principalTable: "Shops",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Concepts_Shops_ShopId",
                table: "Concepts",
                column: "ShopId",
                principalTable: "Shops",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_PortfolioItems_Shops_ShopId",
                table: "PortfolioItems",
                column: "ShopId",
                principalTable: "Shops",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_ServicePackages_Shops_ShopId",
                table: "ServicePackages",
                column: "ShopId",
                principalTable: "Shops",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(name: "FK_ServicePackages_Shops_ShopId", table: "ServicePackages");
            migrationBuilder.DropForeignKey(name: "FK_PortfolioItems_Shops_ShopId", table: "PortfolioItems");
            migrationBuilder.DropForeignKey(name: "FK_Concepts_Shops_ShopId", table: "Concepts");
            migrationBuilder.DropForeignKey(name: "FK_ChatConversations_Shops_ShopId", table: "ChatConversations");
            migrationBuilder.DropForeignKey(name: "FK_Orders_Shops_ShopId", table: "Orders");

            migrationBuilder.DropIndex(name: "IX_ServicePackages_ShopId", table: "ServicePackages");
            migrationBuilder.DropIndex(name: "IX_PortfolioItems_ShopId", table: "PortfolioItems");
            migrationBuilder.DropIndex(name: "IX_Concepts_ShopId", table: "Concepts");
            migrationBuilder.DropIndex(name: "IX_ChatConversations_ShopId", table: "ChatConversations");
            migrationBuilder.DropIndex(name: "IX_Orders_ShopId", table: "Orders");

            migrationBuilder.AddColumn<Guid>(
                name: "ShopOwnerId",
                table: "Orders",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "ShopOwnerId",
                table: "ChatConversations",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: Guid.Empty);

            migrationBuilder.DropColumn(name: "ShopId", table: "ServicePackages");
            migrationBuilder.DropColumn(name: "ShopId", table: "PortfolioItems");
            migrationBuilder.DropColumn(name: "ShopId", table: "Concepts");
            migrationBuilder.DropColumn(name: "ShopId", table: "ChatConversations");
            migrationBuilder.DropColumn(name: "ShopId", table: "Orders");

            migrationBuilder.DropTable(name: "Shops");
        }
    }
}
