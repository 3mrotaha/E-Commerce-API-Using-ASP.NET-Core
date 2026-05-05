using eCommerce.Domain.Entities;
using eCommerce.Domain.Identity;
using eCommerce.Persistence.Configurations;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace eCommerce.Persistence.Data;


public class AppDbContext : IdentityDbContext<Account, ApplicationRole, Guid>
{
    public DbSet<Account> Accounts { get; set; }
    public DbSet<Order> Orders { get; set; }
    public DbSet<Cart> Carts { get; set; }
    public DbSet<PaymentMethod> PaymentMethods { get; set; }
    public DbSet<ProductReview> ProductReviews { get; set; }
    public DbSet<OrderItem> OrderItems { get; set; }
    public DbSet<CartItem> CartItems { get; set; }
    public DbSet<PaymentRecord> PaymentRecords { get; set; }
    public DbSet<Product> Products { get; set; }
    public DbSet<ProductCategory> ProductCategories { get; set; }

    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {

    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder); // required for Identity schema

        modelBuilder.Entity<User>().HasData(AppDbSeedData.Users.ToArray());

        new AccountEntityConfiguration("account", "Accounts").Configure(modelBuilder.Entity<Account>());
        new UserEntityConfiguration("account", "Users").Configure(modelBuilder.Entity<User>());
        new AdminEntityConfiguration("account", "Admins").Configure(modelBuilder.Entity<Admin>());
        new SuperAdminEntityConfiguration("account", "SuperAdmins").Configure(modelBuilder.Entity<SuperAdmin>());

        new OrderEntityConfiguration("purchase", "Orders").Configure(modelBuilder.Entity<Order>());
        new OrderItemEntityConfiguration("purchase", "OrderItems").Configure(modelBuilder.Entity<OrderItem>());
        new CartEntityConfiguration("purchase", "Carts").Configure(modelBuilder.Entity<Cart>());
        new CartItemEntityConfiguration("purchase", "CartItems").Configure(modelBuilder.Entity<CartItem>());

        new PaymentMethodEntityConfiguration("billing", "PaymentMethods").Configure(modelBuilder.Entity<PaymentMethod>());
        new CardPaymentMethodEntityConfiguration("billing", "CardPaymentMethods").Configure(modelBuilder.Entity<CardPaymentMethod>());
        new PaymentRecordEntityConfiguration("billing", "PaymentRecords").Configure(modelBuilder.Entity<PaymentRecord>());

        new ProductEntityConfiguration("inventory", "Products").Configure(modelBuilder.Entity<Product>());
        new ProductCategoryEntityConfiguration("inventory", "ProductCategories").Configure(modelBuilder.Entity<ProductCategory>());
        
        modelBuilder.Entity<ProductCategory>().HasData(AppDbSeedData.ProductCategories.ToArray());
        modelBuilder.Entity<Product>().HasData(AppDbSeedData.Products.ToArray());

        new ProductReviewEntityConfiguration("review", "ProductReviews").Configure(modelBuilder.Entity<ProductReview>());
        modelBuilder.Entity<ProductReview>().HasData(AppDbSeedData.ProductReviews.ToArray());
    }
}
