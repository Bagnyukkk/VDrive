using System.ComponentModel.DataAnnotations;

namespace VDriveWeb.Dtos
{
    public class FileActionDto
    {
        [Required]
        public Guid FileId { get; set; }
        public string FileName { get; set; } = string.Empty;
    }
}
