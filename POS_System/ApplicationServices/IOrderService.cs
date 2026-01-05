using POS_System.Models.Dto;

namespace POS_System.ApplicationServices
{
    public interface IOrderService
    {
        Task<OrderResponseDto> CreateOrderAsync(CreateOrderDto createOrderDto, string userId, string cashierName);
        Task<List<OrderResponseDto>> GetOrdersAsync();
    }
}
