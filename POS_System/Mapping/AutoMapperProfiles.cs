using AutoMapper;
using POS_System.Models.Domain;
using POS_System.Models.Dto;

namespace POS_System.Mapping
{
    public class AutoMapperProfiles : Profile
    {
        public AutoMapperProfiles()
        {
            CreateMap<Product, ProductDto>().ReverseMap();
            CreateMap<Product, CreateProductRequestDto>().ReverseMap();
            CreateMap<UpdateProductRequestDto, Product>().ForMember(dest => dest.Id, opt => opt.Ignore());

            CreateMap<ProductLineItem, ProductLineItemDto>().ReverseMap();
            CreateMap<ProductLineItem, CreateProductLineItemRequestDto>().ReverseMap();
            CreateMap<UpdateProductLineItemRequestDto, ProductLineItem>().ForMember(dest => dest.Id, opt => opt.Ignore());

            CreateMap<Category, ProductDto>().ReverseMap();
            CreateMap<Category, CreateCategoryRequestDto>().ReverseMap();
            CreateMap<UpdateCategoryRequestDto, Category>().ForMember(dest => dest.Id, opt => opt.Ignore());
            CreateMap<CategoryDto, Category>().ReverseMap();


        }
    }
}
