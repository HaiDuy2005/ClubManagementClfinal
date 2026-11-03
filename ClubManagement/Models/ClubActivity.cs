
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ClubManagement.Models
{
    public class ClubActivity
    {
        [Key]
        public int ActivityId { get; set; }

        public string ActivityName { get; set; } = string.Empty;

        public string Description { get; set; } = string.Empty;

        public string Location { get; set; } = string.Empty;

        public DateTime ActivityDate { get; set; }

        public int ClubId { get; set; }

        [ForeignKey(nameof(ClubId))]
        public Club? Club { get; set; }

        public ICollection<Attendance> Attendances { get; set; }
            = new List<Attendance>();

        public ICollection<ActivityAttendance> ActivityAttendances { get; set; }
            = new List<ActivityAttendance>();
    }
}
