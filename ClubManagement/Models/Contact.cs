using System.ComponentModel.DataAnnotations;

namespace ClubManagement.Models
{
    public class Contact
    {
        public int ContactId { get; set; }

        public string Name { get; set; } = "";

        public string Email { get; set; } = "";

        public string Message { get; set; } = "";

        public bool IsAnonymous { get; set; }

        public DateTime CreatedAt { get; set; }
    }
}