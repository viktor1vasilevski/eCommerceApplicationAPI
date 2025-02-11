using FluentValidation;
using Main.Requests.Category;

namespace Main.Validations.Category;

public class CreateEditCategoryRequestValidator : AbstractValidator<CreateEditCategoryRequest>
{
    public CreateEditCategoryRequestValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Category Name is required.")
            .MinimumLength(3).WithMessage("Category Name must be at least 3 characters long.");
    }
}
