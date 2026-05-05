
using eCommerce.Domain.Entities;

namespace eCommerce.Domain.Identity;
public class User : Account
{
    public bool IsVipUser {get; set;}
    public int Points {get; set;}
    public virtual Cart? Cart {get; set;}
    public virtual ICollection<Order>? Orders {get; set;} = new HashSet<Order>();
    public virtual ICollection<PaymentMethod>? PaymentMethods {get; set;} = new HashSet<PaymentMethod>();
    public virtual ICollection<ProductReview>? ProductReviews {get; set;} = new HashSet<ProductReview>();
    public virtual ICollection<PaymentRecord>? PaymentRecords {get; set;} = new HashSet<PaymentRecord>();
}