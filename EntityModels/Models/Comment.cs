using EntityModels.Models.Base;

namespace EntityModels.Models;

public class Comment : AuditableBaseEntity
{
    public Guid UserId { get; set; }
    public Guid ProductId { get; set; }
    public string? Content { get; set; }


    public virtual User? User { get; set; }
    public virtual Product? Product { get; set; }
}

