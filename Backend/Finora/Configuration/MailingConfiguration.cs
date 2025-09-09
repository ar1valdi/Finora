namespace Finora.Web.Configuration;

public class MailingConfiguration {
    public Dictionary<string, MailingTemplate> Templates { get; set; } = new();
}

public class MailingTemplate {
    public string Subject { get; set; } = string.Empty;
    public string Body { get; set; } = string.Empty;
}
