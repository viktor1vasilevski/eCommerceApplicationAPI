using Main.Enums;

namespace Main.Responses;

public class NonGenericApiResponse
{
    public bool Success { get; set; }
    public string? Message { get; set; }
    public Dictionary<string, List<string>>? Errors { get; set; }
    public NotificationType NotificationType { get; set; }
}
