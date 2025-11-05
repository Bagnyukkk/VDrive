using System.ComponentModel.DataAnnotations;

namespace VDriveWeb.Dtos
{
    public class LoginDto
    {
        [Required(ErrorMessage = "Введіть ім'я користувача")]
        public string Username { get; set; }

        [Required(ErrorMessage = "Введіть пароль")]
        [DataType(DataType.Password)]
        public string Password { get; set; }
    }

    public class RegisterDto : LoginDto
    {
        public string FullName { get; set; } = string.Empty;
    }

    public class LoginResponseDto
    {
        public string Token { get; set; }
    }
}
