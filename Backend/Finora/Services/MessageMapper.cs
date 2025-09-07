using System.Text.Json;
using Finora.Messages.Auth;
using Finora.Messages.Users;
using Finora.Messages.Interfaces;

namespace Finora.Backend.Services;

public static class MessageMapper
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = false
    };

    private static readonly Dictionary<string, Type> _messageMap = new Dictionary<string, Type> 
    {
        {"GetAllUsers", typeof(GetAllUsersRequest)},
        {"AddUser", typeof(AddUserRequest)},
        {"UpdateUser", typeof(UpdateUserRequest)},
        {"DeleteUser", typeof(DeleteUserRequest)},
        {"GetUser", typeof(GetUserRequest)},
        {"Login", typeof(LoginRequest)}
    };

    private static Type Map(string message)
    {
        if (_messageMap.TryGetValue(message, out var type)) {
            return type;
        } else {
            throw new Exception($"Unknown message type: {message}");
        }
    }

    public static IMessage DeserializeMessage(string messageType, string jsonData)
    {
        var type = Map(messageType);
        return JsonSerializer.Deserialize(jsonData, type, JsonOptions) as IMessage
            ?? throw new InvalidOperationException($"Failed to deserialize {messageType} from JSON");
    }
}