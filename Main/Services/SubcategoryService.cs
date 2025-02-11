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
using Main.Requests.Subcategory;
using Main.Responses;
using Microsoft.EntityFrameworkCore;

namespace Main.Services;

public class SubcategoryService : ISubcategoryService
{
    private IUnitOfWork<AppDbContext> _uow;
    private IGenericRepository<Subcategory> _subcategoryRepository;
    private IGenericRepository<Category> _categoryRepository;

    private readonly IValidator<CreateEditSubcategoryRequest> _createEditSubcategoryRequestValidator;
    public SubcategoryService(IUnitOfWork<AppDbContext> uow, IValidator<CreateEditSubcategoryRequest> createEditSubcategoryRequestValidator)
    {
        _uow = uow;
        _subcategoryRepository = _uow.GetGenericRepository<Subcategory>();
        _categoryRepository = _uow.GetGenericRepository<Category>();

        _createEditSubcategoryRequestValidator = createEditSubcategoryRequestValidator;
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

    public ApiResponse<CreateSubcategoryDTO> CreateSubcategory(CreateEditSubcategoryRequest request)
    {
        try
        {
            var validationResult = ValidationHelper.ValidateRequest<CreateEditSubcategoryRequest, CreateSubcategoryDTO>(request, _createEditSubcategoryRequestValidator);

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

    public ApiResponse<SubcategoryDetailsDTO> GetSubcategoryById(Guid id)
    {
        try
        {
            if(id == Guid.Empty)
                return new ApiResponse<SubcategoryDetailsDTO>() { Success = false, NotificationType = NotificationType.BadRequest, Message = SharedConstants.INVALID_GUID };

            if (_subcategoryRepository.Exists(x => x.Id == id))
            {
                var subcategory = _subcategoryRepository.GetAsQueryable(x => x.Id == id, null,
                    x => x.Include(x => x.Products).Include(x => x.Category)).FirstOrDefault();

                return new ApiResponse<SubcategoryDetailsDTO>
                {
                    Success = true,
                    NotificationType = NotificationType.Success,
                    Data = new SubcategoryDetailsDTO()
                    {
                        Id = subcategory.Id,
                        Name = subcategory.Name,
                        CategoryId = subcategory.Category.Id,
                        CategoryName = subcategory.Category.Name
                    }
                };
            }

            return new ApiResponse<SubcategoryDetailsDTO>
            {
                Success = false,
                NotificationType = NotificationType.BadRequest,
                Message = SubcategoryConstants.SUBCATEGORY_DOESNT_EXIST,
            };


        }
        catch (Exception ex)
        {
            return new ApiResponse<SubcategoryDetailsDTO>
            {
                Success = false,
                NotificationType= NotificationType.ServerError,
                Message = SubcategoryConstants.ERROR_GET_SUBCATEGORY_BY_ID,
            };
        }
    }

    public ApiResponse<SubcategoryDTO> EditSubcategory(Guid id, CreateEditSubcategoryRequest request)
    {
        try
        {
            var validationResult = ValidationHelper.ValidateRequest<CreateEditSubcategoryRequest, SubcategoryDTO>(request, _createEditSubcategoryRequestValidator);

            if (validationResult != null)
                return validationResult;

            var subcategory = _subcategoryRepository.GetByID(id);
            if (subcategory is null)
                return new ApiResponse<SubcategoryDTO> { Success = false, NotificationType = NotificationType.BadRequest, Message = SubcategoryConstants.SUBCATEGORY_DOESNT_EXIST };

            var editedSubcategoryNameExist = _subcategoryRepository.Exists(x => x.Name.ToLower() == request.Name.ToLower() && x.Id != id);
            if (editedSubcategoryNameExist)
                return new ApiResponse<SubcategoryDTO> { Success = false, NotificationType = NotificationType.BadRequest, Message = SubcategoryConstants.SUBCATEGORY_EXISTS };

            subcategory.Name = request.Name;
            subcategory.CategoryId = request.CategoryId;

            _subcategoryRepository.Update(subcategory);
            _uow.SaveChanges();

            return new ApiResponse<SubcategoryDTO>
            {
                Success = true,
                NotificationType = NotificationType.Success,
                Message = SubcategoryConstants.SUBCATEGORY_SUCCESSFULLY_EDITED,
                Data = new SubcategoryDTO 
                {
                    Id = subcategory.Id,
                    Name = subcategory.Name,
                    CategoryId = subcategory.CategoryId
                }
            };

        }
        catch (Exception ex)
        {
            return new ApiResponse<SubcategoryDTO>
            {
                Success = false,
                NotificationType = NotificationType.ServerError,
                Message = SubcategoryConstants.ERROR_EDITING_SUBCATEGORY
            };
        }
    }

    public NonGenericApiResponse DeleteSubcategory(Guid id)
    {
        try
        {
            if(id == Guid.Empty)
                return new NonGenericApiResponse { Success = false, NotificationType = NotificationType.BadRequest, Message = SharedConstants.INVALID_GUID };

            if (_subcategoryRepository.Exists(x => x.Id == id))
            {
                var subcategory = _subcategoryRepository.GetAsQueryable(x => x.Id == id).Include(x => x.Category).Include(x => x.Products).FirstOrDefault();

                if (!subcategory.Products.Any() && !subcategory.Category.Name.Equals("UNCATEGORIZED"))
                {
                    _subcategoryRepository.Delete(id);
                    _uow.SaveChanges();

                    return new NonGenericApiResponse()
                    {
                        Success = true,
                        Message = SubcategoryConstants.SUBCATEGORY_SUCCESSFULLY_DELETED,
                        NotificationType = NotificationType.Success
                    };
                } 
                else
                {
                    return new NonGenericApiResponse()
                    {
                        Success = false,
                        Message = SubcategoryConstants.SUBCATEGORY_HAS_RELATED_ENTITIES,
                        NotificationType = NotificationType.BadRequest
                    };
                }
            }
            else
            {
                return new NonGenericApiResponse()
                {
                    Success = false,
                    Message = SubcategoryConstants.SUBCATEGORY_DOESNT_EXIST,
                    NotificationType = NotificationType.BadRequest
                };
            }
        }
        catch (Exception ex)
        {
            return new NonGenericApiResponse
            {
                Success = false,
                NotificationType = NotificationType.ServerError,
                Message = SubcategoryConstants.ERROR_DELETING_SUBCATEGORY
            };
        }
    }
}
