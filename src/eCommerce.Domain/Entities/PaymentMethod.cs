
using eCommerce.Domain.Enums;
using eCommerce.Domain.Interfaces;

namespace eCommerce.Domain.Entities;

public abstract class PaymentMethod : IUpdatedOnDate
{
    public Guid Id {get; set;}
    public Guid UserId {get; set;}
    public DateTime UpdatedAt { get; set; }
}