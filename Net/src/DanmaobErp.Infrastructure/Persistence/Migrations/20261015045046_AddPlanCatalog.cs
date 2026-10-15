using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace DanmaobErp.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddPlanCatalog : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Plans",
                schema: "platform",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    IsDefaultForRegistration = table.Column<bool>(type: "bit", nullable: false),
                    ClusterKey = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Plans", x => x.Id)
                        .Annotation("SqlServer:Clustered", false);
                });

            migrationBuilder.CreateTable(
                name: "PlanModules",
                schema: "platform",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PlanId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Module = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    ClusterKey = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PlanModules", x => x.Id)
                        .Annotation("SqlServer:Clustered", false);
                    table.ForeignKey(
                        name: "FK_PlanModules_Plans_PlanId",
                        column: x => x.PlanId,
                        principalSchema: "platform",
                        principalTable: "Plans",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "PlanQuotas",
                schema: "platform",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PlanId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Quota = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: false),
                    Limit = table.Column<long>(type: "bigint", nullable: true),
                    ReturnsOnCancel = table.Column<bool>(type: "bit", nullable: false),
                    ClusterKey = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PlanQuotas", x => x.Id)
                        .Annotation("SqlServer:Clustered", false);
                    table.ForeignKey(
                        name: "FK_PlanQuotas_Plans_PlanId",
                        column: x => x.PlanId,
                        principalSchema: "platform",
                        principalTable: "Plans",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.InsertData(
                schema: "platform",
                table: "Plans",
                columns: new[] { "Id", "IsActive", "IsDefaultForRegistration", "Name" },
                values: new object[,]
                {
                    { new Guid("019a0000-0000-7000-8000-000100000000"), true, true, "Free" },
                    { new Guid("019a0000-0000-7000-8000-000200000000"), true, false, "Básico" },
                    { new Guid("019a0000-0000-7000-8000-000300000000"), false, false, "Premium" }
                });

            migrationBuilder.InsertData(
                schema: "platform",
                table: "PlanModules",
                columns: new[] { "Id", "Module", "PlanId" },
                values: new object[,]
                {
                    { new Guid("019a0000-0000-7000-8000-000101000001"), "Customers", new Guid("019a0000-0000-7000-8000-000100000000") },
                    { new Guid("019a0000-0000-7000-8000-000101000002"), "Inventory", new Guid("019a0000-0000-7000-8000-000100000000") },
                    { new Guid("019a0000-0000-7000-8000-000101000003"), "Sales", new Guid("019a0000-0000-7000-8000-000100000000") },
                    { new Guid("019a0000-0000-7000-8000-000201000001"), "Customers", new Guid("019a0000-0000-7000-8000-000200000000") },
                    { new Guid("019a0000-0000-7000-8000-000201000002"), "Inventory", new Guid("019a0000-0000-7000-8000-000200000000") },
                    { new Guid("019a0000-0000-7000-8000-000201000003"), "Sales", new Guid("019a0000-0000-7000-8000-000200000000") },
                    { new Guid("019a0000-0000-7000-8000-000201000004"), "Purchasing", new Guid("019a0000-0000-7000-8000-000200000000") },
                    { new Guid("019a0000-0000-7000-8000-000201000005"), "Invoicing", new Guid("019a0000-0000-7000-8000-000200000000") },
                    { new Guid("019a0000-0000-7000-8000-000201000006"), "Finance", new Guid("019a0000-0000-7000-8000-000200000000") },
                    { new Guid("019a0000-0000-7000-8000-000201000007"), "Reports", new Guid("019a0000-0000-7000-8000-000200000000") },
                    { new Guid("019a0000-0000-7000-8000-000201000008"), "BusinessEmail", new Guid("019a0000-0000-7000-8000-000200000000") },
                    { new Guid("019a0000-0000-7000-8000-000301000001"), "Customers", new Guid("019a0000-0000-7000-8000-000300000000") },
                    { new Guid("019a0000-0000-7000-8000-000301000002"), "Inventory", new Guid("019a0000-0000-7000-8000-000300000000") },
                    { new Guid("019a0000-0000-7000-8000-000301000003"), "Sales", new Guid("019a0000-0000-7000-8000-000300000000") },
                    { new Guid("019a0000-0000-7000-8000-000301000004"), "Purchasing", new Guid("019a0000-0000-7000-8000-000300000000") },
                    { new Guid("019a0000-0000-7000-8000-000301000005"), "Invoicing", new Guid("019a0000-0000-7000-8000-000300000000") },
                    { new Guid("019a0000-0000-7000-8000-000301000006"), "Finance", new Guid("019a0000-0000-7000-8000-000300000000") },
                    { new Guid("019a0000-0000-7000-8000-000301000007"), "Reports", new Guid("019a0000-0000-7000-8000-000300000000") },
                    { new Guid("019a0000-0000-7000-8000-000301000008"), "BusinessEmail", new Guid("019a0000-0000-7000-8000-000300000000") }
                });

            migrationBuilder.InsertData(
                schema: "platform",
                table: "PlanQuotas",
                columns: new[] { "Id", "Limit", "PlanId", "Quota", "ReturnsOnCancel" },
                values: new object[,]
                {
                    { new Guid("019a0000-0000-7000-8000-000102000001"), 1L, new Guid("019a0000-0000-7000-8000-000100000000"), "Tenants", false },
                    { new Guid("019a0000-0000-7000-8000-000102000002"), 2L, new Guid("019a0000-0000-7000-8000-000100000000"), "ActiveUsers", false },
                    { new Guid("019a0000-0000-7000-8000-000102000003"), 1L, new Guid("019a0000-0000-7000-8000-000100000000"), "Branches", false },
                    { new Guid("019a0000-0000-7000-8000-000102000004"), 1L, new Guid("019a0000-0000-7000-8000-000100000000"), "Warehouses", false },
                    { new Guid("019a0000-0000-7000-8000-000102000005"), 300L, new Guid("019a0000-0000-7000-8000-000100000000"), "Products", false },
                    { new Guid("019a0000-0000-7000-8000-000102000006"), 200L, new Guid("019a0000-0000-7000-8000-000100000000"), "Customers", false },
                    { new Guid("019a0000-0000-7000-8000-000102000007"), 100L, new Guid("019a0000-0000-7000-8000-000100000000"), "Prospects", false },
                    { new Guid("019a0000-0000-7000-8000-000102000008"), 100L, new Guid("019a0000-0000-7000-8000-000100000000"), "SalesDocuments", false },
                    { new Guid("019a0000-0000-7000-8000-000102000009"), 524288000L, new Guid("019a0000-0000-7000-8000-000100000000"), "FileStorageBytes", false },
                    { new Guid("019a0000-0000-7000-8000-000102000010"), 0L, new Guid("019a0000-0000-7000-8000-000100000000"), "InvoiceStamps", false },
                    { new Guid("019a0000-0000-7000-8000-000102000011"), 0L, new Guid("019a0000-0000-7000-8000-000100000000"), "BusinessEmails", false },
                    { new Guid("019a0000-0000-7000-8000-000102000012"), 0L, new Guid("019a0000-0000-7000-8000-000100000000"), "TaxDocumentStorageBytes", false },
                    { new Guid("019a0000-0000-7000-8000-000102000013"), 90L, new Guid("019a0000-0000-7000-8000-000100000000"), "AuditQueryWindowDays", false },
                    { new Guid("019a0000-0000-7000-8000-000202000001"), 1L, new Guid("019a0000-0000-7000-8000-000200000000"), "Tenants", false },
                    { new Guid("019a0000-0000-7000-8000-000202000002"), 5L, new Guid("019a0000-0000-7000-8000-000200000000"), "ActiveUsers", false },
                    { new Guid("019a0000-0000-7000-8000-000202000003"), 2L, new Guid("019a0000-0000-7000-8000-000200000000"), "Branches", false },
                    { new Guid("019a0000-0000-7000-8000-000202000004"), 3L, new Guid("019a0000-0000-7000-8000-000200000000"), "Warehouses", false },
                    { new Guid("019a0000-0000-7000-8000-000202000005"), 5000L, new Guid("019a0000-0000-7000-8000-000200000000"), "Products", false },
                    { new Guid("019a0000-0000-7000-8000-000202000006"), 5000L, new Guid("019a0000-0000-7000-8000-000200000000"), "Customers", false },
                    { new Guid("019a0000-0000-7000-8000-000202000007"), 1000L, new Guid("019a0000-0000-7000-8000-000200000000"), "Prospects", false },
                    { new Guid("019a0000-0000-7000-8000-000202000008"), 2000L, new Guid("019a0000-0000-7000-8000-000200000000"), "SalesDocuments", true },
                    { new Guid("019a0000-0000-7000-8000-000202000009"), 5368709120L, new Guid("019a0000-0000-7000-8000-000200000000"), "FileStorageBytes", false },
                    { new Guid("019a0000-0000-7000-8000-000202000010"), 100L, new Guid("019a0000-0000-7000-8000-000200000000"), "InvoiceStamps", false },
                    { new Guid("019a0000-0000-7000-8000-000202000011"), 1000L, new Guid("019a0000-0000-7000-8000-000200000000"), "BusinessEmails", false },
                    { new Guid("019a0000-0000-7000-8000-000202000012"), 2147483648L, new Guid("019a0000-0000-7000-8000-000200000000"), "TaxDocumentStorageBytes", false },
                    { new Guid("019a0000-0000-7000-8000-000202000013"), 365L, new Guid("019a0000-0000-7000-8000-000200000000"), "AuditQueryWindowDays", false },
                    { new Guid("019a0000-0000-7000-8000-000302000001"), 3L, new Guid("019a0000-0000-7000-8000-000300000000"), "Tenants", false },
                    { new Guid("019a0000-0000-7000-8000-000302000002"), 15L, new Guid("019a0000-0000-7000-8000-000300000000"), "ActiveUsers", false },
                    { new Guid("019a0000-0000-7000-8000-000302000003"), 10L, new Guid("019a0000-0000-7000-8000-000300000000"), "Branches", false },
                    { new Guid("019a0000-0000-7000-8000-000302000004"), 10L, new Guid("019a0000-0000-7000-8000-000300000000"), "Warehouses", false },
                    { new Guid("019a0000-0000-7000-8000-000302000005"), 50000L, new Guid("019a0000-0000-7000-8000-000300000000"), "Products", false },
                    { new Guid("019a0000-0000-7000-8000-000302000006"), 50000L, new Guid("019a0000-0000-7000-8000-000300000000"), "Customers", false },
                    { new Guid("019a0000-0000-7000-8000-000302000007"), null, new Guid("019a0000-0000-7000-8000-000300000000"), "Prospects", false },
                    { new Guid("019a0000-0000-7000-8000-000302000008"), 20000L, new Guid("019a0000-0000-7000-8000-000300000000"), "SalesDocuments", true },
                    { new Guid("019a0000-0000-7000-8000-000302000009"), 26843545600L, new Guid("019a0000-0000-7000-8000-000300000000"), "FileStorageBytes", false },
                    { new Guid("019a0000-0000-7000-8000-000302000010"), 500L, new Guid("019a0000-0000-7000-8000-000300000000"), "InvoiceStamps", false },
                    { new Guid("019a0000-0000-7000-8000-000302000011"), 5000L, new Guid("019a0000-0000-7000-8000-000300000000"), "BusinessEmails", false },
                    { new Guid("019a0000-0000-7000-8000-000302000012"), 10737418240L, new Guid("019a0000-0000-7000-8000-000300000000"), "TaxDocumentStorageBytes", false },
                    { new Guid("019a0000-0000-7000-8000-000302000013"), null, new Guid("019a0000-0000-7000-8000-000300000000"), "AuditQueryWindowDays", false }
                });

            migrationBuilder.CreateIndex(
                name: "IX_PlanModules_ClusterKey",
                schema: "platform",
                table: "PlanModules",
                column: "ClusterKey",
                unique: true)
                .Annotation("SqlServer:Clustered", true);

            migrationBuilder.CreateIndex(
                name: "IX_PlanModules_PlanId_Module",
                schema: "platform",
                table: "PlanModules",
                columns: new[] { "PlanId", "Module" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PlanQuotas_ClusterKey",
                schema: "platform",
                table: "PlanQuotas",
                column: "ClusterKey",
                unique: true)
                .Annotation("SqlServer:Clustered", true);

            migrationBuilder.CreateIndex(
                name: "IX_PlanQuotas_PlanId_Quota",
                schema: "platform",
                table: "PlanQuotas",
                columns: new[] { "PlanId", "Quota" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Plans_ClusterKey",
                schema: "platform",
                table: "Plans",
                column: "ClusterKey",
                unique: true)
                .Annotation("SqlServer:Clustered", true);

            migrationBuilder.CreateIndex(
                name: "IX_Plans_IsDefaultForRegistration",
                schema: "platform",
                table: "Plans",
                column: "IsDefaultForRegistration",
                unique: true,
                filter: "[IsDefaultForRegistration] = 1");

            migrationBuilder.CreateIndex(
                name: "IX_Plans_Name",
                schema: "platform",
                table: "Plans",
                column: "Name",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PlanModules",
                schema: "platform");

            migrationBuilder.DropTable(
                name: "PlanQuotas",
                schema: "platform");

            migrationBuilder.DropTable(
                name: "Plans",
                schema: "platform");
        }
    }
}
