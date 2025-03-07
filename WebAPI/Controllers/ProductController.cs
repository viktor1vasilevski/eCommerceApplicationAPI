using Main.Interfaces;
using Main.Requests.Category;
using Main.Requests.Product;
using Main.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace WebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProductController : BaseController
    {
        private readonly IProductService _productService;

        public ProductController(IProductService productService)
        {
            _productService = productService;
        }


        [HttpGet("Get")]
        public IActionResult Get([FromQuery] ProductRequest request)
        {
            var response = _productService.GetProducts(request);
            return HandleResponse(response);
        }

        [HttpGet("Get/{id}")]
        public IActionResult GetById([FromRoute] Guid id)
        {
            var response = _productService.GetProductById(id);
            return HandleResponse(response);
        }

        [HttpPost("Create")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "Admin")]
        public IActionResult Create([FromBody] CreateEditProductRequest request)
        {
            var response = _productService.CreateProduct(request);
            return HandleResponse(response);
        }

        [HttpPut("Edit/{id}")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "Admin")]
        public IActionResult Edit([FromRoute] Guid id, [FromBody] CreateEditProductRequest request)
        {
            var response = _productService.EditProduct(id, request);
            return HandleResponse(response);
        }

        [HttpDelete("Delete/{id}")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "Admin")]
        public IActionResult Delete([FromRoute] Guid id)
        {
            var response = _productService.DeleteProduct(id);
            return HandleResponse(response);
        }
    }
}
