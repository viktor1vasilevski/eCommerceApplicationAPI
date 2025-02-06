using Data.Context;
using EntityModels.Interfaces;
using EntityModels.Models;
using Main.Constants;
using Main.DTOs.Subcategory;
using Main.Enums;
using Main.Extensions;
using Main.Interfaces;
using Main.Requests.Subcategory;
using Main.Responses;
using Microsoft.EntityFrameworkCore;

namespace Main.Services;

public class SubcategoryService : ISubcategoryService
{
    private IUnitOfWork<AppDbContext> _uow;
    private IGenericRepository<Subcategory> _subcategoryRepository;
    private IGenericRepository<Category> _categoryRepository;
    public SubcategoryService(IUnitOfWork<AppDbContext> uow)
    {
        _uow = uow;
        _subcategoryRepository = _uow.GetGenericRepository<Subcategory>();
        _categoryRepository = _uow.GetGenericRepository<Category>();
    }
    public ApiResponse<List<SubcategoryDTO>> GetSubcategories(SubcategoryRequest request)
    {
        try
        {
            var subcategories = _subcategoryRepository.GetAsQueryableWhereIf(x =>
                x.WhereIf(!String.IsNullOrEmpty(request.CategoryId.ToString()), x => x.CategoryId == request.CategoryId)
                 .WhereIf(!String.IsNullOrEmpty(request.Name), x => x.Name.ToLower().Contains(request.Name.ToLower())),
                null,
                x => x.Include(x => x.Category));


            if (!string.IsNullOrEmpty(request.Sort))
            {
                subcategories = request.Sort.ToLower() switch
                {
                    "asc" => subcategories.OrderBy(x => x.Created),
                    "desc" => subcategories.OrderByDescending(x => x.Created),
                    _ => subcategories.OrderByDescending(x => x.Created)
                };
            }

            var totalCount = subcategories.Count();

            if (request.Skip.HasValue)
                subcategories = subcategories.Skip(request.Skip.Value);

            if (request.Take.HasValue)
                subcategories = subcategories.Take(request.Take.Value);

            var subcategoriesDTO = subcategories.Select(x => new SubcategoryDTO
            {
                Id = x.Id,
                Name = x.Name,
                Category = x.Category.Name,
                CategoryId = x.Category.Id,
                Created = x.Created,
                CreatedBy = x.CreatedBy,
                LastModified = x.LastModified,
                LastModifiedBy = x.LastModifiedBy
            }).ToList();

            return new ApiResponse<List<SubcategoryDTO>>()
            {
                Success = true,
                Data = subcategoriesDTO,
                NotificationType = NotificationType.Success,
                TotalCount = totalCount
            };
        }
        catch (Exception ex)
        {
            return new ApiResponse<List<SubcategoryDTO>>()
            {
                Success = false,
                Message = SubcategoryConstants.ERROR_RETRIEVING_SUBCATEGORIES,
                NotificationType = NotificationType.ServerError,
            };
        }
    }
}
