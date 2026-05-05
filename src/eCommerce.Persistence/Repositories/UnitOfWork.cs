using eCommerce.Domain.Interfaces;

namespace eCommerce.Persistence.Repositories;

public class UnitOfWork : IUnitOfWork
{
    public IUserRepository Users { get; }

    public IAdminRepository Admins { get; }

    public ISuperAdminRepository SuperAdmins { get; }

    public ICartRepository Carts { get; }

    public ICartItemRepository CartItems { get; }

    public IProductCategoryRepository Categories { get; }

    public IProductRepository Products { get; }

    public IProductReviewRepository ProductReviews { get; }

    public IOrderRepository Orders { get; }

    public IOrderItemRepository OrderItems { get; }

    public ICardPaymentMethodRepository CardPaymentMethods { get; }

    public IPaymentRecordRepository PaymentRecords { get; }

    public UnitOfWork(IUserRepository userRepository, IAdminRepository adminRepository, ISuperAdminRepository superAdminRepository, ICartRepository cartRepository, ICartItemRepository cartItemRepository, IProductCategoryRepository categoryRepository, IProductRepository productRepository, IProductReviewRepository productReviewRepository, IOrderRepository orderRepository, IOrderItemRepository orderItemRepository, ICardPaymentMethodRepository cardPaymentMethodRepository, IPaymentRecordRepository paymentRecordRepository)
    {
        Users = userRepository;
        Admins = adminRepository;
        SuperAdmins = superAdminRepository;
        Carts = cartRepository;
        CartItems = cartItemRepository;
        Categories = categoryRepository;
        Products = productRepository;
        ProductReviews = productReviewRepository;
        Orders = orderRepository;
        OrderItems = orderItemRepository;
        CardPaymentMethods = cardPaymentMethodRepository;
        PaymentRecords = paymentRecordRepository;
    }
}
