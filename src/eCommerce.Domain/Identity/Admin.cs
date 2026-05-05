using eCommerce.Domain.Entities;

namespace eCommerce.Domain.Identity;

public class Admin : Account
{
    public bool IsActivated {get; set;}
}