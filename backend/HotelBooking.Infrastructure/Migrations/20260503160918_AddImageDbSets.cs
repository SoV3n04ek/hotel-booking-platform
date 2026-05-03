using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HotelBooking.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddImageDbSets : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_hotel_image_hotels_hotel_id",
                table: "hotel_image");

            migrationBuilder.DropForeignKey(
                name: "fk_room_image_rooms_room_id",
                table: "room_image");

            migrationBuilder.DropPrimaryKey(
                name: "pk_room_image",
                table: "room_image");

            migrationBuilder.DropPrimaryKey(
                name: "pk_hotel_image",
                table: "hotel_image");

            migrationBuilder.RenameTable(
                name: "room_image",
                newName: "room_images");

            migrationBuilder.RenameTable(
                name: "hotel_image",
                newName: "hotel_images");

            migrationBuilder.RenameIndex(
                name: "ix_room_image_room_id",
                table: "room_images",
                newName: "ix_room_images_room_id");

            migrationBuilder.RenameIndex(
                name: "ix_hotel_image_hotel_id",
                table: "hotel_images",
                newName: "ix_hotel_images_hotel_id");

            migrationBuilder.AddPrimaryKey(
                name: "pk_room_images",
                table: "room_images",
                column: "id");

            migrationBuilder.AddPrimaryKey(
                name: "pk_hotel_images",
                table: "hotel_images",
                column: "id");

            migrationBuilder.AddForeignKey(
                name: "fk_hotel_images_hotels_hotel_id",
                table: "hotel_images",
                column: "hotel_id",
                principalTable: "hotels",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "fk_room_images_rooms_room_id",
                table: "room_images",
                column: "room_id",
                principalTable: "rooms",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_hotel_images_hotels_hotel_id",
                table: "hotel_images");

            migrationBuilder.DropForeignKey(
                name: "fk_room_images_rooms_room_id",
                table: "room_images");

            migrationBuilder.DropPrimaryKey(
                name: "pk_room_images",
                table: "room_images");

            migrationBuilder.DropPrimaryKey(
                name: "pk_hotel_images",
                table: "hotel_images");

            migrationBuilder.RenameTable(
                name: "room_images",
                newName: "room_image");

            migrationBuilder.RenameTable(
                name: "hotel_images",
                newName: "hotel_image");

            migrationBuilder.RenameIndex(
                name: "ix_room_images_room_id",
                table: "room_image",
                newName: "ix_room_image_room_id");

            migrationBuilder.RenameIndex(
                name: "ix_hotel_images_hotel_id",
                table: "hotel_image",
                newName: "ix_hotel_image_hotel_id");

            migrationBuilder.AddPrimaryKey(
                name: "pk_room_image",
                table: "room_image",
                column: "id");

            migrationBuilder.AddPrimaryKey(
                name: "pk_hotel_image",
                table: "hotel_image",
                column: "id");

            migrationBuilder.AddForeignKey(
                name: "fk_hotel_image_hotels_hotel_id",
                table: "hotel_image",
                column: "hotel_id",
                principalTable: "hotels",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "fk_room_image_rooms_room_id",
                table: "room_image",
                column: "room_id",
                principalTable: "rooms",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
