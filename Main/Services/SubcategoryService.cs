using Data.Context;
using EntityModels.Interfaces;
using EntityModels.Models;
using FluentValidation;
using Main.Constants;
using Main.DTOs.Category;
using Main.DTOs.Subcategory;
using Main.Enums;
using Main.Extensions;
using Main.Helpers;
using Main.Interfaces;
using Main.Requests.Category;
using Main.Requests.Subcategory;
using Main.Responses;
using Microsoft.EntityFrameworkCore;

namespace Main.Services;

public class SubcategoryService : ISubcategoryService
{
    private IUnitOfWork<AppDbContext> _uow;
    private IGenericRepository<Subcategory> _subcategoryRepository;
    private IGenericRepository<Category> _categoryRepository;

    private readonly IValidator<CreateSubcategoryRequest> _createSubcategoryRequestValidator;
    public SubcategoryService(IUnitOfWork<AppDbContext> uow, IValidator<CreateSubcategoryRequest> createSubcategoryRequestValidator)
    {
        _uow = uow;
        _subcategoryRepository = _uow.GetGenericRepository<Subcategory>();
        _categoryRepository = _uow.GetGenericRepository<Category>();

        _createSubcategoryRequestValidator = createSubcategoryRequestValidator;
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

    public ApiResponse<CreateSubcategoryDTO> CreateSubcategory(CreateSubcategoryRequest request)
    {
        try
        {
            var validationResult = ValidationHelper.ValidateRequest<CreateSubcategoryRequest, CreateSubcategoryDTO>(request, _createSubcategoryRequestValidator);

            if (validationResult != null)
                return validationResult;

            if (_subcategoryRepository.Exists(x => x.Name.ToLower() == request.Name.ToLower()))
                return new ApiResponse<CreateSubcategoryDTO>()
                {
                    Success = false,
                    Message = SubcategoryConstants.SUBCATEGORY_EXISTS,
                    NotificationType = NotificationType.BadRequest
                };

            if(!_categoryRepository.Exists(x => x.Id == request.CategoryId))
                return new ApiResponse<CreateSubcategoryDTO>()
                {
                    Success = false,
                    Message = CategoryConstants.CATEGORY_DOESNT_EXIST,
                    NotificationType = NotificationType.BadRequest
                };

            var entity = new Subcategory()
            {
                Name = request.Name,
                CategoryId = request.CategoryId
            };

            _subcategoryRepository.Insert(entity);
            _uow.SaveChanges();

            return new ApiResponse<CreateSubcategoryDTO>
            {
                Success = true,
                NotificationType = NotificationType.Success,
                Data = new CreateSubcategoryDTO(),
                Message = SubcategoryConstants.SUBCATEGORY_SUCCESSFULLY_CREATED
            };
        }
        catch (Exception ex)
        {
            return new ApiResponse<CreateSubcategoryDTO>
            {
                Success = false,
                NotificationType = NotificationType.ServerError,
                Message = SubcategoryConstants.ERROR_CREATING_SUBCATEGORY,
            };
        }
    }
}
