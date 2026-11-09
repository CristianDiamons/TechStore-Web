using System.ComponentModel.DataAnnotations;

namespace TechStoreMVC.Models
{
    public class LoginViewModel
    {
        [Required(ErrorMessage = "Ingresa tu usuario.")]
        [Display(Name = "Usuario")]
        public string Usuario { get; set; } = string.Empty;

        [Required(ErrorMessage = "Ingresa tu contraseña.")]
        [DataType(DataType.Password)]
        [Display(Name = "Contraseña")]
        public string Contrasena { get; set; } = string.Empty;

        public bool Recordarme { get; set; }
    }
}
