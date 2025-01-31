using Main.Enums;

namespace Main.Responses;

public class ApiResponse<T>: NonGenericApiResponse where T : class
{
    public T? Data { get; set; }
    public int? TotalCount { get; set; }
}
