
using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ClubManagement.Migrations
{
    public partial class AddActivityAttendance : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ActivityAttendances",
                columns: table => new
                {
                    AttendanceId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),

                    FullName = table.Column<string>(type: "nvarchar(max)", nullable: false),

                    StudentCode = table.Column<string>(type: "nvarchar(max)", nullable: false),

                    ActivityId = table.Column<int>(type: "int", nullable: false),

                    AttendanceDate = table.Column<DateTime>(type: "datetime2", nullable: false),

                    EvidenceObjectKey = table.Column<string>(type: "nvarchar(max)", nullable: false),

                    EvidenceFileName = table.Column<string>(type: "nvarchar(max)", nullable: false),

                    Status = table.Column<string>(type: "nvarchar(max)", nullable: false),

                    TrainingPoints = table.Column<int>(type: "int", nullable: false),

                    AdminNote = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey(
                        "PK_ActivityAttendances",
                        x => x.AttendanceId);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ActivityAttendances");
        }
    }
}
