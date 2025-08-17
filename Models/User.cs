using Finora.Kernel;
using Finora.Models;
using System.ComponentModel.DataAnnotations.Schema;

namespace Finora.Models
{
    internal class User : EntityBase
    {
        public string FirstName { get; set; } = string.Empty;
        public string? SecondName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        [NotMapped]
        public string FullName => FirstName + SecondName is not null ? $" {SecondName} " : " " +  LastName;
        public string Email { get; set; } = string.Empty;
        public DateTime DateOfBirth { get; set; }
        public string PasswordHash { get; set; } = string.Empty;
        public string PasswordSalt { get; set; } = string.Empty;
        public bool IsDeleted { get; set; } = true;
    }


}

namespace Finora.MocksGenerator
{
    internal partial class Mocks
    {
        public static List<User> GenerateMockUsers()
        {
            return new List<User>
            {
                new User
                {
                    FirstName = "Alice",
                    SecondName = "Marie",
                    LastName = "Smith",
                    Email = "alice.smith@example.com",
                    DateOfBirth = new DateTime(1990, 5, 14),
                    PasswordHash = "hash1",
                    PasswordSalt = "salt1",
                    IsDeleted = false
                },
                new User
                {
                    FirstName = "Bob",
                    SecondName = null,
                    LastName = "Johnson",
                    Email = "bob.johnson@example.com",
                    DateOfBirth = new DateTime(1985, 8, 22),
                    PasswordHash = "hash2",
                    PasswordSalt = "salt2",
                    IsDeleted = false
                },
                new User
                {
                    FirstName = "Charlie",
                    SecondName = "Lee",
                    LastName = "Brown",
                    Email = "charlie.brown@example.com",
                    DateOfBirth = new DateTime(1995, 12, 2),
                    PasswordHash = "hash3",
                    PasswordSalt = "salt3",
                    IsDeleted = false
                },
                new User
                {
                    FirstName = "Diana",
                    SecondName = "Rose",
                    LastName = "Taylor",
                    Email = "diana.taylor@example.com",
                    DateOfBirth = new DateTime(1992, 3, 10),
                    PasswordHash = "hash4",
                    PasswordSalt = "salt4",
                    IsDeleted = false
                },
                new User
                {
                    FirstName = "Edward",
                    SecondName = "James",
                    LastName = "Wilson",
                    Email = "edward.wilson@example.com",
                    DateOfBirth = new DateTime(1988, 7, 30),
                    PasswordHash = "hash5",
                    PasswordSalt = "salt5",
                    IsDeleted = false
                }
            };
        }
    }
}