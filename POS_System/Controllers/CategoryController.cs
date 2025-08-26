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
    public class CategoryController : ControllerBase
    {
        private readonly ICategoryService categoryService;

        public CategoryController(ICategoryService categoryService)
        {
            this.categoryService = categoryService;
        }
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            return Ok(await categoryService.GetCategories());
        }

        [HttpGet]
        [Route("{id}")]
        public async Task<IActionResult> Get([FromRoute] string id)
        {
            var product = await categoryService.GetCategory(id);
            if (product == null)
            {
                return NotFound();
            }
            return Ok(product);
        }
        [HttpPost]
        [Authorize(Roles = $"{RoleConstants.Admin},{RoleConstants.Manager}")]
        public async Task<IActionResult> Create([FromBody] CreateCategoryRequestDto createCategoryRequestDto)
        {
            return Ok(await categoryService.InsertCategory(createCategoryRequestDto));
        }

        [HttpPut]
        [Route("{id}")]
        [Authorize(Roles = $"{RoleConstants.Admin},{RoleConstants.Manager}")]

        public async Task<IActionResult> Update([FromRoute] string id, [FromBody] UpdateCategoryRequestDto updateCategoryRequestDto)
        {
            var product = await categoryService.UpdateCategory(updateCategoryRequestDto, id);
            if (product == null)
            {
                return NotFound();
            }
            return Ok(product);
        }
        [HttpDelete]
        [Route("{id}")]
        [Authorize(Roles = RoleConstants.Admin)]
        public async Task<IActionResult> Delete([FromRoute] string id)
        {
            return await (categoryService.DeleteCategory(id)) ? Ok() : NotFound();
        }
    }
}
