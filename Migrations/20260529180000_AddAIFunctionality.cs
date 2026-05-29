using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BloomyBE.Migrations
{
    /// <inheritdoc />
    public partial class AddAIFunctionality : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "IndoorOutdoor",
                table: "PortfolioItems",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Style",
                table: "PortfolioItems",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Tags",
                table: "PortfolioItems",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "ToneColor",
                table: "PortfolioItems",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateTable(
                name: "AIConversations",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Title = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    GatheredRequirementsJson = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    SpaceAnalysisJson = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UploadedSpaceImageUrl = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AIConversations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AIConversations_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AIUsages",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UsageType = table.Column<int>(type: "int", nullable: false),
                    RequestCount = table.Column<int>(type: "int", nullable: false),
                    TokensUsed = table.Column<int>(type: "int", nullable: true),
                    UsageDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AIUsages", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AIUsages_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AIMessages",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ConversationId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Role = table.Column<int>(type: "int", nullable: false),
                    MessageType = table.Column<int>(type: "int", nullable: false),
                    Content = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ImageUrl = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    MetadataJson = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AIMessages", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AIMessages_AIConversations_ConversationId",
                        column: x => x.ConversationId,
                        principalTable: "AIConversations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "SavedConcepts",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ConversationId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ToneColor = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Style = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    EstimatedBudget = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    PreviewImageUrl = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    ConceptDataJson = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    MatchedPortfolioIdsJson = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsAiGenerated = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SavedConcepts", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SavedConcepts_AIConversations_ConversationId",
                        column: x => x.ConversationId,
                        principalTable: "AIConversations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_SavedConcepts_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AIConversations_CreatedAt",
                table: "AIConversations",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_AIConversations_UserId",
                table: "AIConversations",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_AIMessages_ConversationId",
                table: "AIMessages",
                column: "ConversationId");

            migrationBuilder.CreateIndex(
                name: "IX_AIMessages_CreatedAt",
                table: "AIMessages",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_AIUsages_UserId_UsageType_UsageDate",
                table: "AIUsages",
                columns: new[] { "UserId", "UsageType", "UsageDate" });

            migrationBuilder.CreateIndex(
                name: "IX_SavedConcepts_CreatedAt",
                table: "SavedConcepts",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_SavedConcepts_UserId",
                table: "SavedConcepts",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_SavedConcepts_ConversationId",
                table: "SavedConcepts",
                column: "ConversationId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(name: "AIMessages");
            migrationBuilder.DropTable(name: "AIUsages");
            migrationBuilder.DropTable(name: "SavedConcepts");
            migrationBuilder.DropTable(name: "AIConversations");

            migrationBuilder.DropColumn(name: "IndoorOutdoor", table: "PortfolioItems");
            migrationBuilder.DropColumn(name: "Style", table: "PortfolioItems");
            migrationBuilder.DropColumn(name: "Tags", table: "PortfolioItems");
            migrationBuilder.DropColumn(name: "ToneColor", table: "PortfolioItems");
        }
    }
}
