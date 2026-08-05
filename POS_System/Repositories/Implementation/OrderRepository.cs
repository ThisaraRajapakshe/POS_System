using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using POS_System.Data;
using POS_System.Models.Domain;

namespace POS_System.Repositories.Implementation
{
    public class OrderRepository : IOrderRepository
    {
        private readonly PosSystemDbContext _context;
        private readonly ILogger<OrderRepository> logger;

        public OrderRepository(PosSystemDbContext context, ILogger<OrderRepository>? logger = null)
        {
            _context = context;
            this.logger = logger ?? NullLogger<OrderRepository>.Instance;
        }

        /// <summary>
        /// Persist order and reduce inventory within a transaction.
        /// Assumes order is already validated and populated by the service layer.
        /// </summary>
        public async Task<Order> CreateOrderAsync(Order order)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                // Add order (order items added via navigation property)
                await _context.Orders.AddAsync(order);

                // Reduce inventory for each order item
                foreach (var item in order.OrderItems)
                {
                    var productInDb = await _context.ProductLineItems.FirstOrDefaultAsync(x => x.Id == item.ProductLineItemId);
                    if (productInDb == null)
                    {
                        logger.LogWarning("Product {ProductLineItemId} not found in inventory", item.ProductLineItemId);
                        throw new InvalidOperationException($"Product {item.ProductLineItemId} not found in inventory.");
                    }

                    // Call domain method to reduce stock (applies validation)
                    productInDb.ReduceStock(item.Quantity);
                }

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                logger.LogInformation("Order {OrderId} created successfully with {ItemCount} items", order.Id, order.OrderItems?.Count ?? 0);
                return order;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error creating order {OrderId}. Transaction rolled back.", order?.Id);
                await transaction.RollbackAsync();
                throw;
            }
        }

        public async Task<Order> FindByIdAsync(string id)
        {
            return await _context.Orders.FirstOrDefaultAsync(x => x.Id == id);
        }

        public async Task<List<Order>> GetAllOrdersAsync()
        {
            return await _context.Orders
                .Include(x => x.OrderItems)
                .OrderByDescending(x => x.OrderDate)
                .ToListAsync();
        }
    }
}
