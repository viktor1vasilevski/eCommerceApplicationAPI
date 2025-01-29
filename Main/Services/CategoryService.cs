using Data.Context;
using EntityModels.Interfaces;
using EntityModels.Models;
using Main.Constants;
using Main.DTOs.Category;
using Main.Enums;
using Main.Extensions;
using Main.Interfaces;
using Main.Requests.Category;
using Main.Responses;

namespace Main.Services;

public class CategoryService : ICategoryService
{
    private readonly IUnitOfWork<AppDbContext> _uow;
    private readonly IGenericRepository<Category> _categoryRepository;

    public CategoryService(IUnitOfWork<AppDbContext> uow)
    {
        _uow = uow;
        _categoryRepository = _uow.GetGenericRepository<Category>();
    }
    public ApiResponse<List<CategoryDTO>> GetCategories(CategoryRequest request)
    {
        try
        {
            // validatior



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
}
