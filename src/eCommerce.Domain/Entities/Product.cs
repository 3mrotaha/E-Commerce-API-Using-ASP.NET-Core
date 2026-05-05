
using System.ComponentModel.DataAnnotations.Schema;
using eCommerce.Domain.Interfaces;

namespace eCommerce.Domain.Entities;

// [Table("Products", Schema = "inventory")]
public class Product : ICreatedOnDate, IUpdatedOnDate, ISoftDeletable
{
    public Guid Id { get; set; }
    public Guid? CategoryId { get; set; }
    public string Name { get; set; } = null!;
    public string? Description { get; set; }
    public int QuantityInStock { get; set; }
    public decimal UnitPrice { get; set; }
    public bool IsDeleted { get; set; }
    public DateTime UpdatedAt { get; set; }
    public DateTime CreatedAt { get; set; }
    public virtual ProductCategory? Category { get; set; }
    public virtual ICollection<CartItem>? CartItems { get; set; }
    public virtual ICollection<OrderItem>? OrderItems { get; set; }
    public virtual ICollection<ProductReview>? ProductReviews { get; set; }
}