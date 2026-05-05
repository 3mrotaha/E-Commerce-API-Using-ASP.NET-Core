namespace eCommerce.Domain.Interfaces;

public interface IUnitOfWork
{
    IUserRepository Users { get; }
    IAdminRepository Admins { get; }
    ISuperAdminRepository SuperAdmins { get; }
    ICartRepository Carts { get; }
    ICartItemRepository CartItems { get; }
    IProductCategoryRepository Categories { get; }
    IProductRepository Products { get; }
    IProductReviewRepository ProductReviews { get; }
    IOrderRepository Orders { get; }
    IOrderItemRepository OrderItems { get; }
    ICardPaymentMethodRepository CardPaymentMethods { get; }
    IPaymentRecordRepository PaymentRecords { get; }
}
