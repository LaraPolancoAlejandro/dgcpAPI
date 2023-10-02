namespace dgcp.domain.Models;

public class RegisterModel
{
    public string UserNameOrEmail { get; set; }
    public string Password { get; set; }
    public bool IsProjectAdmin { get; set; }
}
