using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ProyectoAvengers.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class RoleHierarchyLevel : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "HierarchyLevel",
                table: "roles",
                type: "integer",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "HierarchyLevel",
                table: "roles");
        }
    }
}
