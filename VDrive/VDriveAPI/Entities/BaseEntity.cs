using System.ComponentModel.DataAnnotations;

namespace VDriveAPI.Entities
{
    public class BaseEntity
    {
        [Key]
        public Guid Id { get; set; }
    }
}
