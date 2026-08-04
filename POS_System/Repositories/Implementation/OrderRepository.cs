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

        public async Task<Order> CreateOrderAsync(Order order)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                order.TotalAmount = 0;
                await _context.Orders.AddAsync(order);

                foreach (var item in order.OrderItems)
                {
                    //  1. Find Product in inventory
                    var productInDb = await _context.ProductLineItems
                        .Include(x => x.Product)
                        .FirstOrDefaultAsync(x => x.Id == item.ProductLineItemId);
                    if (productInDb == null)
                    {
                        logger.LogWarning("Product {ProductLineItemId} not found", item.ProductLineItemId);
                        throw new Exception($"Product {item.ProductLineItemId} not found.");
                    }
                    item.ProductName = productInDb.Product.Name;

                    item.Cost = productInDb.Cost;
                    item.DisplayPrice = productInDb.DisplayPrice;
                    item.SubTotal = item.SalesPrice * item.Quantity;
                    order.TotalAmount += item.SubTotal;
                    //  2. Check if we have enough stock
                    if (productInDb.Quantity < item.Quantity)
                    {
                        logger.LogWarning("Not enough stock for {ProductLineItemId}. Available: {Available}", productInDb.Id, productInDb.Quantity);
                        throw new Exception($"Not enough stock for {productInDb.Id}. Only Available {productInDb.Quantity}");
                    }
                    //  3. Reduce Stock
                    productInDb.Quantity -= item.Quantity;

                }
                await _context.SaveChangesAsync();
                await transaction.CommitAsync();
                return order;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error creating order {OrderId}", order?.Id);
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
            return await _context.Orders.Include( x=> x.OrderItems ).OrderByDescending(x => x.OrderDate).ToListAsync();
        }
    }
}
