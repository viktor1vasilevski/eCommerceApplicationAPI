using EntityModels.Enums;
using EntityModels.Models.Base;

namespace EntityModels.Models;

public class User : AuditableBaseEntity
{
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public string Username { get; set; }
    public Role Role { get; set; }
    public string Email { get; set; }
    public string PasswordHash { get; set; }
    public string SaltKey { get; set; }


    public virtual ICollection<Order>? Orders { get; set; }
    public virtual ICollection<UserBasket>? UserBaskets { get; set; }
    public virtual ICollection<Comment>? Comments { get; set; }
}
