﻿using Data.Context;
using EntityModels.Interfaces;
using EntityModels.Models;
using FluentValidation;
using Main.Constants;
using Main.DTOs.Product;
using Main.Enums;
using Main.Extensions;
using Main.Helpers;
using Main.Interfaces;
using Main.Requests.Product;
using Main.Responses;
using Microsoft.EntityFrameworkCore;

namespace Main.Services;

public class ProductService : IProductService
{
    private readonly IUnitOfWork<AppDbContext> _uow;
    private readonly IGenericRepository<Product> _productRepository;
    private readonly IImageService _imageService;

    private readonly IValidator<CreateEditProductRequest> _createEditProductRequestValidator;
    public ProductService(IUnitOfWork<AppDbContext> uow, IImageService imageService, IValidator<CreateEditProductRequest> createEditProductRequestValidator)
    {
        _uow = uow;
        _productRepository = _uow.GetGenericRepository<Product>();
        _imageService = imageService;

        _createEditProductRequestValidator = createEditProductRequestValidator;
    }
    public ApiResponse<CreateProductDTO> CreateProduct(CreateEditProductRequest request)
    {
        try
        {
            var validationResult = ValidationHelper.ValidateRequest<CreateEditProductRequest, CreateProductDTO>(request, _createEditProductRequestValidator);

            if (validationResult != null)
                return validationResult;

            string imageType = _imageService.ExtractImageType(request.Image);
            byte[] imageBytes = _imageService.ConvertBase64ToBytes(request.Image);

            var entity = new Product
            {
                Name = request.Name,
                Brand = request.Brand,
                Description = request.Description,
                Edition = request.Edition,
                Scent = request.Scent,
                Volume = request.Volume,
                UnitPrice = request.UnitPrice,
                UnitQuantity = request.UnitQuantity,
                Image = imageBytes,
                ImageType = imageType,
                SubcategoryId = request.SubcategoryId,
            };

            _productRepository.Insert(entity);
            _uow.SaveChanges();

            return new ApiResponse<CreateProductDTO>
            {
                Success = true,
                NotificationType = NotificationType.Success,
                Message = ProductConstants.PRODUCT_SUCCESSFULLY_CREATED
            };
        }
        catch (Exception ex)
        {
            return new ApiResponse<CreateProductDTO>
            {
                Success = false,
                NotificationType = NotificationType.ServerError,
                Message = ProductConstants.ERROR_CREATING_PRODUCT
            };
        }
    }

    public ApiResponse<List<ProductDTO>> GetProducts(ProductRequest request)
   {
        try
        {
            var products = _productRepository.GetAsQueryableWhereIf(x =>
            x.WhereIf(!String.IsNullOrEmpty(request.CategoryId.ToString()), x => x.Subcategory.Category.Id == request.CategoryId)
             .WhereIf(!String.IsNullOrEmpty(request.SubcategoryId.ToString()), x => x.Subcategory.Id == request.SubcategoryId)
             .WhereIf(!String.IsNullOrEmpty(request.Name), x => x.Name.ToLower().Contains(request.Name.ToLower()))
             .WhereIf(!String.IsNullOrEmpty(request.Description), x => x.Description.ToLower().Contains(request.Description.ToLower()))
             .WhereIf(!String.IsNullOrEmpty(request.Brand), x => x.Brand.ToLower().Contains(request.Brand.ToLower()))
             .WhereIf(!String.IsNullOrEmpty(request.Edition), x => x.Edition.ToLower().Contains(request.Edition.ToLower()))
             .WhereIf(!String.IsNullOrEmpty(request.Scent), x => x.Scent.ToLower().Contains(request.Scent.ToLower())),
            null,
            x => x.Include(x => x.Subcategory).ThenInclude(sc => sc.Category));

            // Apply dynamic sorting
            if (!string.IsNullOrEmpty(request.SortBy) && !string.IsNullOrEmpty(request.SortDirection))
            {
                // Ensure valid sort direction
                if (request.SortDirection.ToLower() == "asc")
                {
                    products = request.SortBy.ToLower() switch
                    {
                        "created" => products.OrderBy(x => x.Created),
                        "lastmodified" => products.OrderBy(x => x.LastModified),
                        "volume" => products.OrderBy(x => x.Volume),
                        "unitprice" => products.OrderBy(x => x.UnitPrice),
                        "unitquantity" => products.OrderBy(x => x.UnitQuantity),
                        _ => products.OrderBy(x => x.Created) // Default sorting
                    };
                }
                else if (request.SortDirection.ToLower() == "desc")
                {
                    products = request.SortBy.ToLower() switch
                    {
                        "created" => products.OrderByDescending(x => x.Created),
                        "lastmodified" => products.OrderByDescending(x => x.LastModified),
                        "volume" => products.OrderByDescending(x => x.Volume),
                        "unitprice" => products.OrderByDescending(x => x.UnitPrice),
                        "unitquantity" => products.OrderByDescending(x => x.UnitQuantity),
                        _ => products.OrderByDescending(x => x.Created) // Default sorting
                    };
                }
            }

            var totalCount = products.Count();

            if (request.Skip.HasValue)
                products = products.Skip(request.Skip.Value);

            if (request.Take.HasValue)
                products = products.Take(request.Take.Value);

            var productsDTO = products.Select(x => new ProductDTO
            {
                Id = x.Id,
                Name = x.Name,
                Brand = x.Brand,
                Description = x.Description,
                UnitPrice = x.UnitPrice,
                UnitQuantity = x.UnitQuantity,
                Volume = x.Volume,
                Scent = x.Scent,
                ImageBase64 = x.Image != null ? $"data:{x.ImageType};base64,{Convert.ToBase64String(x.Image)}" : null,
                ImageType = x.ImageType,
                Edition = x.Edition,
                Category = x.Subcategory.Category.Name,
                Subcategory = x.Subcategory.Name,
                SubcategoryId = x.SubcategoryId,
                Created = x.Created,
                CreatedBy = x.CreatedBy,
                LastModified = x.LastModified,
                LastModifiedBy = x.LastModifiedBy
            }).ToList();

            return new ApiResponse<List<ProductDTO>>()
            {
                Success = true,
                Data = productsDTO,
                TotalCount = totalCount,
                NotificationType = NotificationType.Success
            };
        }
        catch (Exception ex)
        {
            return new ApiResponse<List<ProductDTO>>()
            {
                Success = false,
                Message = ProductConstants.ERROR_RETRIEVING_PRODUCTS,
                NotificationType = NotificationType.ServerError
            };
        }
    }
}
