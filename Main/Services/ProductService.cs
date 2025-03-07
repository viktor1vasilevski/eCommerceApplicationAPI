using Data.Context;
using EntityModels.Interfaces;
using EntityModels.Models;
using Main.Constants;
using Main.DTOs.Product;
using Main.DTOs.Subcategory;
using Main.Enums;
using Main.Extensions;
using Main.Interfaces;
using Main.Requests.Product;
using Main.Responses;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Main.Services;

public class ProductService(IUnitOfWork<AppDbContext> _uow, IImageService _imageService, ILogger<CategoryService> _logger) : IProductService
{
    private readonly IGenericRepository<Product> _productRepository = _uow.GetGenericRepository<Product>();

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


            if (!string.IsNullOrEmpty(request.SortBy) && !string.IsNullOrEmpty(request.SortDirection))
            {
                if (request.SortDirection.ToLower() == "asc")
                {
                    products = request.SortBy.ToLower() switch
                    {
                        "created" => products.OrderBy(x => x.Created),
                        "lastmodified" => products.OrderBy(x => x.LastModified),
                        "volume" => products.OrderBy(x => x.Volume),
                        "unitprice" => products.OrderBy(x => x.UnitPrice),
                        "unitquantity" => products.OrderBy(x => x.UnitQuantity),
                        _ => products.OrderBy(x => x.Created)
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
                        _ => products.OrderByDescending(x => x.Created)
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
                LastModified = x.LastModified,
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

    public ApiResponse<CreateProductDTO> CreateProduct(CreateEditProductRequest request)
    {
        try
        {
            string imageType = _imageService.ExtractImageType(request.Image);
            byte[] imageBytes = _imageService.ConvertBase64ToBytes(request.Image);

            var entity = new Product
            {
                Id = Guid.NewGuid(),
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
                Message = ProductConstants.PRODUCT_SUCCESSFULLY_CREATED,
                Data = new CreateProductDTO { Id = entity.Id }
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,"An error occurred while creating product at {Timestamp}. Name: {Name}, Brand: {Brand}, Description: {Description}, UnitPrice: {UnitPrice}, UnitQuantity: {UnitQuantity}, Volume: {Volume}, Scent: {Scent}, Edition: {Edition}, Image: {Image}, SubcategoryId: {SubcategoryId}",
                DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss"),request.Name, request.Brand, request.Description, request.UnitPrice, request.UnitQuantity, request.Volume,
                request.Scent, request.Edition, request.Image, request.SubcategoryId);

            return new ApiResponse<CreateProductDTO>
            {
                Success = false,
                NotificationType = NotificationType.ServerError,
                Message = ProductConstants.ERROR_CREATING_PRODUCT
            };
        }
    }

    public ApiResponse<EditProductDTO> EditProduct(Guid id, CreateEditProductRequest request)
    {
        try
        {
            if (id == Guid.Empty)
                return new ApiResponse<EditProductDTO>
                {
                    Success = false,
                    NotificationType = NotificationType.BadRequest,
                    Message = SharedConstants.INVALID_GUID
                };

            var product = _productRepository.GetByID(id);
            if (product is null)
                return new ApiResponse<EditProductDTO> { Success = false, NotificationType = NotificationType.BadRequest, Message = ProductConstants.PRODUCT_DOESNT_EXIST };

            var productNameExist = _productRepository.Exists(x => x.Name.ToLower() == request.Name.ToLower() && x.Id != id);
            if (productNameExist)
                return new ApiResponse<EditProductDTO> { Success = false, NotificationType = NotificationType.BadRequest, Message = ProductConstants.PRODUCT_EXISTS };

            string imageType = _imageService.ExtractImageType(request.Image);
            byte[] imageBytes = _imageService.ConvertBase64ToBytes(request.Image);

            product.Name = request.Name;
            product.Brand = request.Brand;
            product.Description = request.Description;
            product.Scent = request.Scent;
            product.UnitPrice = request.UnitPrice;
            product.UnitQuantity = request.UnitQuantity;
            product.Volume = request.Volume;
            product.ImageType = imageType;
            product.Image = imageBytes;
            product.Edition = request.Edition;
            product.SubcategoryId = request.SubcategoryId;

            _productRepository.Update(product);
            _uow.SaveChanges();

            return new ApiResponse<EditProductDTO>
            {
                Success = true,
                NotificationType = NotificationType.Success,
                Message = ProductConstants.PRODUCT_SUCCESSFULLY_UPDATED
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while editing product at {Timestamp}. Name: {Name}, Brand: {Brand}, Description: {Description}, UnitPrice: {UnitPrice}, UnitQuantity: {UnitQuantity}, Volume: {Volume}, Scent: {Scent}, Edition: {Edition}, Image: {Image}, SubcategoryId: {SubcategoryId}",
                DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss"), request.Name, request.Brand, request.Description, request.UnitPrice, request.UnitQuantity, request.Volume,
                request.Scent, request.Edition, request.Image, request.SubcategoryId);

            return new ApiResponse<EditProductDTO>
            {
                Success = false,
                NotificationType = NotificationType.ServerError,
                Message = ProductConstants.ERROR_CREATING_PRODUCT
            };
        }
    }
    public NonGenericApiResponse DeleteProduct(Guid id)
    {
        try
        {
            if (id == Guid.Empty)
                return new NonGenericApiResponse
                {
                    Success = false,
                    NotificationType = NotificationType.BadRequest,
                    Message = SharedConstants.INVALID_GUID
                };

            var product = _productRepository.GetByID(id);

            if (product is null)
                return new NonGenericApiResponse
                {
                    Success = false,
                    NotificationType = NotificationType.BadRequest,
                    Message = ProductConstants.PRODUCT_DOESNT_EXIST
                };

            _productRepository.Delete(product);
            _uow.SaveChanges();

            return new NonGenericApiResponse
            {
                Success = true,
                Message = ProductConstants.PRODUCT_SUCCESSFULLY_DELETED,
                NotificationType = NotificationType.Success,
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while deleting product at {Timestamp}. ProductId: {ProductId}",
                    DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss"), id);
            return new NonGenericApiResponse
            {
                Success = false,
                NotificationType = NotificationType.ServerError,
                Message = ProductConstants.ERROR_DELETING_PRODUCT
            };
        }
    }

    public ApiResponse<ProductDTO> GetProductById(Guid id)
    {
        try
        {
            if (id == Guid.Empty)
                return new ApiResponse<ProductDTO>
                {
                    Success = false,
                    NotificationType = NotificationType.BadRequest,
                    Message = SharedConstants.INVALID_GUID
                };

            var product = _productRepository.GetByID(id);
            if (product is null)
                return new ApiResponse<ProductDTO>
                {
                    Success = false,
                    NotificationType = NotificationType.BadRequest,
                    Message = ProductConstants.PRODUCT_DOESNT_EXIST
                };

            // Validate image data
            string? imageBase64 = null;
            if (product.Image != null && product.Image.Length > 0)
            {
                if (string.IsNullOrEmpty(product.ImageType))
                {
                    _logger.LogWarning("Product ID {ProductId} has an image but missing ImageType.", product.Id);
                }
                else
                {
                    imageBase64 = $"data:{product.ImageType};base64,{Convert.ToBase64String(product.Image)}";
                }
            }

            _logger.LogInformation("Product ID {ProductId}: Retrieved ImageType={ImageType}, ImageSize={Size} bytes",
                product.Id, product.ImageType ?? "N/A", product.Image?.Length ?? 0);

            return new ApiResponse<ProductDTO>
            {
                Success = true,
                NotificationType = NotificationType.Success,
                Data = new ProductDTO
                {
                    Name = product.Name,
                    Brand = product.Brand,
                    ImageBase64 = imageBase64,
                    Edition = product.Edition,
                    Description = product.Description,
                    Id = product.Id,
                    Scent = product.Scent,
                    UnitPrice = product.UnitPrice,
                    UnitQuantity = product.UnitQuantity,
                    Volume = product.Volume,
                    SubcategoryId = product.SubcategoryId
                }
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while getting product at {Timestamp}. ProductId: {ProductId}",
                    DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss"), id);

            return new ApiResponse<ProductDTO>
            {
                Success = false,
                NotificationType = NotificationType.ServerError,
                Message = ProductConstants.ERROR_GET_PRODUCT
            };
        }
    }



}
