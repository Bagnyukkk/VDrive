using Newtonsoft.Json;

namespace VDriveDesktop.Models
{
    public class FileDetailDto
    {
        [JsonProperty("fileId")]
        public Guid FileId { get; set; }

        [JsonProperty("fileName")]
        public string FileName { get; set; }

        [JsonProperty("creationDate")]
        public DateTime CreationDate { get; set; }

        [JsonProperty("modificationDate")]
        public DateTime ModificationDate { get; set; }

        [JsonProperty("uploaderName")]
        public string UploaderName { get; set; }

        [JsonProperty("lastEditorName")]
        public string LastEditorName { get; set; }
    }
}
