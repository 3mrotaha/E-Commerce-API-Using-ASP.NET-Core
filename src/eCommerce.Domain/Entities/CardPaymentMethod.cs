
namespace eCommerce.Domain.Entities;

public class CardPaymentMethod : PaymentMethod
{
    public string CardNumber {get; set;} = null!;
    public DateTime CardExpiryDate {get; set;}
    public string CVV {get; set;} = null!;
    public string CardHolderName { get; set; } = null!;
}