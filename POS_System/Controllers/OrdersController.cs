using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
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

        public OrdersController(IOrderService orderService)
        {
            _orderService = orderService;
        }

        // GET: api/<OrdersController>
        [HttpGet]
        public async Task<IActionResult> GetOrders()
        {
            return Ok(await _orderService.GetOrdersAsync());
        }

        

        // POST api/<OrdersController>
        [HttpPost]
        public async Task<IActionResult> CreateOrder([FromBody] CreateOrderDto orderDto)
        {
            string userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            string cashierName = User.Identity.Name;

            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized("User Id Not Found in Token");
            }
            var result = await _orderService.CreateOrderAsync(orderDto, userId, cashierName);
            return Ok (result);
        }
    }
}
