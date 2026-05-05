
using System.ComponentModel.DataAnnotations.Schema;
using eCommerce.Domain.Interfaces;

namespace eCommerce.Domain.Entities;

public class ProductReview : ICreatedOnDate, IUpdatedOnDate, ISoftDeletable
{
    public Guid Id { get; set; }
    public Guid ProductId { get; set; }
    public Guid UserId { get; set; }
    public int Stars { get; set; }
    public string? Comment { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public bool IsDeleted { get; set; }
    public virtual Product? Product { get; set; }

}