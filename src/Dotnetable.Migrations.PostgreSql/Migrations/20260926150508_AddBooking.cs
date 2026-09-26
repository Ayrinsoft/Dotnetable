using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Dotnetable.Migrations.PostgreSql.Migrations
{
    /// <inheritdoc />
    public partial class AddBooking : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "BookingProfiles",
                columns: table => new
                {
                    BookingProfileID = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    WebsiteID = table.Column<int>(type: "integer", nullable: false),
                    TimeZoneId = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    BookThrough = table.Column<DateOnly>(type: "date", nullable: true),
                    RetentionDays = table.Column<int>(type: "integer", nullable: false, defaultValue: 180),
                    RequirePayment = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    AllowOnlinePayment = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    AllowOfflinePayment = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    HoldMinutes = table.Column<int>(type: "integer", nullable: false, defaultValue: 30),
                    LeadMinutes = table.Column<int>(type: "integer", nullable: false, defaultValue: 60),
                    DayStartMinutes = table.Column<int>(type: "integer", nullable: false, defaultValue: 540),
                    DayEndMinutes = table.Column<int>(type: "integer", nullable: false, defaultValue: 1020),
                    WorkDays = table.Column<byte>(type: "smallint", nullable: false, defaultValue: (byte)31),
                    BreakStartMinutes = table.Column<int>(type: "integer", nullable: true),
                    BreakEndMinutes = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BookingProfiles", x => x.BookingProfileID);
                    table.ForeignKey(
                        name: "FK_BookingProfiles_Websites_WebsiteID",
                        column: x => x.WebsiteID,
                        principalTable: "Websites",
                        principalColumn: "WebsiteID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "BookingResources",
                columns: table => new
                {
                    BookingResourceID = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    WebsiteID = table.Column<int>(type: "integer", nullable: false),
                    Name = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    SortOrder = table.Column<int>(type: "integer", nullable: false),
                    DayStartMinutes = table.Column<int>(type: "integer", nullable: false, defaultValue: 540),
                    DayEndMinutes = table.Column<int>(type: "integer", nullable: false, defaultValue: 1020),
                    WorkDays = table.Column<byte>(type: "smallint", nullable: false, defaultValue: (byte)31),
                    BreakStartMinutes = table.Column<int>(type: "integer", nullable: true),
                    BreakEndMinutes = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BookingResources", x => x.BookingResourceID);
                    table.ForeignKey(
                        name: "FK_BookingResources_Websites_WebsiteID",
                        column: x => x.WebsiteID,
                        principalTable: "Websites",
                        principalColumn: "WebsiteID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "BookingClosures",
                columns: table => new
                {
                    BookingClosureID = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    WebsiteID = table.Column<int>(type: "integer", nullable: false),
                    BookingResourceID = table.Column<int>(type: "integer", nullable: true),
                    Date = table.Column<DateOnly>(type: "date", nullable: false),
                    Note = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BookingClosures", x => x.BookingClosureID);
                    table.ForeignKey(
                        name: "FK_BookingClosures_BookingResources_BookingResourceID",
                        column: x => x.BookingResourceID,
                        principalTable: "BookingResources",
                        principalColumn: "BookingResourceID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "BookingOfferings",
                columns: table => new
                {
                    BookingOfferingID = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    WebsiteID = table.Column<int>(type: "integer", nullable: false),
                    BookingResourceID = table.Column<int>(type: "integer", nullable: false),
                    Name = table.Column<string>(type: "character varying(160)", maxLength: 160, nullable: false),
                    Description = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    DurationMinutes = table.Column<int>(type: "integer", nullable: false),
                    FullPrice = table.Column<decimal>(type: "numeric(18,4)", nullable: false),
                    DepositAmount = table.Column<decimal>(type: "numeric(18,4)", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    SortOrder = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BookingOfferings", x => x.BookingOfferingID);
                    table.ForeignKey(
                        name: "FK_BookingOfferings_BookingResources_BookingResourceID",
                        column: x => x.BookingResourceID,
                        principalTable: "BookingResources",
                        principalColumn: "BookingResourceID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "BookingAppointments",
                columns: table => new
                {
                    BookingAppointmentID = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    WebsiteID = table.Column<int>(type: "integer", nullable: false),
                    BookingResourceID = table.Column<int>(type: "integer", nullable: false),
                    BookingOfferingID = table.Column<int>(type: "integer", nullable: false),
                    WebsiteClientID = table.Column<int>(type: "integer", nullable: true),
                    OrderID = table.Column<int>(type: "integer", nullable: true),
                    CustomerName = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false),
                    Phone = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: true),
                    Note = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    StartsAtUtc = table.Column<DateTime>(type: "datetime", nullable: false),
                    EndsAtUtc = table.Column<DateTime>(type: "datetime", nullable: false),
                    Status = table.Column<byte>(type: "smallint", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime", nullable: false),
                    CreatedByMemberID = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BookingAppointments", x => x.BookingAppointmentID);
                    table.ForeignKey(
                        name: "FK_BookingAppointments_BookingOfferings_BookingOfferingID",
                        column: x => x.BookingOfferingID,
                        principalTable: "BookingOfferings",
                        principalColumn: "BookingOfferingID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_BookingAppointments_BookingResources_BookingResourceID",
                        column: x => x.BookingResourceID,
                        principalTable: "BookingResources",
                        principalColumn: "BookingResourceID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_BookingAppointments_Orders_OrderID",
                        column: x => x.OrderID,
                        principalTable: "Orders",
                        principalColumn: "OrderID",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateIndex(
                name: "IX_BookingAppointments_BookingOfferingID",
                table: "BookingAppointments",
                column: "BookingOfferingID");

            migrationBuilder.CreateIndex(
                name: "IX_BookingAppointments_OrderID",
                table: "BookingAppointments",
                column: "OrderID");

            migrationBuilder.CreateIndex(
                name: "IX_BookingAppointments_Resource_Start",
                table: "BookingAppointments",
                columns: new[] { "BookingResourceID", "StartsAtUtc" });

            migrationBuilder.CreateIndex(
                name: "IX_BookingAppointments_WebsiteID",
                table: "BookingAppointments",
                column: "WebsiteID");

            migrationBuilder.CreateIndex(
                name: "IX_BookingClosures_BookingResourceID",
                table: "BookingClosures",
                column: "BookingResourceID");

            migrationBuilder.CreateIndex(
                name: "IX_BookingClosures_WebsiteID_Date",
                table: "BookingClosures",
                columns: new[] { "WebsiteID", "Date" });

            migrationBuilder.CreateIndex(
                name: "IX_BookingOfferings_BookingResourceID",
                table: "BookingOfferings",
                column: "BookingResourceID");

            migrationBuilder.CreateIndex(
                name: "IX_BookingOfferings_WebsiteID",
                table: "BookingOfferings",
                column: "WebsiteID");

            migrationBuilder.CreateIndex(
                name: "UQ_BookingProfiles_WebsiteID",
                table: "BookingProfiles",
                column: "WebsiteID",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_BookingResources_WebsiteID",
                table: "BookingResources",
                column: "WebsiteID");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "BookingAppointments");

            migrationBuilder.DropTable(
                name: "BookingClosures");

            migrationBuilder.DropTable(
                name: "BookingProfiles");

            migrationBuilder.DropTable(
                name: "BookingOfferings");

            migrationBuilder.DropTable(
                name: "BookingResources");
        }
    }
}
