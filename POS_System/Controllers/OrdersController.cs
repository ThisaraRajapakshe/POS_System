using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using POS_System.ApplicationServices;
using POS_System.Models.Dto;
using System.Security.Claims;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace POS_System.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public class OrdersController : ControllerBase
    {
        private readonly IOrderService _orderService;
        private readonly ILogger<OrdersController> logger;

        public OrdersController(IOrderService orderService, ILogger<OrdersController>? logger = null)
        {
            _orderService = orderService;
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        // GET: api/<OrdersController>
        [HttpGet]
        public async Task<IActionResult> GetOrders()
        {
            try
            {
                var orders = await _orderService.GetOrdersAsync();
                return Ok(orders);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error retrieving orders");
                return StatusCode(StatusCodes.Status500InternalServerError, new { message = "An error occurred while retrieving orders" });
            }
        }

        // POST api/<OrdersController>
        [HttpPost]
        public async Task<IActionResult> CreateOrder([FromBody] CreateOrderDto orderDto)
        {
            try
            {
                string userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                string cashierName = User.Identity.Name;

                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized("User Id Not Found in Token");
                }

                var result = await _orderService.CreateOrderAsync(orderDto, userId, cashierName);
                return Ok(result);
            }
            catch (ArgumentException aex)
            {
                logger.LogWarning(aex, "Validation error in CreateOrder");
                return BadRequest(new { message = aex.Message });
            }
            catch (InvalidOperationException iex)
            {
                logger.LogWarning(iex, "Business logic error in CreateOrder");
                return BadRequest(new { message = iex.Message });
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Unexpected error in CreateOrder");
                return StatusCode(StatusCodes.Status500InternalServerError, new { message = "An error occurred while creating the order" });
            }
        }
    }
}
