
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ClubManagement.Models
{
    public class ActivityRegistration
    {
        [Key]
        public int RegistrationId { get; set; }

        public string FullName { get; set; } = string.Empty;

        public string ClassName { get; set; } = string.Empty;

        public string StudentCode { get; set; } = string.Empty;

        public string Email { get; set; } = string.Empty;

        public int ActivityId { get; set; }

        [ForeignKey(nameof(ActivityId))]
        public ClubActivity? Activity { get; set; }

        public string ActivityName { get; set; } = string.Empty;

        public DateTime ActivityDate { get; set; }

        public bool IsApproved { get; set; } = false;
    }
}
