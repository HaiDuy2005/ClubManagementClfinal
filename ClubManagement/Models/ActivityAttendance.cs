
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ClubManagement.Models
{
    public class ActivityAttendance
    {
        [Key]
        public int AttendanceId { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập họ tên")]
        public string FullName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Vui lòng nhập mã số sinh viên")]
        public string StudentCode { get; set; } = string.Empty;

        [Required(ErrorMessage = "Vui lòng nhập lớp")]
        public string ClassName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Vui lòng chọn hoạt động")]
        public int ActivityId { get; set; }

        [ForeignKey(nameof(ActivityId))]
        public ClubActivity? Activity { get; set; }

        public DateTime AttendanceDate { get; set; } = DateTime.Now;

        public string EvidenceObjectKey { get; set; } = string.Empty;

        public string EvidenceFileName { get; set; } = string.Empty;

        public string Status { get; set; } = "Pending";

        public int TrainingPoints { get; set; } = 0;

        public string AdminNote { get; set; } = string.Empty;
    }
}
