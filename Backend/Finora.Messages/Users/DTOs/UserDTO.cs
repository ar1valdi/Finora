using System.ComponentModel.DataAnnotations.Schema;

namespace Finora.Messages.Users.DTOs
{
    internal class UserDTO
    {
        public string FirstName { get; set; } = string.Empty;
        public string? SecondName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public DateTime DateOfBirth { get; set; }
        public bool IsDeleted { get; set; } = true;
    }
}
