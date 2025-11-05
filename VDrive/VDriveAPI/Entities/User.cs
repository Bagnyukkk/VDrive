using System.ComponentModel.DataAnnotations;

namespace VDriveAPI.Entities
{
    public class User : BaseEntity
    {
        public string Username { get; set; }
        public string PasswordHash { get; set; }
        public string FullName { get; set; }
    }
}
