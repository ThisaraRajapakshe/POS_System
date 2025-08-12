using AutoMapper;
using POS_System.Models.Domain;
using POS_System.Models.Dto;
using POS_System.Models.Identity;

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

            // User mappings
            CreateMap<ApplicationUser, RegisterDto>().ReverseMap()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.SecurityStamp, opt => opt.Ignore())
                .ForMember(dest => dest.ConcurrencyStamp, opt => opt.Ignore());

            CreateMap<ApplicationUser, LoginDto>().ReverseMap();

            // Role mappings
            CreateMap<ApplicationRole, string>()
                .ConvertUsing(src => src.Name ?? string.Empty);

            CreateMap<string, ApplicationRole>()
                .ConvertUsing(src => new ApplicationRole { Name = src });

            // Auth response mappings
            CreateMap<ApplicationUser, AuthResponseDto>()
                .ForMember(dest => dest.AccessToken, opt => opt.Ignore())
                .ForMember(dest => dest.RefreshToken, opt => opt.Ignore())
                .ForMember(dest => dest.ExpiresAt, opt => opt.Ignore())
                .ForMember(dest => dest.Roles, opt => opt.Ignore());


        }
    }
}
