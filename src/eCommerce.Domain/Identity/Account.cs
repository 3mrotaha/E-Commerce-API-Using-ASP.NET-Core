using eCommerce.Domain.Enums;
using eCommerce.Domain.Interfaces;
using Microsoft.AspNetCore.Identity;

namespace eCommerce.Domain.Identity;

public abstract class Account : IdentityUser<Guid>, ICreatedOnDate
{
    public string FullName { get; set; } = null!;
    public DateTime CreatedAt { get; set; }
    public string? RefreshToken {get; set;}
    public DateTime RefreshTokenExpiryDate {get; set;}
}