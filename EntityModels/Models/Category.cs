using EntityModels.Models.Base;

namespace EntityModels.Models;

public class Category : AuditableBaseEntity
{
    public string Name { get; set; }

    public virtual ICollection<Subcategory>? Subcategories { get; set; }
}
