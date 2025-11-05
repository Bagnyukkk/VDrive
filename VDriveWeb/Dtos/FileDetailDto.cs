namespace VDriveWeb.Dtos
{
    public class FileDetailDto
    {
        public Guid FileId { get; set; }
        public string FileName { get; set; }
        public string FileType => Path.GetExtension(FileName);
        public DateTime CreationDate { get; set; }
        public DateTime ModificationDate { get; set; }
        public string UploaderName { get; set; }
        public string LastEditorName { get; set; }
    }
}