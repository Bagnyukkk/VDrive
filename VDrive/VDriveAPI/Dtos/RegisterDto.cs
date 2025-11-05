namespace VDriveAPI.Dtos
{
    public class RegisterDto
    {
        public required string Username { get; set; }
        public required string Password { get; set; }
        public string FullName { get; set; } = string.Empty;
    }
}
