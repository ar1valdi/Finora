using Finora.Messages.Interfaces;

namespace Finora.Messages.Mail;

public class MailRequest : IMessage {
    public string Subject { get; set; } = string.Empty;
    public string Body { get; set; } = string.Empty;
    public string To { get; set; } = string.Empty;
}