using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using POS_System.ApplicationServices;
using POS_System.Models.Domain;
using POS_System.Models.Dto;
using System.Reflection.Metadata.Ecma335;

namespace POS_System.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProductController : ControllerBase
    {
        private readonly IProductService productService;

        public ProductController(IProductService productService)
        {
            this.productService = productService;
        }
        [HttpGet]
        public async Task<IActionResult> Get()
        {
            return Ok(await productService.GetProducts());
        }
        [HttpGet]
        [Route("{id}")]
        public async Task<IActionResult> Get([FromRoute] string id)
        {
            var product = await productService.GetProduct(id);
            if (product == null)
            {
                return NotFound();
            }
            return Ok(product);
        }
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateProductRequestDto createProductRequest)
        {
            return Ok(await productService.InsertProduct(createProductRequest));
        }

        [HttpPut]
        [Route("{id}")]
        public async Task<IActionResult> Update([FromRoute] string id, [FromBody] UpdateProductRequestDto updateProductRequest)
        {
            var product = await productService.UpdateProduct(updateProductRequest, id);
            if (product == null)
            {
                return NotFound();
            }
            return Ok(product);
        }
        [HttpDelete]
        [Route("{id}")]
        public async Task<IActionResult> Delete([FromRoute] string id)
        {
            return await (productService.DeleteProduct(id)) ? Ok(): NotFound();
        }
        //Get Products by Category ID
        //GET /api/categories/{id}/products
        [HttpGet("category/{categoryId}")]
        public async Task<IActionResult> GetProductsbyCategory([FromRoute] string categoryId)
        {
            var productList = await productService.GetProductsByCategory(categoryId);
            if (productList == null)
            {
                return NotFound();
            }
            return Ok(productList);
        }

    }
}
