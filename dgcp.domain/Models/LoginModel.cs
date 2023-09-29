using System.ComponentModel.DataAnnotations;

namespace dgcp.domain.Models;

public class LoginModel
{
    [Required(ErrorMessage = "El nombre de usuario o correo electrónico es requerido.")]
    public string UserNameOrEmail { get; set; }

    [Required(ErrorMessage = "La contraseña es requerida.")]
    [DataType(DataType.Password)]
    public string Password { get; set; }
}