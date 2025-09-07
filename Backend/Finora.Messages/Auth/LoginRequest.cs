using Finora.Messages.Interfaces;

namespace Finora.Messages.Auth
{
    public class LoginRequest : IQuery
    {
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
    }
}