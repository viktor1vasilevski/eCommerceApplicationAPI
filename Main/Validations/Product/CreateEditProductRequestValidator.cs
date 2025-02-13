using FluentValidation;
using Main.Requests.Product;
using System.Text.RegularExpressions;

namespace Main.Validations.Product;

public class CreateEditProductRequestValidator : AbstractValidator<CreateEditProductRequest>
{
    public CreateEditProductRequestValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Product name is required.")
            .MaximumLength(100).WithMessage("Product name cannot exceed 100 characters.");

        RuleFor(x => x.Brand)
            .NotEmpty().WithMessage("Brand is required.")
            .MaximumLength(50).WithMessage("Brand name cannot exceed 50 characters.");

        RuleFor(x => x.Description)
            .NotEmpty().WithMessage("Description is required.")
            .MaximumLength(500).WithMessage("Description cannot exceed 500 characters.");

        RuleFor(x => x.UnitPrice)
            .NotNull().WithMessage("Unit price is required.")
            .GreaterThan(0).WithMessage("Unit price must be greater than zero.");

        RuleFor(x => x.UnitQuantity)
            .NotNull().WithMessage("Unit quantity is required.")
            .GreaterThan(0).WithMessage("Unit quantity must be greater than zero.");

        RuleFor(x => x.Volume)
            .GreaterThan(0).WithMessage("Volume must be greater than zero.")
            .When(x => x.Volume.HasValue);

        RuleFor(x => x.Scent)
            .MaximumLength(100).WithMessage("Scent name cannot exceed 100 characters.");

        RuleFor(x => x.Edition)
            .MaximumLength(100).WithMessage("Edition name cannot exceed 100 characters.");

        RuleFor(x => x.Image)
            .NotEmpty().WithMessage("Image is required.")
            .Must(BeValidBase64).WithMessage("Invalid image format.")
            .When(x => !string.IsNullOrEmpty(x.Image));

        RuleFor(x => x.SubcategoryId)
            .NotEmpty().WithMessage("Subcategory ID is required.")
            .Must(id => id != Guid.Empty).WithMessage("Subcategory Id must not be an empty GUID."); ;
    }


    private bool BeValidBase64(string? image)
    {
        if (string.IsNullOrEmpty(image)) return false;

        var base64Pattern = @"^data:image\/(png|jpg|jpeg|gif);base64,([A-Za-z0-9+/]+={0,2})$";
        return Regex.IsMatch(image, base64Pattern);
    }
}
