namespace Main.Requests;

public class BaseRequest
{
    public int? Skip { get; set; }
    public int? Take { get; set; }
    public string? SortBy { get; set; }
    public string? SortDirection { get; set; }
}
