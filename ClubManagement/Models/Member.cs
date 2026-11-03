
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace ClubManagement.Models
{
    public class Member
    {
        public int MemberId { get; set; }

        public string FullName { get; set; } = string.Empty;

        public string StudentCode { get; set; } = string.Empty;

        public string ClassName { get; set; } = string.Empty;

        public string Email { get; set; } = string.Empty;

        public string Phone { get; set; } = string.Empty;

        public string Position { get; set; } = string.Empty;

        public int TrainingPoints { get; set; } = 0;

        public int ClubId { get; set; }

        [ForeignKey(nameof(ClubId))]
        public Club? Club { get; set; }

        public ICollection<Attendance> Attendances { get; set; }
            = new List<Attendance>();
    }
}
