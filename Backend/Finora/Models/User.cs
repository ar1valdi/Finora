using Finora.Kernel;
using Finora.Models.Enums;
using System.ComponentModel.DataAnnotations.Schema;

namespace Finora.Models
{
    public class User : EntityBase
    {
        public string FirstName { get; set; } = string.Empty;
        public string? SecondName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        [NotMapped]
        public string FullName => FirstName + SecondName is not null ? $" {SecondName} " : " " +  LastName;
        public string Email { get; set; } = string.Empty;
        public DateTime DateOfBirth { get; set; }
        public string PasswordHash { get; set; } = string.Empty;
        public bool IsDeleted { get; set; } = true;
        public Role Role { get; set; } = Role.User;
        public BankAccount? BankAccount { get; set; }
        public Guid? BankAccountId { get; set; }
    }
}