using Main.Enums;
using Main.Interfaces;
using Main.Requests.Category;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace WebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CategoryController : BaseController
    {
        private readonly ICategoryService _categoryService;
        public CategoryController(ICategoryService categoryService)
        {
            _categoryService = categoryService;
        }


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
        public IActionResult Create([FromBody] CreateCategoryRequest request)
        {
            var response = _categoryService.CreateCategory(request);
            return HandleResponse(response);
        }

        [HttpPut("Edit/{id}")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "Admin")]
        public IActionResult Edit([FromRoute] Guid id, [FromBody] EditCategoryRequest request)
        {
            var response = _categoryService.EditCategory(id, request);
            return HandleResponse(response);
        }

        [HttpDelete("Delete/{id}")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "Admin")]
        public IActionResult Delete([FromRoute] Guid id)
        {
            var response = _categoryService.DeleteCategory(id);
            return Ok(response);
        }
    }
}
