using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Data;
using dgcp.domain.Models;
using System.Security.Claims;
using dgcp.infrastructure;
using Microsoft.EntityFrameworkCore;
using dgcp.infrastructure.Services;
using dgcp.domain.Abstractions;


namespace dgcp.api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UsersController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly IUsersService _usersService;

        public UsersController(AppDbContext context, IUsersService usersService)
        {
            _context = context;
            _usersService = usersService;
        }

        [HttpPost("register")]
        [Authorize]
        public async Task<IActionResult> Register([FromBody] RegisterModel model)
        {
            // Obtener el UserNameOrEmail del usuario actual desde los claims
            var userNameOrEmail = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrEmpty(userNameOrEmail))
            {
                // Retornar un error si el UserNameOrEmail del usuario no es válido
                return BadRequest("Invalid user name or email");
            }

            // Verificar si el usuario actual es administrador
            var currentUser = await _context.Users.SingleOrDefaultAsync(u => u.UserNameOrEmail == userNameOrEmail);
            if (currentUser == null || !currentUser.IsProjectAdmin)
                return Forbid();

            // Crear nuevo usuario
            var user = new User
            {
                ID = Guid.NewGuid(),
                UserNameOrEmail = model.UserNameOrEmail,
                Password = BCrypt.Net.BCrypt.HashPassword(model.Password), // Asegúrate de usar un método de hash seguro
                IsProjectAdmin = model.IsProjectAdmin
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            return Ok();
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginModel model)
        {
            var user = await _context.Users.SingleOrDefaultAsync(u => u.UserNameOrEmail == model.UserNameOrEmail);
            if (user == null || !BCrypt.Net.BCrypt.Verify(model.Password, user.Password))
                return Unauthorized(); // Usuario no encontrado o contraseña incorrecta

            // Generar el token JWT
            var token = _usersService.GenerateJwtToken(user); // Asume que tienes un método que genera el token

            // Devolver el token en la respuesta
            return Ok(new { Token = token });
        }

    }
}
