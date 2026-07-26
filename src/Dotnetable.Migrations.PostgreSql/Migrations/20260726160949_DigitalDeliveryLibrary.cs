using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Dotnetable.Migrations.PostgreSql.Migrations
{
    /// <inheritdoc />
    public partial class DigitalDeliveryLibrary : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "OrderDigitalAssets",
                columns: table => new
                {
                    OrderDigitalAssetID = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    WebsiteID = table.Column<int>(type: "integer", nullable: false),
                    WebsiteClientID = table.Column<int>(type: "integer", nullable: false),
                    OrderID = table.Column<int>(type: "integer", nullable: false),
                    OrderItemID = table.Column<int>(type: "integer", nullable: false),
                    ProductID = table.Column<int>(type: "integer", nullable: false),
                    ProductType = table.Column<byte>(type: "smallint", nullable: false),
                    TitleSnapshot = table.Column<string>(type: "character varying(400)", maxLength: 400, nullable: false),
                    DigitalFileID = table.Column<int>(type: "integer", nullable: true),
                    DigitalServiceUrl = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    DigitalDeliveryNote = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    OriginalFileName = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    GrantedAt = table.Column<DateTime>(type: "datetime", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OrderDigitalAssets", x => x.OrderDigitalAssetID);
                    table.ForeignKey(
                        name: "FK_OrderDigitalAssets_FileRecords",
                        column: x => x.DigitalFileID,
                        principalTable: "FileRecords",
                        principalColumn: "FileRecordID");
                    table.ForeignKey(
                        name: "FK_OrderDigitalAssets_OrderItems",
                        column: x => x.OrderItemID,
                        principalTable: "OrderItems",
                        principalColumn: "OrderItemID");
                    table.ForeignKey(
                        name: "FK_OrderDigitalAssets_Orders",
                        column: x => x.OrderID,
                        principalTable: "Orders",
                        principalColumn: "OrderID");
                    table.ForeignKey(
                        name: "FK_OrderDigitalAssets_Products",
                        column: x => x.ProductID,
                        principalTable: "Products",
                        principalColumn: "ProductID");
                    table.ForeignKey(
                        name: "FK_OrderDigitalAssets_WebsiteClients",
                        column: x => x.WebsiteClientID,
                        principalTable: "WebsiteClients",
                        principalColumn: "WebsiteClientID");
                    table.ForeignKey(
                        name: "FK_OrderDigitalAssets_Websites",
                        column: x => x.WebsiteID,
                        principalTable: "Websites",
                        principalColumn: "WebsiteID");
                });

            migrationBuilder.CreateTable(
                name: "DigitalAccessLogs",
                columns: table => new
                {
                    DigitalAccessLogID = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    OrderDigitalAssetID = table.Column<int>(type: "integer", nullable: false),
                    WebsiteClientID = table.Column<int>(type: "integer", nullable: false),
                    WebsiteID = table.Column<int>(type: "integer", nullable: false),
                    AccessType = table.Column<byte>(type: "smallint", nullable: false),
                    IpAddress = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    UserAgent = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    AccessedAt = table.Column<DateTime>(type: "datetime", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DigitalAccessLogs", x => x.DigitalAccessLogID);
                    table.ForeignKey(
                        name: "FK_DigitalAccessLogs_OrderDigitalAssets",
                        column: x => x.OrderDigitalAssetID,
                        principalTable: "OrderDigitalAssets",
                        principalColumn: "OrderDigitalAssetID");
                    table.ForeignKey(
                        name: "FK_DigitalAccessLogs_WebsiteClients",
                        column: x => x.WebsiteClientID,
                        principalTable: "WebsiteClients",
                        principalColumn: "WebsiteClientID");
                    table.ForeignKey(
                        name: "FK_DigitalAccessLogs_Websites",
                        column: x => x.WebsiteID,
                        principalTable: "Websites",
                        principalColumn: "WebsiteID");
                });

            migrationBuilder.CreateIndex(
                name: "IX_DigitalAccessLogs_AccessedAt",
                table: "DigitalAccessLogs",
                column: "AccessedAt");

            migrationBuilder.CreateIndex(
                name: "IX_DigitalAccessLogs_OrderDigitalAssetID",
                table: "DigitalAccessLogs",
                column: "OrderDigitalAssetID");

            migrationBuilder.CreateIndex(
                name: "IX_DigitalAccessLogs_WebsiteClientID",
                table: "DigitalAccessLogs",
                column: "WebsiteClientID");

            migrationBuilder.CreateIndex(
                name: "IX_DigitalAccessLogs_WebsiteID",
                table: "DigitalAccessLogs",
                column: "WebsiteID");

            migrationBuilder.CreateIndex(
                name: "IX_OrderDigitalAssets_DigitalFileID",
                table: "OrderDigitalAssets",
                column: "DigitalFileID");

            migrationBuilder.CreateIndex(
                name: "IX_OrderDigitalAssets_OrderID",
                table: "OrderDigitalAssets",
                column: "OrderID");

            migrationBuilder.CreateIndex(
                name: "IX_OrderDigitalAssets_ProductID",
                table: "OrderDigitalAssets",
                column: "ProductID");

            migrationBuilder.CreateIndex(
                name: "IX_OrderDigitalAssets_WebsiteClientID",
                table: "OrderDigitalAssets",
                column: "WebsiteClientID");

            migrationBuilder.CreateIndex(
                name: "IX_OrderDigitalAssets_WebsiteID",
                table: "OrderDigitalAssets",
                column: "WebsiteID");

            migrationBuilder.CreateIndex(
                name: "UQ_OrderDigitalAssets_OrderItemID",
                table: "OrderDigitalAssets",
                column: "OrderItemID",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "DigitalAccessLogs");

            migrationBuilder.DropTable(
                name: "OrderDigitalAssets");
        }
    }
}
