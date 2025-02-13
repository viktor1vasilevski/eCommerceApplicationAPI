using Data.Context;
using EntityModels.Interfaces;
using EntityModels.Models;
using FluentValidation;
using Main.Constants;
using Main.DTOs.Category;
using Main.DTOs.Product;
using Main.Enums;
using Main.Helpers;
using Main.Interfaces;
using Main.Requests.Category;
using Main.Requests.Product;
using Main.Responses;
using Main.Validations.Category;
using Main.Validations.Product;

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
}
