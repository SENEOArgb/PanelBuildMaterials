using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PanelBuildMaterials.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {

            migrationBuilder.CreateTable(
                name: "Services",
                columns: table => new
                {
                    serviceID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    nameService = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    priceService = table.Column<decimal>(type: "decimal(8,2)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Services", x => x.serviceID);
                });

            migrationBuilder.CreateTable(
                name: "Users",
                columns: table => new
                {
                    userID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    userLogin = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    userPasswordHash = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: false),
                    userLaws = table.Column<string>(type: "nvarchar(75)", maxLength: 75, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.userID);
                });

            migrationBuilder.CreateTable(
                name: "Warehouses",
                columns: table => new
                {
                    warehouseID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    warehouseName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    warehouseCity = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    warehouseStreet = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Warehouses", x => x.warehouseID);
                });

            migrationBuilder.CreateTable(
                name: "BuildingMaterials",
                columns: table => new
                {
                    buildingMaterialID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    categoryId = table.Column<int>(type: "int", nullable: false),
                    nameBuildingMaterial = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: false),
                    retailPrice = table.Column<decimal>(type: "decimal(8,0)", nullable: false),
                    wholesalePrice = table.Column<decimal>(type: "decimal(8,0)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BuildingMaterials", x => x.buildingMaterialID);
                    table.ForeignKey(
                        name: "FK_BuildingMaterials_Categories1",
                        column: x => x.categoryId,
                        principalTable: "Categories",
                        principalColumn: "categoryID");
                });

            migrationBuilder.CreateTable(
                name: "Logs",
                columns: table => new
                {
                    logID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    userId = table.Column<int>(type: "int", nullable: false),
                    dateTimeLog = table.Column<DateTime>(type: "datetime", nullable: false),
                    logDescription = table.Column<string>(type: "nvarchar(550)", maxLength: 550, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Logs", x => x.logID);
                    table.ForeignKey(
                        name: "FK_Logs_Users",
                        column: x => x.userId,
                        principalTable: "Users",
                        principalColumn: "userID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Orders",
                columns: table => new
                {
                    orderID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    userId = table.Column<int>(type: "int", nullable: false),
                    dateOrder = table.Column<DateOnly>(type: "date", nullable: false),
                    timeOrder = table.Column<TimeOnly>(type: "time(2)", precision: 2, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Orders", x => x.orderID);
                    table.ForeignKey(
                        name: "FK_Orders_Users1",
                        column: x => x.userId,
                        principalTable: "Users",
                        principalColumn: "userID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "BuildingMaterialsWarehouses",
                columns: table => new
                {
                    buildingMaterialWarehouseID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    buildingMaterialId = table.Column<int>(type: "int", nullable: false),
                    warehouseId = table.Column<int>(type: "int", nullable: false),
                    amountBuildingMaterial = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BuildingMaterialsWarehouses", x => x.buildingMaterialWarehouseID);
                    table.ForeignKey(
                        name: "FK_BuildingMaterialsWarehouses_BuildingMaterials",
                        column: x => x.buildingMaterialId,
                        principalTable: "BuildingMaterials",
                        principalColumn: "buildingMaterialID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_BuildingMaterialsWarehouses_Warehouses1",
                        column: x => x.warehouseId,
                        principalTable: "Warehouses",
                        principalColumn: "warehouseID");
                });

            migrationBuilder.CreateTable(
                name: "BuildingMaterialsServicesOrders",
                columns: table => new
                {
                    BuildingMaterialServiceOrderID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    buildingMaterialId = table.Column<int>(type: "int", nullable: true),
                    serviceId = table.Column<int>(type: "int", nullable: true),
                    orderId = table.Column<int>(type: "int", nullable: false),
                    countBuildingMaterial = table.Column<int>(type: "int", nullable: true),
                    orderPrice = table.Column<decimal>(type: "decimal(9,2)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BuildingMaterialsServicesOrders", x => x.BuildingMaterialServiceOrderID);
                    table.ForeignKey(
                        name: "FK_BuildingMaterialsServicesOrders_BuildingMaterials1",
                        column: x => x.buildingMaterialId,
                        principalTable: "BuildingMaterials",
                        principalColumn: "buildingMaterialID");
                    table.ForeignKey(
                        name: "FK_BuildingMaterialsServicesOrders_Orders1",
                        column: x => x.orderId,
                        principalTable: "Orders",
                        principalColumn: "orderID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_BuildingMaterialsServicesOrders_Services1",
                        column: x => x.serviceId,
                        principalTable: "Services",
                        principalColumn: "serviceID");
                });

            migrationBuilder.CreateIndex(
                name: "IX_BuildingMaterials_categoryId",
                table: "BuildingMaterials",
                column: "categoryId");

            migrationBuilder.CreateIndex(
                name: "IX_BuildingMaterialsServicesOrders_buildingMaterialId",
                table: "BuildingMaterialsServicesOrders",
                column: "buildingMaterialId");

            migrationBuilder.CreateIndex(
                name: "IX_BuildingMaterialsServicesOrders_orderId",
                table: "BuildingMaterialsServicesOrders",
                column: "orderId");

            migrationBuilder.CreateIndex(
                name: "IX_BuildingMaterialsServicesOrders_serviceId",
                table: "BuildingMaterialsServicesOrders",
                column: "serviceId");

            migrationBuilder.CreateIndex(
                name: "IX_BuildingMaterialsWarehouses_buildingMaterialId",
                table: "BuildingMaterialsWarehouses",
                column: "buildingMaterialId");

            migrationBuilder.CreateIndex(
                name: "IX_BuildingMaterialsWarehouses_warehouseId",
                table: "BuildingMaterialsWarehouses",
                column: "warehouseId");

            migrationBuilder.CreateIndex(
                name: "IX_Logs_userId",
                table: "Logs",
                column: "userId");

            migrationBuilder.CreateIndex(
                name: "IX_Orders_userId",
                table: "Orders",
                column: "userId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "BuildingMaterialsServicesOrders");

            migrationBuilder.DropTable(
                name: "BuildingMaterialsWarehouses");

            migrationBuilder.DropTable(
                name: "Logs");

            migrationBuilder.DropTable(
                name: "Orders");

            migrationBuilder.DropTable(
                name: "Services");

            migrationBuilder.DropTable(
                name: "BuildingMaterials");

            migrationBuilder.DropTable(
                name: "Warehouses");

            migrationBuilder.DropTable(
                name: "Users");

            migrationBuilder.DropTable(
                name: "Categories");
        }
    }
}
