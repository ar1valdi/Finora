using System.Text.Json;
using Finora.Messages.Auth;
using Finora.Messages.Users;
using Finora.Messages.Interfaces;
using Finora.Backend.Common;
using Finora.Messages.Banking;
using Finora.Messages.System;

namespace Finora.Backend.Services;

public static class MessageMapper
{
    private static readonly Dictionary<string, Type> _messageMap = new Dictionary<string, Type> 
    {
        // auth
        {"Login", typeof(LoginRequest)},

        // users
        {"AddUser", typeof(AddUserRequest)},
        {"UpdateUser", typeof(UpdateUserRequest)},
        {"DeleteUser", typeof(DeleteUserRequest)},
        {"GetUser", typeof(GetUserRequest)},
        {"GetAllUsers", typeof(GetAllUsersRequest)},

        // banking
        {"DepositWithdrawl", typeof(DepositWithdrawlRequest)},
        {"TransferMoney", typeof(TransferMoneyRequest)},
        {"GetUserTransactions", typeof(GetUserTransactionsRequest)},

        // health
        {"HealthCheck", typeof(HealthCheckRequest)}
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
        return JsonSerializer.Deserialize(jsonData, type, JsonConfig.JsonOptions) as IMessage
            ?? throw new InvalidOperationException($"Failed to deserialize {messageType} from JSON");
    }
}