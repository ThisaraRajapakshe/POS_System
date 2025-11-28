using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace POS_System.Migrations
{
    /// <inheritdoc />
    public partial class AddQuantityColumnToLineItems : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "Quantity",
                table: "ProductLineItems",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Quantity",
                table: "ProductLineItems");
        }
    }
}
