using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using POS_System.ApplicationServices;
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
        private readonly ILogger<ProductLineItemController> logger;

        public ProductLineItemController(IProductLineItemService productService, ILogger<ProductLineItemController>? logger = null)
        {
            this.productService = productService;
            this.logger = logger ?? Microsoft.Extensions.Logging.Abstractions.NullLogger<ProductLineItemController>.Instance;
        }
        [HttpGet]
        public async Task<IActionResult> Get()
        {
            try
            {
                return Ok(await productService.GetProductLineItems());
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error getting product line items");
                return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while getting product line items.");
            }
        }
        [HttpGet]
        [Route("{id}")]
        public async Task<IActionResult> Get([FromRoute] string id)
        {
            try
            {
                var product = await productService.GetProductLineItem(id);
                if (product == null)
                {
                    return NotFound();
                }
                return Ok(product);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error getting product line item {Id}", id);
                return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while getting the product line item.");
            }
        }
        [HttpPost]
        [Authorize (Roles = $"{RoleConstants.Admin},{RoleConstants.Manager},{RoleConstants.StockClerk}")]
        public async Task<IActionResult> Create([FromBody] CreateProductLineItemRequestDto createProductLineItemRequest)
        {
            try
            {
                var result = await productService.InsertProductLineItem(createProductLineItemRequest);
                return Ok(result);
            }
            catch (ArgumentException ex)
            {
                logger.LogWarning(ex, "Validation error creating product line item: {Request}", createProductLineItemRequest);
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error creating product line item");
                return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while creating the product line item.");
            }
        }

        [HttpPut]
        [Route("{id}")]
        [Authorize(Roles = $"{RoleConstants.Admin},{RoleConstants.Manager},{RoleConstants.StockClerk},{RoleConstants.Accountant}")]

        public async Task<IActionResult> Update([FromRoute] string id, [FromBody] UpdateProductLineItemRequestDto updateProductLineItemRequest)
        {
            try
            {
                var product = await productService.UpdateProductLineItem(updateProductLineItemRequest, id);
                if (product == null)
                {
                    return NotFound();
                }
                return Ok(product);
            }
            catch (ArgumentException ex)
            {
                logger.LogWarning(ex, "Validation error updating product line item {Id}: {Request}", id, updateProductLineItemRequest);
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error updating product line item {Id}", id);
                return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while updating the product line item.");
            }
        }
        [HttpDelete]
        [Route("{id}")]
        [Authorize (Roles = $"{RoleConstants.Admin},{RoleConstants.Manager},{RoleConstants.StockClerk}")]
        public async Task<IActionResult> Delete([FromRoute] string id)
        {
            try
            {
                return await (productService.DeleteProductLineItem(id)) ? Ok() : NotFound();
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error deleting product line item {Id}", id);
                return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while deleting the product line item.");
            }
        }
        [HttpGet("product/{productId}")]
        public async Task<IActionResult> GetLineItemByProduct([FromRoute] string productId)
        {
            try
            {
                var lineItem = await productService.GetLineItemByProductIdAsync(productId);
                if (lineItem == null)
                {
                    return NotFound();
                }
                return Ok(lineItem);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error getting line items for product {ProductId}", productId);
                return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while getting line items by product.");
            }
        }
    }
}
