
using System.ComponentModel.DataAnnotations.Schema;
using eCommerce.Domain.Interfaces;

namespace eCommerce.Domain.Entities;

// [Table("ProductCategories", Schema = "inventory")]
public class ProductCategory : ISoftDeletable
{
    public Guid Id {get; set;}
    public string Name {get; set;} = null!;
    public string? Description {get; set;}
    public bool IsDeleted { get; set; }
    public virtual ICollection<Product> Products {get; set;} = new HashSet<Product>();
}