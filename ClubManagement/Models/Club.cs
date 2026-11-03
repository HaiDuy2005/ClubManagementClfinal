
using System.Collections.Generic;

namespace ClubManagement.Models
{
    public class Club
    {
        public int ClubId { get; set; }

        public string ClubName { get; set; } = string.Empty;

        public string Description { get; set; } = string.Empty;

        public DateTime CreatedDate { get; set; }

        public ICollection<Member> Members { get; set; }
            = new List<Member>();

        public ICollection<ClubActivity> Activities { get; set; }
            = new List<ClubActivity>();
    }
}
