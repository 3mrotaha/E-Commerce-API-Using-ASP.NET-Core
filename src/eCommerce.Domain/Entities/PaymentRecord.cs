
using eCommerce.Domain.Interfaces;

namespace eCommerce.Domain.Entities;

public class PaymentRecord : ICreatedOnDate
{
    public Guid Id {get; set;}
    public Guid UserId {get; set;}
    public Guid OrderId {get; set;}
    public Guid? PaymentMethodId {get; set;}
    public decimal Amount {get; set;}
    public DateTime CreatedAt { get; set; }
    public Order? Order {get; set;}
    public PaymentMethod? PaymentMethod {get; set;}
}