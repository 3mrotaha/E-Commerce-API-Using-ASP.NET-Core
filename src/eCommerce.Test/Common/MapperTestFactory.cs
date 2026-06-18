using AutoMapper;
using eCommerce.Application.Mapping;

namespace eCommerce.Test.Common;

/// <summary>
/// Builds a real <see cref="IMapper"/> from the production AutoMapper profiles. Used only by the
/// mapping-profile tests, where we want to exercise the actual mapping/masking logic rather than a mock.
/// </summary>
public static class MapperTestFactory
{
    public static MapperConfiguration CreateConfiguration() =>
        new(cfg =>
        {
            cfg.AddProfile<AuthProfile>();
            cfg.AddProfile<ProductProfile>();
            cfg.AddProfile<CartProfile>();
            cfg.AddProfile<OrderProfile>();
            cfg.AddProfile<PaymentProfile>();
        });

    public static IMapper CreateMapper() => CreateConfiguration().CreateMapper();
}
