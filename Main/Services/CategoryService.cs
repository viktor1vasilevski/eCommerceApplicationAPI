using Data.Context;
using EntityModels.Interfaces;
using EntityModels.Models;
using FluentValidation;
using Main.Constants;
using Main.DTOs.Category;
using Main.Enums;
using Main.Extensions;
using Main.Helpers;
using Main.Interfaces;
using Main.Requests.Category;
using Main.Responses;

namespace Main.Services;

public class CategoryService : ICategoryService
{
    private readonly IUnitOfWork<AppDbContext> _uow;
    private readonly IGenericRepository<Category> _categoryRepository;

    private readonly IValidator<CreateCategoryRequest> _createCategoryRequestValidator;

    public CategoryService(IUnitOfWork<AppDbContext> uow, IValidator<CreateCategoryRequest> createCategoryRequestValidator)
    {
        _uow = uow;
        _categoryRepository = _uow.GetGenericRepository<Category>();

        _createCategoryRequestValidator = createCategoryRequestValidator;
    }

    public ApiResponse<List<CategoryDTO>> GetCategories(CategoryRequest request)
    {
        try
        {
            var categories = _categoryRepository.GetAsQueryableWhereIf(c => c
                .WhereIf(!String.IsNullOrEmpty(request.Name), x => x.Name.ToLower().Contains(request.Name.ToLower())),
                null,
                null
                );

            if (!string.IsNullOrEmpty(request.Sort))
            {
                categories = request.Sort.ToLower() switch
                {
                    "asc" => categories.OrderBy(x => x.Created),
                    "desc" => categories.OrderByDescending(x => x.Created),
                    _ => categories.OrderByDescending(x => x.Created)
                };
            }

            var totalCount = categories.Count();

            if (request.Skip.HasValue)
                categories = categories.Skip(request.Skip.Value);

            if (request.Take.HasValue)
                categories = categories.Take(request.Take.Value);

            var categoriesDTO = categories.Select(x => new CategoryDTO
            {
                Id = x.Id,
                Name = x.Name,
                Created = x.Created,
                CreatedBy = x.CreatedBy,
                LastModified = x.LastModified,
                LastModifiedBy = x.LastModifiedBy
            }).ToList();

            return new ApiResponse<List<CategoryDTO>>()
            {
                Success = true,
                Data = categoriesDTO,
                TotalCount = totalCount,
                NotificationType = NotificationType.Success,
            };
        }
        catch (Exception ex)
        {
            return new ApiResponse<List<CategoryDTO>>
            {
                Success = false,
                NotificationType = NotificationType.ServerError,
                Message = CategoryConstants.ERROR_RETRIEVING_CATEGORIES
            };
        }
    }
    public ApiResponse<CreateCategoryDTO> CreateCategory(CreateCategoryRequest request)
    {
        try
        {
            var validationResult = ValidationHelper.ValidateRequest<CreateCategoryRequest, CreateCategoryDTO>(request, _createCategoryRequestValidator);

            if (validationResult != null)
                return validationResult;

            if (_categoryRepository.Exists(x => x.Name.ToLower() == request.Name.ToLower()))
                return new ApiResponse<CreateCategoryDTO>()
                {
                    Success = false,
                    Message = CategoryConstants.CATEGORY_EXISTS,
                    NotificationType = NotificationType.BadRequest
                };

            _categoryRepository.Insert(new Category { Name = request.Name });
            _uow.SaveChanges();

            return new ApiResponse<CreateCategoryDTO>
            {
                Success = true,
                NotificationType = NotificationType.Success,
                Message = CategoryConstants.CATEGORY_SUCCESSFULLY_CREATED
            };
        }
        catch (Exception ex)
        {
            return new ApiResponse<CreateCategoryDTO>
            {
                Success = false,
                NotificationType = NotificationType.ServerError,
                Message = CategoryConstants.ERROR_CREATING_CATEGORY
            };
        }
    }


}
