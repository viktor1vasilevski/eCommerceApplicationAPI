﻿using Main.Interfaces;
using Main.Requests.Category;
using Main.Requests.Subcategory;
using Main.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace WebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SubcategoryController(ISubcategoryService subcategoryService) : BaseController
    {
        private readonly ISubcategoryService _subcategoryService = subcategoryService;


        [HttpGet("Get")]
        public IActionResult Get([FromQuery] SubcategoryRequest request)
        {
            var response = _subcategoryService.GetSubcategories(request);
            return Ok(response);
        }

        [HttpGet("Get/{id}")]
        public IActionResult GetById([FromRoute] Guid id)
        {
            var response = _subcategoryService.GetSubcategoryById(id);
            return HandleResponse(response);
        }

        [HttpPost("Create")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "Admin")]
        public IActionResult Create([FromBody] CreateEditSubcategoryRequest request)
        {
            var response = _subcategoryService.CreateSubcategory(request);
            return HandleResponse(response);
        }

        [HttpPut("Edit/{id}")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "Admin")]
        public IActionResult Edit([FromRoute] Guid id, [FromBody] CreateEditSubcategoryRequest request)
        {
            var response = _subcategoryService.EditSubcategory(id, request);
            return HandleResponse(response);
        }

        [HttpDelete("Delete/{id}")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "Admin")]
        public IActionResult Delete([FromRoute] Guid id)
        {
            var response = _subcategoryService.DeleteSubcategory(id);
            return HandleResponse(response);
        }

        [HttpGet("GetSubcategoriesDropdownList")]
        public IActionResult GetSubcategoriesDropdownList()
        {
            var response = _subcategoryService.GetSubcategoriesDropdownList();
            return HandleResponse(response);

        }
    }
}
