using dgcp.domain.Models;

namespace dgcp.domain.Abstractions;

public interface IUsersService
{
    string GenerateJwtToken(User user);
}