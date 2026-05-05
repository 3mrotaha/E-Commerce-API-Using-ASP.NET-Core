using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace eCommerce.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class Initial : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "account");

            migrationBuilder.EnsureSchema(
                name: "billing");

            migrationBuilder.EnsureSchema(
                name: "purchase");

            migrationBuilder.EnsureSchema(
                name: "inventory");

            migrationBuilder.EnsureSchema(
                name: "review");

            migrationBuilder.CreateTable(
                name: "Accounts",
                schema: "account",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    FullName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()"),
                    RefreshToken = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    RefreshTokenExpiryDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UserName = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    NormalizedUserName = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    Email = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    NormalizedEmail = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    EmailConfirmed = table.Column<bool>(type: "bit", nullable: false),
                    PasswordHash = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    SecurityStamp = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ConcurrencyStamp = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PhoneNumber = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PhoneNumberConfirmed = table.Column<bool>(type: "bit", nullable: false),
                    TwoFactorEnabled = table.Column<bool>(type: "bit", nullable: false),
                    LockoutEnd = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    LockoutEnabled = table.Column<bool>(type: "bit", nullable: false),
                    AccessFailedCount = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Accounts", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AspNetRoles",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    NormalizedName = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    ConcurrencyStamp = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetRoles", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ProductCategories",
                schema: "inventory",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProductCategories", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Admins",
                schema: "account",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    IsActivated = table.Column<bool>(type: "bit", nullable: false, defaultValue: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Admins", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Admins_Accounts_Id",
                        column: x => x.Id,
                        principalSchema: "account",
                        principalTable: "Accounts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserClaims",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ClaimType = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ClaimValue = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserClaims", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AspNetUserClaims_Accounts_UserId",
                        column: x => x.UserId,
                        principalSchema: "account",
                        principalTable: "Accounts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserLogins",
                columns: table => new
                {
                    LoginProvider = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    ProviderKey = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    ProviderDisplayName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserLogins", x => new { x.LoginProvider, x.ProviderKey });
                    table.ForeignKey(
                        name: "FK_AspNetUserLogins_Accounts_UserId",
                        column: x => x.UserId,
                        principalSchema: "account",
                        principalTable: "Accounts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserTokens",
                columns: table => new
                {
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    LoginProvider = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Value = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserTokens", x => new { x.UserId, x.LoginProvider, x.Name });
                    table.ForeignKey(
                        name: "FK_AspNetUserTokens_Accounts_UserId",
                        column: x => x.UserId,
                        principalSchema: "account",
                        principalTable: "Accounts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "SuperAdmins",
                schema: "account",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SuperAdmins", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SuperAdmins_Accounts_Id",
                        column: x => x.Id,
                        principalSchema: "account",
                        principalTable: "Accounts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Users",
                schema: "account",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    IsVipUser = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    Points = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Users_Accounts_Id",
                        column: x => x.Id,
                        principalSchema: "account",
                        principalTable: "Accounts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetRoleClaims",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    RoleId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ClaimType = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ClaimValue = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetRoleClaims", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AspNetRoleClaims_AspNetRoles_RoleId",
                        column: x => x.RoleId,
                        principalTable: "AspNetRoles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserRoles",
                columns: table => new
                {
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    RoleId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserRoles", x => new { x.UserId, x.RoleId });
                    table.ForeignKey(
                        name: "FK_AspNetUserRoles_Accounts_UserId",
                        column: x => x.UserId,
                        principalSchema: "account",
                        principalTable: "Accounts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_AspNetUserRoles_AspNetRoles_RoleId",
                        column: x => x.RoleId,
                        principalTable: "AspNetRoles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Products",
                schema: "inventory",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CategoryId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    QuantityInStock = table.Column<int>(type: "int", nullable: false),
                    UnitPrice = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()"),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Products", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Products_ProductCategories_CategoryId",
                        column: x => x.CategoryId,
                        principalSchema: "inventory",
                        principalTable: "ProductCategories",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "Carts",
                schema: "purchase",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()"),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Carts", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Carts_Users_UserId",
                        column: x => x.UserId,
                        principalSchema: "account",
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Orders",
                schema: "purchase",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PaymentId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    OrderState = table.Column<int>(type: "int", nullable: false),
                    HasDiscount = table.Column<bool>(type: "bit", nullable: false),
                    DiscountValue = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Orders", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Orders_Users_UserId",
                        column: x => x.UserId,
                        principalSchema: "account",
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PaymentMethods",
                schema: "billing",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PaymentMethods", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PaymentMethods_Users_UserId",
                        column: x => x.UserId,
                        principalSchema: "account",
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ProductReviews",
                schema: "review",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ProductId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Stars = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    Comment = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()"),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()"),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProductReviews", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ProductReviews_Products_ProductId",
                        column: x => x.ProductId,
                        principalSchema: "inventory",
                        principalTable: "Products",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ProductReviews_Users_UserId",
                        column: x => x.UserId,
                        principalSchema: "account",
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "CartItems",
                schema: "purchase",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CartId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ProductId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Quantity = table.Column<int>(type: "int", nullable: false),
                    UnitPrice = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()"),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CartItems", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CartItems_Carts_CartId",
                        column: x => x.CartId,
                        principalSchema: "purchase",
                        principalTable: "Carts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CartItems_Products_ProductId",
                        column: x => x.ProductId,
                        principalSchema: "inventory",
                        principalTable: "Products",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "OrderItems",
                schema: "purchase",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    OrderId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ProductId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Quantity = table.Column<int>(type: "int", nullable: false),
                    UnitPrice = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OrderItems", x => x.Id);
                    table.ForeignKey(
                        name: "FK_OrderItems_Orders_OrderId",
                        column: x => x.OrderId,
                        principalSchema: "purchase",
                        principalTable: "Orders",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_OrderItems_Products_ProductId",
                        column: x => x.ProductId,
                        principalSchema: "inventory",
                        principalTable: "Products",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "CardPaymentMethods",
                schema: "billing",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CardNumber = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    CardExpiryDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CVV = table.Column<string>(type: "nvarchar(4)", maxLength: 4, nullable: false),
                    CardHolderName = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CardPaymentMethods", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CardPaymentMethods_PaymentMethods_Id",
                        column: x => x.Id,
                        principalSchema: "billing",
                        principalTable: "PaymentMethods",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PaymentRecords",
                schema: "billing",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    OrderId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PaymentMethodId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    Amount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PaymentRecords", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PaymentRecords_Orders_OrderId",
                        column: x => x.OrderId,
                        principalSchema: "purchase",
                        principalTable: "Orders",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_PaymentRecords_PaymentMethods_PaymentMethodId",
                        column: x => x.PaymentMethodId,
                        principalSchema: "billing",
                        principalTable: "PaymentMethods",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_PaymentRecords_Users_UserId",
                        column: x => x.UserId,
                        principalSchema: "account",
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                schema: "account",
                table: "Accounts",
                columns: new[] { "Id", "AccessFailedCount", "ConcurrencyStamp", "CreatedAt", "Email", "EmailConfirmed", "FullName", "LockoutEnabled", "LockoutEnd", "NormalizedEmail", "NormalizedUserName", "PasswordHash", "PhoneNumber", "PhoneNumberConfirmed", "RefreshToken", "RefreshTokenExpiryDate", "SecurityStamp", "TwoFactorEnabled", "UserName" },
                values: new object[,]
                {
                    { new Guid("8ad8a683-a2c1-4df2-94af-b1b4470cb6d1"), 0, "1E5EB718-66E6-4596-9091-0D76EF394295", new DateTime(2026, 4, 14, 0, 0, 0, 0, DateTimeKind.Utc), "lena.carter@example.com", true, "Lena Carter", false, null, "LENA.CARTER@EXAMPLE.COM", "LENA.CARTER", null, null, false, null, new DateTime(2026, 5, 14, 0, 0, 0, 0, DateTimeKind.Utc), "B24E9502-B337-45E0-AF6D-5F1133AE0B0A", false, "lena.carter" },
                    { new Guid("bcbd8034-46f1-4e07-9f45-4b3871572a4b"), 0, "5403C123-50E6-4292-A88F-774830F1B330", new DateTime(2026, 4, 14, 0, 0, 0, 0, DateTimeKind.Utc), "marcus.reed@example.com", true, "Marcus Reed", false, null, "MARCUS.REED@EXAMPLE.COM", "MARCUS.REED", null, null, false, null, new DateTime(2026, 5, 14, 0, 0, 0, 0, DateTimeKind.Utc), "42D1AB89-0A11-4A80-A784-0343D5C5DE4B", false, "marcus.reed" }
                });

            migrationBuilder.InsertData(
                schema: "inventory",
                table: "ProductCategories",
                columns: new[] { "Id", "Description", "IsDeleted", "Name" },
                values: new object[,]
                {
                    { new Guid("28978b4b-df88-4c5a-b371-d4c10eaebff9"), "Connected devices and premium personal tech.", false, "Electronics" },
                    { new Guid("4b4fa4f4-6905-4036-808f-c5876c469bb1"), "Workout essentials and recovery-focused gear.", false, "Fitness" },
                    { new Guid("c6f81854-315d-4676-a221-7b578d9d0f44"), "Desk setups and accessories built for all-day comfort.", false, "Home Office" }
                });

            migrationBuilder.InsertData(
                schema: "inventory",
                table: "Products",
                columns: new[] { "Id", "CategoryId", "CreatedAt", "Description", "IsDeleted", "Name", "QuantityInStock", "UnitPrice" },
                values: new object[,]
                {
                    { new Guid("2dd712d1-7cee-4b47-987a-446eb1907f6a"), new Guid("4b4fa4f4-6905-4036-808f-c5876c469bb1"), new DateTime(2026, 4, 14, 0, 0, 0, 0, DateTimeKind.Utc), "Non-slip extra-thick yoga mat designed for home workouts and studio use.", false, "Balance Grip Yoga Mat", 77, 39.95m },
                    { new Guid("6ea52fd5-f353-42d0-af5e-e09a7997fa8a"), new Guid("4b4fa4f4-6905-4036-808f-c5876c469bb1"), new DateTime(2026, 4, 14, 0, 0, 0, 0, DateTimeKind.Utc), "Fitness smartwatch with GPS, sleep tracking, and heart-rate alerts.", false, "Stride Pro Smart Watch", 34, 199.00m },
                    { new Guid("7593a8b6-c0ae-4b0b-b350-37deaf436fc2"), new Guid("c6f81854-315d-4676-a221-7b578d9d0f44"), new DateTime(2026, 4, 14, 0, 0, 0, 0, DateTimeKind.Utc), "Electric standing desk with programmable height presets and cable tray.", false, "Rise Standing Desk", 12, 599.99m },
                    { new Guid("bc7f9c55-ad74-448f-870b-13b8b82f501c"), new Guid("c6f81854-315d-4676-a221-7b578d9d0f44"), new DateTime(2026, 4, 14, 0, 0, 0, 0, DateTimeKind.Utc), "Mesh ergonomic chair with adjustable lumbar support and 4D armrests.", false, "Contour Ergonomic Chair", 15, 389.00m },
                    { new Guid("db6f5fd4-bf4e-44b9-88b9-6df6cfd0a13e"), new Guid("28978b4b-df88-4c5a-b371-d4c10eaebff9"), new DateTime(2026, 4, 14, 0, 0, 0, 0, DateTimeKind.Utc), "Low-profile mechanical keyboard with hot-swappable switches and white backlight.", false, "Pulse Mechanical Keyboard", 68, 129.50m },
                    { new Guid("fb7a7720-c40a-4eef-bb5e-9a592b6e00c5"), new Guid("28978b4b-df88-4c5a-b371-d4c10eaebff9"), new DateTime(2026, 4, 14, 0, 0, 0, 0, DateTimeKind.Utc), "Wireless over-ear headphones with adaptive ANC and 35-hour battery life.", false, "Auralux Noise Cancelling Headphones", 42, 249.99m }
                });

            migrationBuilder.InsertData(
                schema: "account",
                table: "Users",
                columns: new[] { "Id", "IsVipUser", "Points" },
                values: new object[] { new Guid("8ad8a683-a2c1-4df2-94af-b1b4470cb6d1"), true, 240 });

            migrationBuilder.InsertData(
                schema: "account",
                table: "Users",
                columns: new[] { "Id", "Points" },
                values: new object[] { new Guid("bcbd8034-46f1-4e07-9f45-4b3871572a4b"), 95 });

            migrationBuilder.InsertData(
                schema: "review",
                table: "ProductReviews",
                columns: new[] { "Id", "Comment", "CreatedAt", "IsDeleted", "ProductId", "Stars", "UserId" },
                values: new object[,]
                {
                    { new Guid("01112e2f-7d32-4cb5-92b4-e0b5244620fe"), "Great grip on hardwood floors and enough padding for longer mobility sessions.", new DateTime(2026, 4, 14, 0, 0, 0, 0, DateTimeKind.Utc), false, new Guid("2dd712d1-7cee-4b47-987a-446eb1907f6a"), 5, new Guid("bcbd8034-46f1-4e07-9f45-4b3871572a4b") },
                    { new Guid("08b76ca9-3af4-4dfd-b6d7-4cd7993eef40"), "Tactile without being too loud. It feels premium and works well for both coding and writing.", new DateTime(2026, 4, 14, 0, 0, 0, 0, DateTimeKind.Utc), false, new Guid("db6f5fd4-bf4e-44b9-88b9-6df6cfd0a13e"), 5, new Guid("8ad8a683-a2c1-4df2-94af-b1b4470cb6d1") },
                    { new Guid("82eb6ca2-e910-4f2c-ab2d-504c5b71501e"), "Accurate activity tracking and a bright display. The companion app could be a little faster.", new DateTime(2026, 4, 14, 0, 0, 0, 0, DateTimeKind.Utc), false, new Guid("6ea52fd5-f353-42d0-af5e-e09a7997fa8a"), 4, new Guid("8ad8a683-a2c1-4df2-94af-b1b4470cb6d1") },
                    { new Guid("99a30d2c-9cfb-42cb-8ea0-edc0d5cb3af8"), "Excellent noise isolation for commuting and the battery easily lasts a full work week.", new DateTime(2026, 4, 14, 0, 0, 0, 0, DateTimeKind.Utc), false, new Guid("fb7a7720-c40a-4eef-bb5e-9a592b6e00c5"), 5, new Guid("8ad8a683-a2c1-4df2-94af-b1b4470cb6d1") },
                    { new Guid("cf013e87-4588-4230-aad2-6ed6920221da"), "Assembly took a bit of time, but the lumbar support made a noticeable difference after a few days.", new DateTime(2026, 4, 14, 0, 0, 0, 0, DateTimeKind.Utc), false, new Guid("bc7f9c55-ad74-448f-870b-13b8b82f501c"), 4, new Guid("bcbd8034-46f1-4e07-9f45-4b3871572a4b") },
                    { new Guid("eb674a0a-cf6a-4d3a-991c-fc95fbc0f944"), "Comfortable fit and clean sound profile, though the carry case is bulkier than expected.", new DateTime(2026, 4, 14, 0, 0, 0, 0, DateTimeKind.Utc), false, new Guid("fb7a7720-c40a-4eef-bb5e-9a592b6e00c5"), 4, new Guid("bcbd8034-46f1-4e07-9f45-4b3871572a4b") }
                });

            migrationBuilder.CreateIndex(
                name: "EmailIndex",
                schema: "account",
                table: "Accounts",
                column: "NormalizedEmail");

            migrationBuilder.CreateIndex(
                name: "UserNameIndex",
                schema: "account",
                table: "Accounts",
                column: "NormalizedUserName",
                unique: true,
                filter: "[NormalizedUserName] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetRoleClaims_RoleId",
                table: "AspNetRoleClaims",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "RoleNameIndex",
                table: "AspNetRoles",
                column: "NormalizedName",
                unique: true,
                filter: "[NormalizedName] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUserClaims_UserId",
                table: "AspNetUserClaims",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUserLogins_UserId",
                table: "AspNetUserLogins",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUserRoles_RoleId",
                table: "AspNetUserRoles",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "IX_CartItems_CartId",
                schema: "purchase",
                table: "CartItems",
                column: "CartId");

            migrationBuilder.CreateIndex(
                name: "IX_CartItems_ProductId",
                schema: "purchase",
                table: "CartItems",
                column: "ProductId");

            migrationBuilder.CreateIndex(
                name: "IX_Carts_UserId",
                schema: "purchase",
                table: "Carts",
                column: "UserId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_OrderItems_OrderId",
                schema: "purchase",
                table: "OrderItems",
                column: "OrderId");

            migrationBuilder.CreateIndex(
                name: "IX_OrderItems_ProductId",
                schema: "purchase",
                table: "OrderItems",
                column: "ProductId");

            migrationBuilder.CreateIndex(
                name: "IX_Orders_UserId",
                schema: "purchase",
                table: "Orders",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_PaymentMethods_UserId",
                schema: "billing",
                table: "PaymentMethods",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_PaymentRecords_OrderId",
                schema: "billing",
                table: "PaymentRecords",
                column: "OrderId");

            migrationBuilder.CreateIndex(
                name: "IX_PaymentRecords_PaymentMethodId",
                schema: "billing",
                table: "PaymentRecords",
                column: "PaymentMethodId");

            migrationBuilder.CreateIndex(
                name: "IX_PaymentRecords_UserId",
                schema: "billing",
                table: "PaymentRecords",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_ProductReviews_ProductId",
                schema: "review",
                table: "ProductReviews",
                column: "ProductId");

            migrationBuilder.CreateIndex(
                name: "IX_ProductReviews_UserId",
                schema: "review",
                table: "ProductReviews",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_Products_CategoryId",
                schema: "inventory",
                table: "Products",
                column: "CategoryId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Admins",
                schema: "account");

            migrationBuilder.DropTable(
                name: "AspNetRoleClaims");

            migrationBuilder.DropTable(
                name: "AspNetUserClaims");

            migrationBuilder.DropTable(
                name: "AspNetUserLogins");

            migrationBuilder.DropTable(
                name: "AspNetUserRoles");

            migrationBuilder.DropTable(
                name: "AspNetUserTokens");

            migrationBuilder.DropTable(
                name: "CardPaymentMethods",
                schema: "billing");

            migrationBuilder.DropTable(
                name: "CartItems",
                schema: "purchase");

            migrationBuilder.DropTable(
                name: "OrderItems",
                schema: "purchase");

            migrationBuilder.DropTable(
                name: "PaymentRecords",
                schema: "billing");

            migrationBuilder.DropTable(
                name: "ProductReviews",
                schema: "review");

            migrationBuilder.DropTable(
                name: "SuperAdmins",
                schema: "account");

            migrationBuilder.DropTable(
                name: "AspNetRoles");

            migrationBuilder.DropTable(
                name: "Carts",
                schema: "purchase");

            migrationBuilder.DropTable(
                name: "Orders",
                schema: "purchase");

            migrationBuilder.DropTable(
                name: "PaymentMethods",
                schema: "billing");

            migrationBuilder.DropTable(
                name: "Products",
                schema: "inventory");

            migrationBuilder.DropTable(
                name: "Users",
                schema: "account");

            migrationBuilder.DropTable(
                name: "ProductCategories",
                schema: "inventory");

            migrationBuilder.DropTable(
                name: "Accounts",
                schema: "account");
        }
    }
}
