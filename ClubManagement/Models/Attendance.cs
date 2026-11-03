
using System.ComponentModel.DataAnnotations.Schema;

namespace ClubManagement.Models
{
    public class Attendance
    {
        public int AttendanceId { get; set; }

        public int MemberId { get; set; }

        public int ActivityId { get; set; }

        public bool Status { get; set; }

        [ForeignKey(nameof(MemberId))]
        public Member? Member { get; set; }

        [ForeignKey(nameof(ActivityId))]
        public ClubActivity? Activity { get; set; }
    }
}
