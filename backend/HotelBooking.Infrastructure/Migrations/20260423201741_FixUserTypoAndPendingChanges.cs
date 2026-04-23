using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HotelBooking.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class FixUserTypoAndPendingChanges : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "passwor_hash",
                table: "users",
                newName: "password_hash");

            migrationBuilder.AddColumn<uint>(
                name: "xmin",
                table: "rooms",
                type: "xid",
                rowVersion: true,
                nullable: false,
                defaultValue: 0u);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "xmin",
                table: "rooms");

            migrationBuilder.RenameColumn(
                name: "password_hash",
                table: "users",
                newName: "passwor_hash");
        }
    }
}
