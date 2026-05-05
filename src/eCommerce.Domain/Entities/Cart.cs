
using eCommerce.Domain.Interfaces;

namespace eCommerce.Domain.Entities;
public class Cart : ICreatedOnDate, IUpdatedOnDate
{
    public Guid Id {get; set;}
    public Guid UserId {get; set;}    
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt {get; set;}
    public virtual ICollection<CartItem>? CartItems {get; set;} = new HashSet<CartItem>();
}