using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WebApplication1.Migrations
{
    public partial class AddAuthFieldsToUser : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Thêm các cột mới cho Authentication/RefreshToken/Role
            migrationBuilder.AddColumn<string>(
                name: "Username",
                table: "Users",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: ""); // DefaultValue để tránh lỗi nếu bảng đã có dữ liệu

            migrationBuilder.AddColumn<string>(
                name: "PasswordHash",
                table: "Users",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Role",
                table: "Users",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "User");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Rollback: xóa các cột vừa thêm
            migrationBuilder.DropColumn(name: "Username", table: "Users");
            migrationBuilder.DropColumn(name: "PasswordHash", table: "Users");
            migrationBuilder.DropColumn(name: "Role", table: "Users");
            migrationBuilder.DropColumn(name: "RefreshToken", table: "Users");
            migrationBuilder.DropColumn(name: "RefreshTokenExpiryTime", table: "Users");
        }
    }
}