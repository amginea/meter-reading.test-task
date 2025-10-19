using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace meter_reading.Infrastructure.Database.EF.Migrations
{
    /// <inheritdoc />
    public partial class Seeddata : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData("Accounts", columns: ["AccountId", "FirstName", "LastName", "Created", "Updated"], values: new object[,]
            {
                {2344, "Tommy", "Test", DateTime.UtcNow, DateTime.UtcNow},
                {2233, "Barry", "Test", DateTime.UtcNow, DateTime.UtcNow},
                {8766, "Sally", "Test", DateTime.UtcNow, DateTime.UtcNow},
                {2345, "Jerry", "Test", DateTime.UtcNow, DateTime.UtcNow},
                {2346, "Ollie", "Test", DateTime.UtcNow, DateTime.UtcNow},
                {2347, "Tara", "Test", DateTime.UtcNow, DateTime.UtcNow},
                {2348, "Tammy", "Test", DateTime.UtcNow, DateTime.UtcNow},
                {2349, "Simon", "Test", DateTime.UtcNow, DateTime.UtcNow},
                {2350, "Colin", "Test", DateTime.UtcNow, DateTime.UtcNow},
                {2351, "Gladys", "Test", DateTime.UtcNow, DateTime.UtcNow},
                {2352, "Greg", "Test", DateTime.UtcNow, DateTime.UtcNow},
                {2353, "Tony", "Test", DateTime.UtcNow, DateTime.UtcNow},
                {2355, "Arthur", "Test", DateTime.UtcNow, DateTime.UtcNow},
                {2356, "Craig", "Test", DateTime.UtcNow, DateTime.UtcNow},
                {6776, "Laura", "Test", DateTime.UtcNow, DateTime.UtcNow},
                {4534, "JOSH", "TEST", DateTime.UtcNow, DateTime.UtcNow},
                {1234, "Freya", "Test", DateTime.UtcNow, DateTime.UtcNow},
                {1239, "Noddy", "Test", DateTime.UtcNow, DateTime.UtcNow},
                {1240, "Archie", "Test", DateTime.UtcNow, DateTime.UtcNow},
                {1241, "Lara", "Test", DateTime.UtcNow, DateTime.UtcNow},
                {1242, "Tim", "Test", DateTime.UtcNow, DateTime.UtcNow},
                {1243, "Graham", "Test", DateTime.UtcNow, DateTime.UtcNow},
                {1244, "Tony", "Test", DateTime.UtcNow, DateTime.UtcNow},
                {1245, "Neville", "Test", DateTime.UtcNow, DateTime.UtcNow},
                {1246, "Jo", "Test", DateTime.UtcNow, DateTime.UtcNow},
                {1247, "Jim", "Test", DateTime.UtcNow, DateTime.UtcNow},
                {1248, "Pam", "Test", DateTime.UtcNow, DateTime.UtcNow}
            });

        }
    }
}
