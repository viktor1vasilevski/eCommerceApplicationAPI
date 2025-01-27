using EntityModels.Models.Base;

namespace EntityModels.Models;

public class Order : AuditableBaseEntity
{
    public Guid ProductId { get; set; }
    public Guid UserId { get; set; }


    public virtual Product? Product { get; set; }
    public virtual User? User { get; set; }
}
