using System.ComponentModel.DataAnnotations.Schema;

namespace VDriveAPI.Entities
{
    public class FileMetaData : BaseEntity
    {
        public string FileName { get; set; }
        public string FileType { get; set; }
        public DateTime CreationDate { get; set; }
        public DateTime ModificationDate { get; set; }
        public string StoragePath { get; set; }

        public Guid UploaderId { get; set; }
        public Guid LastEditorId { get; set; }

        [ForeignKey("UploaderId")]
        public virtual User Uploader { get; set; } = null!;

        [ForeignKey("LastEditorId")]
        public virtual User LastEditor { get; set; } = null!;
    }
}
