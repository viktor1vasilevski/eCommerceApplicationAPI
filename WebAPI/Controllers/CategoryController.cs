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

        [HttpPost("Create")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "Admin")]
        public IActionResult Create([FromBody] CreateCategoryRequest request)
        {
            var response = _categoryService.CreateCategory(request);
            return HandleResponse(response);
        }

        [HttpPut("Edit/{id}")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "Admin")]
        public IActionResult Edit(Guid id, [FromBody] EditCategoryRequest request)
        {
            var response = _categoryService.EditCategory(id, request);
            return HandleResponse(response);
        }
    }
}
