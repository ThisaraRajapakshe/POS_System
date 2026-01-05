using AutoMapper;
using POS_System.Models.Domain;
using POS_System.Models.Dto;
using POS_System.Repositories;
using POS_System.Repositories.Implementation;

namespace POS_System.ApplicationServices.Implementation
{
    public class OrderServise : IOrderService
    {
        private readonly IOrderRepository _repository;
        private readonly IMapper _mapper;

        public OrderServise(IMapper mapper,IOrderRepository repository)
        {
            _mapper = mapper;
            _repository = repository;
        }

        public async Task<OrderResponseDto> CreateOrderAsync(CreateOrderDto createOrderDto, string userId, string cashierName)
        {
            var order = _mapper.Map<Order>(createOrderDto);
            order.Id = Guid.NewGuid().ToString();
            foreach (var item in order.OrderItems)
            {
                item.OderId = order.Id;
                item.Id = Guid.NewGuid().ToString();
            }
            order.UserId = userId;
            order.CashierName = cashierName;    
            order.OrderDate = DateTime.Now;
            order.Status = createOrderDto.IsPending ? "Pending" : "Completed";
            order.OrderNumber = $"INV-{DateTime.Now:yyyyMMdd}-{Guid.NewGuid().ToString().Substring(0, 4).ToUpper()}";


            var savedOrder = await _repository.CreateOrderAsync(order);
            return _mapper.Map<OrderResponseDto>(savedOrder);
        }

        public async Task<List<OrderResponseDto>> GetOrdersAsync()
        {
            var orders = await _repository.GetAllOrdersAsync();
            return _mapper.Map<List<OrderResponseDto>>(orders);
        }
    }
}
