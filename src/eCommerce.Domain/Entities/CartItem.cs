
using System.ComponentModel.DataAnnotations.Schema;
using eCommerce.Domain.Interfaces;


namespace eCommerce.Domain.Entities;

public class CartItem : IUpdatedOnDate, ISoftDeletable
{
    public int Id {get; set;}
    public Guid CartId {get; set;}
    public Guid ProductId {get; set;}
    public int Quantity {get; set;}
    public decimal UnitPrice {get; set;}
    public DateTime UpdatedAt { get; set; }
    public bool IsDeleted { get; set; }
    public virtual Cart? Cart {get; set;}
    public virtual Product? Product {get; set;}

    [NotMapped]
    public decimal TotalPrice {get => Quantity * UnitPrice; }
}