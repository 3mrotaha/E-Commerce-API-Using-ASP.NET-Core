using System.ComponentModel.DataAnnotations.Schema;
using eCommerce.Domain.Interfaces;

namespace eCommerce.Domain.Entities;

public class OrderItem : ISoftDeletable
{
    public int Id { get; set; }
    public Guid UserId { get; set; }
    public Guid OrderId { get; set; }
    public Guid ProductId { get; set; }
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public bool IsDeleted { get; set; }
    public virtual Product? Product { get; set; }

    [NotMapped]
    public decimal TotalPrice { get => Quantity * UnitPrice; }
}