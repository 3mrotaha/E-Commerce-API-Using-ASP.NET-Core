
using eCommerce.Domain.Enums;
using eCommerce.Domain.Interfaces;

namespace eCommerce.Domain.Entities;
public class Order : ICreatedOnDate
{
    public Guid Id {get; set;}
    public Guid UserId {get; set;}
    public Guid PaymentId {get; set;}
    public OrderState OrderState {get; set;}
    public bool HasDiscount {get; set;}
    public decimal? DiscountValue {get; set;}
    public virtual ICollection<OrderItem> OrderItems {get; set;} = new HashSet<OrderItem>();
    public DateTime CreatedAt { get; set; }
}