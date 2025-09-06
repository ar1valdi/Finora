using Finora.Messages.Interfaces;

namespace Finora.Messages.Users
{
    public class AddUserRequest : ICommand
    {
        public Guid Id { get; set; }
        public string FirstName { get; set; } = string.Empty;
        public string? SecondName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public DateTime DateOfBirth { get; set; }
        public string Password { get; set; } = string.Empty;
    }
}
