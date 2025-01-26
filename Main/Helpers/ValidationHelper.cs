using FluentValidation;
using Main.Enums;
using Main.Responses;

namespace Main.Helpers
{
    public static class ValidationHelper
    {
        public static ApiResponse<TResponse> ValidateRequest<TRequest, TResponse>(
            TRequest request,
            IValidator<TRequest> validator)
            where TRequest : class
            where TResponse : class
        {
            var validationResult = validator.Validate(request);

            if (!validationResult.IsValid)
            {
                var errors = validationResult.Errors
                    .GroupBy(error => error.PropertyName, StringComparer.OrdinalIgnoreCase)
                    .ToDictionary(
                        group => group.Key,
                        group => group.Select(error => error.ErrorMessage).ToList()
                    );

                return new ApiResponse<TResponse>
                {
                    Success = false,
                    NotificationType = NotificationType.BadRequest,
                    Message = "Validation failed.",
                    Errors = errors
                };
            }

            return null;
        }
    }
}
