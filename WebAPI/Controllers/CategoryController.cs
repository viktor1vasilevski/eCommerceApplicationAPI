using Main.Interfaces;
using Main.Requests.Category;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace WebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CategoryController(ICategoryService categoryService) : BaseController
    {
        private readonly ICategoryService _categoryService = categoryService;

        [HttpGet("Get")]
        public IActionResult Get([FromQuery] CategoryRequest request)
        {
            var response = _categoryService.GetCategories(request);
            return HandleResponse(response);
        }

        [HttpGet("Get/{id}")]
        public IActionResult GetById([FromRoute] Guid id)
        {
            var response = _categoryService.GetCategoryById(id);
            return HandleResponse(response);
        }

        [HttpPost("Create")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "Admin")]
        public IActionResult Create([FromBody] CreateEditCategoryRequest request)
        {
            var response = _categoryService.CreateCategory(request);
            return HandleResponse(response);
        }

        [HttpPut("Edit/{id}")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "Admin")]
        public IActionResult Edit([FromRoute] Guid id, [FromBody] CreateEditCategoryRequest request)
        {
            var response = _categoryService.EditCategory(id, request);
            return HandleResponse(response);
        }

        [HttpDelete("Delete/{id}")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "Admin")]
        public IActionResult Delete([FromRoute] Guid id)
        {
            var response = _categoryService.DeleteCategory(id);
            return HandleResponse(response);
        }

        [HttpGet("GetCategoriesDropdownList")]
        public IActionResult GetCategoriesDropdownList()
        {
            var response = _categoryService.GetCategoriesDropdownList();
            return HandleResponse(response);

        }

        [HttpGet("GetCategoriesWithSubcategories")]
        public IActionResult GetCategoriesWithSubcategories()
        {
            var response = _categoryService.GetCategoriesWithSubcategories();
            return HandleResponse(response);

        }
    }
}
