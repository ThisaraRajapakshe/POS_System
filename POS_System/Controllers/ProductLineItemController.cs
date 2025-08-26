using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using POS_System.ApplicationServices;
using POS_System.ApplicationServices.Implementation;
using POS_System.Models.Dto;
using POS_System.Models.Identity;

namespace POS_System.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]

    public class ProductLineItemController : ControllerBase
    {
        private readonly IProductLineItemService productService;

        public ProductLineItemController(IProductLineItemService productService)
        {
            this.productService = productService;
        }
        [HttpGet]
        public async Task<IActionResult> Get()
        {
            return Ok(await productService.GetProductLineItems());
        }
        [HttpGet]
        [Route("{id}")]
        public async Task<IActionResult> Get([FromRoute] string id)
        {
            var product = await productService.GetProductLineItem(id);
            if (product == null)
            {
                return NotFound();
            }
            return Ok(product);
        }
        [HttpPost]
        [Authorize (Roles = $"{RoleConstants.Admin},{RoleConstants.Manager},{RoleConstants.StockClerk}")]
        public async Task<IActionResult> Create([FromBody] CreateProductLineItemRequestDto createProductLineItemRequest)
        {
            return Ok(await productService.InsertProductLineItem(createProductLineItemRequest));
        }

        [HttpPut]
        [Route("{id}")]
        [Authorize(Roles = $"{RoleConstants.Admin},{RoleConstants.Manager},{RoleConstants.StockClerk},{RoleConstants.Accountant}")]

        public async Task<IActionResult> Update([FromRoute] string id, [FromBody] UpdateProductLineItemRequestDto updateProductLineItemRequest)
        {
            var product = await productService.UpdateProductLineItem(updateProductLineItemRequest, id);
            if (product == null)
            {
                return NotFound();
            }
            return Ok(product);
        }
        [HttpDelete]
        [Route("{id}")]
        [Authorize (Roles = $"{RoleConstants.Admin},{RoleConstants.Manager},{RoleConstants.StockClerk}")]
        public async Task<IActionResult> Delete([FromRoute] string id)
        {
            return await (productService.DeleteProductLineItem(id)) ? Ok() : NotFound();
        }
        [HttpGet("product/{productId}")]
        public async Task<IActionResult> GetLineItemByProduct([FromRoute] string productId)
        {
            var lineItem = await productService.GetLineItemByProductIdAsync(productId);
            if (lineItem == null)
            {
                return NotFound();
            }
            return Ok(lineItem);
        }
    }
}
