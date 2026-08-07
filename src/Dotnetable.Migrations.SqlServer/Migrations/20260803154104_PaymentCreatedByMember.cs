using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Dotnetable.Migrations.SqlServer.Migrations
{
    /// <inheritdoc />
    public partial class PaymentCreatedByMember : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "CreatedByMemberID",
                table: "Payments",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Payments_CreatedByMemberID",
                table: "Payments",
                column: "CreatedByMemberID");

            migrationBuilder.AddForeignKey(
                name: "FK_Payments_Members_CreatedBy",
                table: "Payments",
                column: "CreatedByMemberID",
                principalTable: "Members",
                principalColumn: "MemberID");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Payments_Members_CreatedBy",
                table: "Payments");

            migrationBuilder.DropIndex(
                name: "IX_Payments_CreatedByMemberID",
                table: "Payments");

            migrationBuilder.DropColumn(
                name: "CreatedByMemberID",
                table: "Payments");
        }
    }
}
