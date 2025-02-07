using FluentValidation;
using Main.Requests.Subcategory;

namespace Main.Validations.Subcategory;

public class CreateSubcategoryRequestValidator : AbstractValidator<CreateSubcategoryRequest>
{
    public CreateSubcategoryRequestValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Category Name is required.")
            .MinimumLength(3).WithMessage("Category Name must be at least 3 characters long.");


        RuleFor(x => x.CategoryId)
            .NotEmpty().WithMessage("Category Id is required.")
            .Must(id => id != Guid.Empty).WithMessage("Category Id must not be an empty GUID.");
    }
}
