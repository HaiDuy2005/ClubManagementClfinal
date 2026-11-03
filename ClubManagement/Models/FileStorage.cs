
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ClubManagement.Models
{
    public class FileStorage
    {
        [Key]
        public int FileId { get; set; }

        public string FileName { get; set; } = string.Empty;

        public string ObjectKey { get; set; } = string.Empty;

        public string FileType { get; set; } = string.Empty;

        public long FileSize { get; set; }

        public DateTime UploadDate { get; set; }

        public int ActivityId { get; set; }

        [ForeignKey(nameof(ActivityId))]
        public ClubActivity? Activity { get; set; }
    }
}
