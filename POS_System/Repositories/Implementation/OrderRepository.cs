using Microsoft.EntityFrameworkCore;
using POS_System.Data;
using POS_System.Models.Domain;

namespace POS_System.Repositories.Implementation
{
    public class OrderRepository : IOrderRepository
    {
        private readonly PosSystemDbContext _context;

        public OrderRepository(PosSystemDbContext context)
        {
            _context = context;
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
                        throw new Exception($"Not enough stock for {productInDb.Id}. Only Available {productInDb.Quantity}");
                    }
                    //  3. Reduce Stock
                    productInDb.Quantity -= item.Quantity;

                }
                await _context.SaveChangesAsync();
                await transaction.CommitAsync();
                return order;
            }
            catch (Exception)
            {
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
