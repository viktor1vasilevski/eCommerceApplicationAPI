using EntityModels.Models.Base;

namespace EntityModels.Models;

public class Subcategory : AuditableBaseEntity
{
    public string Name { get; set; }
    public Guid CategoryId { get; set; }

    public virtual Category? Category { get; set; }
}
