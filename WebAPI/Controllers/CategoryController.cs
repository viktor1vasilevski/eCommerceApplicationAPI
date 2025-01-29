using Main.Interfaces;
using Main.Requests.Category;
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
    }
}
