using AutoMapper;
using XProducts.API.DTOs;
using XProducts.Core.Entities;

namespace XProducts.API.Mapping
{
    public class ProductMappingProfile : Profile
    {
        public ProductMappingProfile()
        {
            CreateMap<ProductCreateDto, Product>();
            CreateMap<ProductUpdateDto, Product>();
            //CreateMap<Product, ProductResponseDto>(); 
        }
    }
}
