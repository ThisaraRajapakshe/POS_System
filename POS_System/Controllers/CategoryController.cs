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
    public class CategoryController : ControllerBase
    {
        private readonly ICategoryService categoryService;
        private readonly ILogger<CategoryController> logger;

        public CategoryController(ICategoryService categoryService, ILogger<CategoryController> logger)
        {
            this.categoryService = categoryService;
            this.logger = logger;
        }
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            try
            {
                return Ok(await categoryService.GetCategories());
            }
            catch (ArgumentException aex)
            {
                logger.LogWarning(aex, "Invalid request in GetAll categories");
                return BadRequest(aex.Message);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error in GetAll categories");
                return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while getting categories.");
            }
        }

        [HttpGet]
        [Route("{id}")]
        public async Task<IActionResult> Get([FromRoute] string id)
        {
            try
            {
                var category = await categoryService.GetCategory(id);
                if (category == null)
                {
                    return NotFound();
                }
                return Ok(category);
            }
            catch (ArgumentException aex)
            {
                logger.LogWarning(aex, "Invalid request in Get category {CategoryId}", id);
                return BadRequest(aex.Message);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error in Get category {CategoryId}", id);
                return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while getting the category.");
            }
        }
        [HttpPost]
        [Authorize(Roles = $"{RoleConstants.Admin},{RoleConstants.Manager}")]
        public async Task<IActionResult> Create([FromBody] CreateCategoryRequestDto createCategoryRequestDto)
        {
            try
            {
                var result = await categoryService.InsertCategory(createCategoryRequestDto);
                return Ok(result);
            }
            catch (ArgumentException aex)
            {
                logger.LogWarning(aex, "Invalid request in Create category");
                return BadRequest(aex.Message);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error in Create category");
                return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while creating the category.");
            }
        }

        [HttpPut]
        [Route("{id}")]
        [Authorize(Roles = $"{RoleConstants.Admin},{RoleConstants.Manager}")]

        public async Task<IActionResult> Update([FromRoute] string id, [FromBody] UpdateCategoryRequestDto updateCategoryRequestDto)
        {
            try
            {
                var category = await categoryService.UpdateCategory(updateCategoryRequestDto, id);
                if (category == null)
                {
                    return NotFound();
                }
                return Ok(category);
            }
            catch (ArgumentException aex)
            {
                logger.LogWarning(aex, "Invalid request in Update category {CategoryId}", id);
                return BadRequest(aex.Message);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error in Update category {CategoryId}", id);
                return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while updating the category.");
            }
        }
        [HttpDelete]
        [Route("{id}")]
        [Authorize(Roles = RoleConstants.Admin)]
        public async Task<IActionResult> Delete([FromRoute] string id)
        {
            try
            {
                return await (categoryService.DeleteCategory(id)) ? Ok() : NotFound();
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error in Delete category {CategoryId}", id);
                return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while deleting the category.");
            }
        }
    }
}
