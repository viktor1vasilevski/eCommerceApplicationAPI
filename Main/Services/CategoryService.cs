using Azure;
using Data.Context;
using EntityModels.Interfaces;
using EntityModels.Models;
using Main.Constants;
using Main.DTOs.Category;
using Main.DTOs.Subcategory;
using Main.Enums;
using Main.Extensions;
using Main.Helpers;
using Main.Interfaces;
using Main.Requests.Category;
using Main.Responses;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Main.Services;

public class CategoryService(IUnitOfWork<AppDbContext> _uow, ILogger<CategoryService> _logger) : ICategoryService
{
    private readonly IGenericRepository<Category> _categoryRepository = _uow.GetGenericRepository<Category>();

    public ApiResponse<List<CategoryDTO>> GetCategories(CategoryRequest request)
    {
        try
        {
            var categories = _categoryRepository.GetAsQueryableWhereIf(c => c
                .WhereIf(!String.IsNullOrEmpty(request.Name), x => x.Name.ToLower().Contains(request.Name.ToLower())), null, null);

            if (!string.IsNullOrEmpty(request.SortBy) && !string.IsNullOrEmpty(request.SortDirection))
            {
                if (request.SortDirection.ToLower() == "asc")
                {
                    categories = request.SortBy.ToLower() switch
                    {
                        "created" => categories.OrderBy(x => x.Created),
                        "lastmodified" => categories.OrderBy(x => x.LastModified),
                        _ => categories.OrderBy(x => x.Created)
                    };
                }
                else if (request.SortDirection.ToLower() == "desc")
                {
                    categories = request.SortBy.ToLower() switch
                    {
                        "created" => categories.OrderByDescending(x => x.Created),
                        "lastmodified" => categories.OrderByDescending(x => x.LastModified),
                        _ => categories.OrderByDescending(x => x.Created)
                    };
                }
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
                LastModified = x.LastModified,
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
            _logger.LogError(ex, "An error occurred while retrieving categories at {Timestamp}. Name: {Name}",
                DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss"), request.Name);

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
            _logger.LogError(ex, "An error occurred while creating category at {Timestamp}. Name: {Name}",
                    DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss"), request.Name);

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
            _logger.LogError(ex, "An error occurred while editing category at {Timestamp}. Name: {Name}",
                    DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss"), request.Name);

            return new ApiResponse<EditCategoryDTO>
            {
                Success = false,
                NotificationType = NotificationType.ServerError,
                Message = CategoryConstants.ERROR_EDITING_CATEGORY
            };
        }
    }

    public ApiResponse<DeleteCategoryDTO> DeleteCategory(Guid id)
    {
        try
        {
            if (id == Guid.Empty)
                return new ApiResponse<DeleteCategoryDTO>
                {
                    Success = false,
                    NotificationType = NotificationType.BadRequest,
                    Message = SharedConstants.INVALID_GUID
                };

            var category = _categoryRepository.GetAsQueryable(x => x.Id == id && x.Name != "UNCATEGORIZED", null, 
                x => x.Include(x => x.Subcategories).ThenInclude(x => x.Products)).FirstOrDefault();

            if (category is null)
                return new ApiResponse<DeleteCategoryDTO>
                {
                    Success = false,
                    NotificationType = NotificationType.BadRequest,
                    Message = CategoryConstants.CATEGORY_DOESNT_EXIST
                };

            if (HasRelatedEntities(category))
                return new ApiResponse<DeleteCategoryDTO>
                {
                    Success = false,
                    NotificationType = NotificationType.BadRequest,
                    Message = CategoryConstants.CATEGORY_HAS_RELATED_ENTITIES
                };


            _categoryRepository.Delete(category);
            _uow.SaveChanges();

            return new ApiResponse<DeleteCategoryDTO>
            {
                Success = true,
                Message = CategoryConstants.CATEGORY_SUCCESSFULLY_DELETED,
                NotificationType = NotificationType.Success,
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while deleting category at {Timestamp}. CategoryId: {CategoryId}",
                        DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss"), id);

            return new ApiResponse<DeleteCategoryDTO>
            {
                Success = false,
                NotificationType = NotificationType.ServerError,
                Message = CategoryConstants.ERROR_DELETING_CATEGORY
            };
        }
    }

    public ApiResponse<CategoryDetailsDTO> GetCategoryById(Guid id)
    {
        try
        {
            var category = _categoryRepository.GetByID(id);

            if (category is null)
                return new ApiResponse<CategoryDetailsDTO>
                {
                    Success = false,
                    NotificationType = NotificationType.BadRequest,
                    Message = CategoryConstants.CATEGORY_DOESNT_EXIST
                };

            return new ApiResponse<CategoryDetailsDTO>
            {
                Success = true,
                NotificationType = NotificationType.Success,
                Data = new CategoryDetailsDTO { Id = category.Id, Name = category.Name }
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while deleting category at {Timestamp}. CategoryId: {CategoryId}",
                        DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss"), id);

            return new ApiResponse<CategoryDetailsDTO>
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
            _logger.LogError(ex, "An error occurred while getting categories for dropdown list at {Timestamp}",
                    DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss"));

            return new ApiResponse<List<SelectCategoryListItemDTO>>
            {
                Success = false,
                Message = CategoryConstants.ERROR_RETRIEVING_CATEGORIES,
                NotificationType = NotificationType.ServerError
            };
        }
    }


    private bool HasRelatedEntities(Category category)
    {
        return category.Subcategories?.Any() == true ||
               category.Subcategories?.FirstOrDefault()?.Products?.Any() == true;
    }

    public ApiResponse<List<CategoryWithSubcategoriesDetialsDTO>> GetCategoriesWithSubcategories()
    {
        var categories = _categoryRepository.Get(
            x => x.Name != "UNCATEGORIZED" && x.Subcategories.Any(sc => sc.Products.Any()),
            null,
            query => query.Include(c => c.Subcategories).ThenInclude(sc => sc.Products)
        ).ToList();

        var categoryDtos = categories.Select(c => new CategoryWithSubcategoriesDetialsDTO
        {
            Id = c.Id,
            Name = c.Name,
            Slug = SlugHelper.GenerateHashSlug(c.Id),
            Subcategories = c.Subcategories
                .Where(sc => sc.Products.Any())
                .Select(sc => new SubcategorySlugDTO
                {
                    Id = sc.Id,
                    Name = sc.Name,
                    Slug = SlugHelper.GenerateHashSlug(sc.Id),
                }).ToList()
        })
        .Where(c => c.Subcategories.Any())
        .ToList();

        return new ApiResponse<List<CategoryWithSubcategoriesDetialsDTO>>
        {
            Success = true,
            NotificationType = NotificationType.Success,
            Data = categoryDtos
        };
    }



}
