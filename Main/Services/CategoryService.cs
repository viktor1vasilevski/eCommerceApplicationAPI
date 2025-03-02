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
using Microsoft.EntityFrameworkCore;

namespace Main.Services;

public class CategoryService : ICategoryService
{
    private readonly IUnitOfWork<AppDbContext> _uow;
    private readonly IGenericRepository<Category> _categoryRepository;

    private readonly IValidator<CreateEditCategoryRequest> _createEditCategoryRequestValidator;

    public CategoryService(IUnitOfWork<AppDbContext> uow, 
        IValidator<CreateEditCategoryRequest> createEditCategoryRequestValidator)
    {
        _uow = uow;
        _categoryRepository = _uow.GetGenericRepository<Category>();

        _createEditCategoryRequestValidator = createEditCategoryRequestValidator;
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

            //if (!string.IsNullOrEmpty(request.Sort))
            //{
            //    categories = request.Sort.ToLower() switch
            //    {
            //        "asc" => categories.OrderBy(x => x.Created),
            //        "desc" => categories.OrderByDescending(x => x.Created),
            //        _ => categories.OrderByDescending(x => x.Created)
            //    };
            //}

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
    public ApiResponse<CreateCategoryDTO> CreateCategory(CreateEditCategoryRequest request)
    {
        try
        {
            var validationResult = ValidationHelper.ValidateRequest<CreateEditCategoryRequest, CreateCategoryDTO>(request, _createEditCategoryRequestValidator);

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
    public ApiResponse<EditCategoryDTO> EditCategory(Guid id, CreateEditCategoryRequest request)
    {
        try
        {
            var validationResult = ValidationHelper.ValidateRequest<CreateEditCategoryRequest, EditCategoryDTO>(request, _createEditCategoryRequestValidator);

            if (validationResult != null)
                return validationResult;

            var category = _categoryRepository.GetByID(id);
            if(category is null)
                return new ApiResponse<EditCategoryDTO> { Success = false, NotificationType = NotificationType.BadRequest, Message = CategoryConstants.CATEGORY_DOESNT_EXIST };

            var editedCategoryNameExist = _categoryRepository.Exists(x => x.Name.ToLower() == request.Name.ToLower() && x.Id != id);
            if (editedCategoryNameExist)
                return new ApiResponse<EditCategoryDTO> { Success = false, NotificationType = NotificationType.BadRequest, Message = CategoryConstants.CATEGORY_EXISTS };

            category.Name = request.Name;

            _categoryRepository.Update(category);
            _uow.SaveChanges();

            return new ApiResponse<EditCategoryDTO>
            {
                Success = true,
                NotificationType = NotificationType.Success,
                Message = CategoryConstants.CATEGORY_SUCCESSFULLY_UPDATE,
                Data = new EditCategoryDTO { Id = id, Name = category.Name }
            };
        }
        catch (Exception ex)
        {
            return new ApiResponse<EditCategoryDTO>
            {
                Success = false,
                NotificationType = NotificationType.ServerError,
                Message = CategoryConstants.ERROR_EDITING_CATEGORY
            };
        }
    }
    public NonGenericApiResponse DeleteCategory(Guid id)
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

            var category = _categoryRepository.GetAsQueryable(x => x.Id == id && x.Name != "UNCATEGORIZED", null, 
                x => x.Include(x => x.Subcategories).ThenInclude(x => x.Products)).FirstOrDefault();

            if (category is null)
                return new NonGenericApiResponse
                {
                    Success = false,
                    NotificationType = NotificationType.BadRequest,
                    Message = CategoryConstants.CATEGORY_DOESNT_EXIST
                };

            if (HasRelatedEntities(category))
                return new NonGenericApiResponse
                {
                    Success = false,
                    NotificationType = NotificationType.BadRequest,
                    Message = CategoryConstants.CATEGORY_HAS_RELATED_ENTITIES
                };


            _categoryRepository.Delete(category);
            _uow.SaveChanges();

            return new NonGenericApiResponse
            {
                Success = true,
                Message = CategoryConstants.CATEGORY_SUCCESSFULLY_DELETED,
                NotificationType = NotificationType.Success,
            };
        }
        catch (Exception ex)
        {
            return new NonGenericApiResponse
            {
                Success = false,
                NotificationType = NotificationType.ServerError,
                Message = CategoryConstants.ERROR_DELETING_CATEGORY
            };
        }
    }
    private bool HasRelatedEntities(Category category)
    {
        return category.Subcategories?.Any() == true ||
               category.Subcategories?.FirstOrDefault()?.Products?.Any() == true;
    }
    public ApiResponse<EditCategoryDTO> GetCategoryById(Guid id)
    {
        try
        {
            if (id == Guid.Empty)
                return new ApiResponse<EditCategoryDTO>
                {
                    Success = false,
                    NotificationType = NotificationType.BadRequest,
                    Message = SharedConstants.INVALID_GUID
                };

            var category = _categoryRepository.GetByID(id);

            if (category == null)
                return new ApiResponse<EditCategoryDTO>
                {
                    Success = false,
                    NotificationType = NotificationType.BadRequest,
                    Message = CategoryConstants.CATEGORY_DOESNT_EXIST
                };

            return new ApiResponse<EditCategoryDTO>
            {
                Success = true,
                NotificationType = NotificationType.Success,
                Data = new EditCategoryDTO { Id = category.Id, Name = category.Name }
            };
        }
        catch (Exception ex)
        {
            return new ApiResponse<EditCategoryDTO>
            {
                Success = false,
                NotificationType = NotificationType.ServerError,
                Message = CategoryConstants.ERROR_GET_CATEGORY
            };
        }
    }
    public ApiResponse<List<SelectCategoryListItemDTO>> GetCategoriesDropdownList()
    {
        try
        {
            var categories = _categoryRepository.GetAsQueryable(null, null, null);

            var categoriesDropdownDTO = categories.Select(x => new SelectCategoryListItemDTO
            {
                Id = x.Id,
                Name = x.Name
            }).ToList();

            return new ApiResponse<List<SelectCategoryListItemDTO>>
            {
                Success = true,
                Data = categoriesDropdownDTO
            };
        }
        catch (Exception ex)
        {
            return new ApiResponse<List<SelectCategoryListItemDTO>>
            {
                Success = false,
                Message = CategoryConstants.ERROR_RETRIEVING_CATEGORIES,
                NotificationType = NotificationType.ServerError
            };
        }
    }
}
