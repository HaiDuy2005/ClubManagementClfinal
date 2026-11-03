using System;
using System.ComponentModel.DataAnnotations;

namespace ClubManagement.Models
{
    public class MemberRegistration
    {
        [Key]
        public int RegistrationId { get; set; }

        public string FullName { get; set; } = string.Empty;

        public string StudentCode { get; set; } = string.Empty;

        public string ClassName { get; set; } = string.Empty;

        public string Phone { get; set; } = string.Empty;

        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Vui lòng nhập lý do tham gia CLB")]
        public string Reason { get; set; } = string.Empty;

        public string Status { get; set; } = "Pending";

        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }
}
