using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ApnaRent.Migrations
{
    /// <inheritdoc />
    public partial class AddItemApproval : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ApprovalStatus",
                table: "Items",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "ItemRejectionReason",
                table: "Items",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ApprovalStatus",
                table: "Items");

            migrationBuilder.DropColumn(
                name: "ItemRejectionReason",
                table: "Items");
        }
    }
}
