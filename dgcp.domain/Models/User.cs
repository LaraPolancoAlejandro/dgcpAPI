using System.ComponentModel.DataAnnotations;

namespace dgcp.domain.Models;

public class User
{
    [Key]
    public Guid ID { get; set; }
    public string UserNameOrEmail { get; set; }
    public string Password { get; set; }
    public bool IsProjectAdmin { get; set; }
}
