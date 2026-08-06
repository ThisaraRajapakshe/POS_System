using AutoMapper;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using POS_System.Models.Domain;
using POS_System.Models.Dto;
using POS_System.Repositories;

namespace POS_System.ApplicationServices.Implementation
{
    public class OrderServise : IOrderService
    {
        private readonly IOrderRepository _repository;
        private readonly IProductLineItemRepository _productLineItemRepository;
        private readonly IMapper _mapper;
        private readonly ILogger<OrderServise> logger;

        public OrderServise(
            IMapper mapper,
            IOrderRepository repository,
            IProductLineItemRepository productLineItemRepository,
            ILogger<OrderServise>? logger = null)
        {
            _mapper = mapper;
            _repository = repository;
            _productLineItemRepository = productLineItemRepository;
            this.logger = logger ?? NullLogger<OrderServise>.Instance;
        }

        public async Task<OrderResponseDto> CreateOrderAsync(CreateOrderDto createOrderDto, string userId, string cashierName)
        {
            try
            {
                // 1. Validate inputs
                if (string.IsNullOrWhiteSpace(userId))
                    throw new ArgumentException("UserId must be provided", nameof(userId));

                // 2. Map DTO to domain entity
                var order = _mapper.Map<Order>(createOrderDto);

                // 3. Initialize order (assign ids, timestamps, order number, status)
                order.InitializeForCreate(userId, cashierName, createOrderDto.IsPending);

                // 4. Populate order items from inventory and validate stock
                await PopulateOrderItemsAsync(order);

                // 5. Calculate subtotals and total
                CalculateOrderTotals(order);

                // 6. Persist order (repository handles transaction and stock reduction)
                var savedOrder = await _repository.CreateOrderAsync(order);

                logger.LogInformation("Order {OrderNumber} created successfully by user {UserId}", savedOrder.OrderNumber, userId);

                // 7. Map to response DTO
                return _mapper.Map<OrderResponseDto>(savedOrder);
            }
            catch (ArgumentException aex)
            {
                logger.LogWarning(aex, "Validation error creating order for user {UserId}", userId);
                throw;
            }
            catch (InvalidOperationException iex)
            {
                logger.LogWarning(iex, "Business logic error creating order for user {UserId}", userId);
                throw;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Unexpected error creating order for user {UserId}", userId);
                throw;
            }
        }

        public async Task<List<OrderResponseDto>> GetOrdersAsync()
        {
            try
            {
                var orders = await _repository.GetAllOrdersAsync();
                return _mapper.Map<List<OrderResponseDto>>(orders);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error getting orders");
                throw;
            }
        }

        /// <summary>
        /// Populate order items with product information from inventory and validate stock availability.
        /// This method executes application-level orchestration before persistence.
        /// </summary>
        private async Task PopulateOrderItemsAsync(Order order)
        {
            // Validate order items exist
            order.ValidateOrderItems();

            foreach (var item in order.OrderItems)
            {
                // Fetch product line item from inventory
                var productLineItem = await _productLineItemRepository.GetByIdWithNavPropsAsync(item.ProductLineItemId);

                if (productLineItem == null)
                {
                    logger.LogWarning("Product line item {ProductLineItemId} not found", item.ProductLineItemId);
                    throw new InvalidOperationException($"Product {item.ProductLineItemId} not found in inventory.");
                }

                // Check stock availability BEFORE adding to order
                if (productLineItem.Quantity < item.Quantity)
                {
                    logger.LogWarning(
                        "Insufficient stock for product {ProductLineItemId}. Available: {Available}, Requested: {Requested}",
                        productLineItem.Id,
                        productLineItem.Quantity,
                        item.Quantity);
                    throw new InvalidOperationException(
                        $"Not enough stock for product {productLineItem.BarCodeId}. Available: {productLineItem.Quantity}, Requested: {item.Quantity}");
                }

                // Populate order item with product snapshot data
                item.ProductName = productLineItem.Product?.Name ?? "Unknown Product";
                item.Cost = productLineItem.Cost;
                item.DisplayPrice = productLineItem.DisplayPrice;
            }
        }

        /// <summary>
        /// Calculate subtotals for each order item and the total order amount.
        /// </summary>
        private void CalculateOrderTotals(Order order)
        {
            if (order.OrderItems == null || order.OrderItems.Count == 0)
            {
                order.TotalAmount = 0;
                return;
            }

            decimal total = 0;
            foreach (var item in order.OrderItems)
            {
                item.CalculateSubTotal();
                total += item.SubTotal;
            }

            order.TotalAmount = total;
        }
    }
}
