using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BookStore.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddCheckConstraints : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddCheckConstraint(
                name: "CK_OrderBook_Quantity",
                table: "OrderBooks",
                sql: "[Quantity] >= 1 AND [Quantity] <= 10000");

            migrationBuilder.AddCheckConstraint(
                name: "CK_Book_NumberOfCopies",
                table: "Books",
                sql: "[NumberOfCopies] >= 0 AND [NumberOfCopies] <= 100000");

            migrationBuilder.AddCheckConstraint(
                name: "CK_Book_Price",
                table: "Books",
                sql: "[Price] >= 0 AND [Price] <= 9999.99");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropCheckConstraint(
                name: "CK_OrderBook_Quantity",
                table: "OrderBooks");

            migrationBuilder.DropCheckConstraint(
                name: "CK_Book_NumberOfCopies",
                table: "Books");

            migrationBuilder.DropCheckConstraint(
                name: "CK_Book_Price",
                table: "Books");
        }
    }
}
