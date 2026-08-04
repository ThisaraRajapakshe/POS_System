using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using POS_System.ApplicationServices;
using POS_System.Models.Dto;
using POS_System.Models.Identity;

namespace POS_System.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public class ProductController : ControllerBase
    {
        private readonly IProductService productService;
        private readonly ILogger<ProductController> logger;

        public ProductController(IProductService productService, ILogger<ProductController> logger)
        {
            this.productService = productService;
            this.logger = logger;
        }
        [HttpGet]
        public async Task<IActionResult> Get()
        {
            try
            {
                return Ok(await productService.GetProducts());
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error in Get products");
                return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while getting products.");
            }
        }
        [HttpGet]
        [Route("{id}")]
        public async Task<IActionResult> Get([FromRoute] string id)
        {
            try
            {
                var product = await productService.GetProduct(id);
                if (product == null)
                {
                    return NotFound();
                }
                return Ok(product);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error in Get product {ProductId}", id);
                return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while getting the product.");
            }
        }
        [HttpPost]
        [Authorize(Roles = $"{RoleConstants.Admin},{RoleConstants.Manager},{RoleConstants.StockClerk}")]
        public async Task<IActionResult> Create([FromBody] CreateProductRequestDto createProductRequest)
        {
            try
            {
                var result = await productService.InsertProduct(createProductRequest);
                return Ok(result);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error in Create product");
                return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while creating the product.");
            }
        }

        [HttpPut]
        [Route("{id}")]
        [Authorize(Roles = $"{RoleConstants.Admin},{RoleConstants.Manager},{RoleConstants.StockClerk}")]

        public async Task<IActionResult> Update([FromRoute] string id, [FromBody] UpdateProductRequestDto updateProductRequest)
        {
            try
            {
                var product = await productService.UpdateProduct(updateProductRequest, id);
                if (product == null)
                {
                    return NotFound();
                }
                return Ok(product);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error in Update product {ProductId}", id);
                return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while updating the product.");
            }
        }
        [HttpDelete]
        [Route("{id}")]
        [Authorize(Roles = $"{RoleConstants.Admin},{RoleConstants.Manager}")]
        public async Task<IActionResult> Delete([FromRoute] string id)
        {
            try
            {
                return await (productService.DeleteProduct(id)) ? Ok() : NotFound();
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error in Delete product {ProductId}", id);
                return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while deleting the product.");
            }
        }
        //Get Products by Category ID
        //GET /api/categories/{id}/products
        [HttpGet("category/{categoryId}")]
        public async Task<IActionResult> GetProductsbyCategory([FromRoute] string categoryId)
        {
            try
            {
                var productList = await productService.GetProductsByCategory(categoryId);
                if (productList == null)
                {
                    return NotFound();
                }
                return Ok(productList);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error in GetProductsbyCategory {CategoryId}", categoryId);
                return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while getting products by category.");
            }
        }

    }
}
