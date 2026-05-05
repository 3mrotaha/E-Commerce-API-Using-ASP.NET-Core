using AutoMapper;
using eCommerce.Application.DTOs.Product;
using eCommerce.Application.DTOs.ProductCategory;
using eCommerce.Application.DTOs.ProductReview;
using eCommerce.Domain.Entities;

namespace eCommerce.Application.Mapping;

public class ProductProfile : Profile
{
    public ProductProfile()
    {
        CreateMap<Product, ProductResponseDto>()
            .ForMember(dest => dest.CategoryName,
                       opt => opt.MapFrom(src => src.Category != null ? src.Category.Name : string.Empty));   

        CreateMap<CreateProductDto, Product>();
        CreateMap<UpdateProductDto, Product>();
        
        CreateMap<ProductCategory, ProductCategoryResponseDto>();
        CreateMap<CreateProductCategoryDto, ProductCategory>();
        CreateMap<UpdateProductCategoryDto, ProductCategory>();

        CreateMap<ProductReview, ProductReviewResponseDto>();
        CreateMap<CreateProductReviewDto, ProductReview>();
        CreateMap<UpdateProductReviewDto, ProductReview>();
    }
}
