namespace Finora.Kernel;

public class MessageMetadata {
    public Type Type { get; set; } = typeof(object);
    public bool IsTransactional { get; set; }

    public MessageMetadata(Type type, bool isTransactional) {
        Type = type;
        IsTransactional = isTransactional;
    }
}