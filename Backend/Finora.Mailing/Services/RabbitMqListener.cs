using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Configuration;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using Finora.Backend.Models.Concrete;
using Finora.Messages.Mail;
using System.Net.Mail;

namespace Finora.Mailing.Services;

public class RabbitMqListener(
    IConfiguration configuration,
    ILogger<RabbitMqListener> logger
) : BackgroundService
{
    private JsonSerializerOptions jsonOptions => new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = false
    };

    private IConnection? _connection;
    private IChannel? _channel;
    private readonly string _exchangeName = configuration["RabbitMQ:ExchangeName"]!;
    private readonly string _routingKey = configuration["RabbitMQ:RoutingKey"]!;
    private readonly string _queueName = configuration["RabbitMQ:QueueName"]!;
    private readonly string _from = configuration["Mailing:From"]!;

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await InitializeRabbitMqAsync();
        
        logger.LogInformation("Mailing service started. Exchange={exchangeName}, routing key={routingKey}", _exchangeName, _routingKey);
        
        while (!stoppingToken.IsCancellationRequested)
        {
            await Task.Delay(1000, stoppingToken);
        }
    }

    private async Task InitializeRabbitMqAsync()
    {
        try
        {
            var factory = new ConnectionFactory()
            {
                HostName = configuration.GetSection("RabbitMQ").GetSection("Host").Value!,
                UserName = configuration.GetSection("RabbitMQ").GetSection("Username").Value!,
                Password = configuration.GetSection("RabbitMQ").GetSection("Password").Value!,
                Port = int.Parse(configuration.GetSection("RabbitMQ").GetSection("Port").Value!)
            };

            _connection = await factory.CreateConnectionAsync();
            _channel = await _connection.CreateChannelAsync();

            await _channel.ExchangeDeclareAsync(
                exchange: _exchangeName,
                type: ExchangeType.Direct,
                durable: true
            );
            await _channel.QueueDeclareAsync(
                queue: _queueName,
                durable: true,
                exclusive: false,
                autoDelete: false
            );
            await _channel.QueueBindAsync(
                queue: _queueName,
                exchange: _exchangeName,
                routingKey: _routingKey
            );

            var consumer = new AsyncEventingBasicConsumer(_channel);
            consumer.ReceivedAsync += async (model, ea) =>
            {
                await Handle(ea);
            };

            await _channel.BasicConsumeAsync(
                queue: _queueName,
                autoAck: false,
                consumer: consumer
            );

            logger.LogInformation("RabbitMQ listener initialized.");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to initialize RabbitMQ");
            throw;
        }
    }

    private async Task Handle(BasicDeliverEventArgs ea)
    {
        if (_channel == null)
        {
            throw new InvalidOperationException("Channel is not initialized");
        }

        try
        {
            var body = ea.Body.ToArray();
            var messageJson = Encoding.UTF8.GetString(body);

            logger.LogInformation("Received message from queue {QueueName}: {Message}", _queueName, messageJson);
            var envelope = JsonSerializer.Deserialize<MessageEnvelope>(messageJson, jsonOptions);

            if (envelope is null)  
            {
                throw new InvalidOperationException("Invalid message type");
            }

            MailRequest? message;
    
            if (envelope.Data is JsonElement jsonElement)
            {
                message = JsonSerializer.Deserialize<MailRequest>(jsonElement.GetRawText(), jsonOptions);
            }
            else if (envelope.Data is MailRequest directRequest)
            {
                message = directRequest;
            }
            else {
                throw new InvalidOperationException("Invalid message type");
            }

            if (message is null)
            {
                throw new InvalidOperationException("Failed to deserialize MailRequest");
            }


            await SendEmailToMailHog(message);
            await _channel.BasicAckAsync(ea.DeliveryTag, false);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error processing message from queue {QueueName}", _queueName);
            await _channel.BasicNackAsync(ea.DeliveryTag, false, false);
        }
    }

    private async Task SendEmailToMailHog(MailRequest mailRequest)
    {
        var mailHogHost = configuration["MailHog:Host"] ?? "localhost";

        var mailHogPort = int.Parse(configuration["MailHog:Port"] ?? "1025");

        logger.LogInformation("Sending email to {To} via MailHog at {Host}:{Port}", 
            mailRequest.To, mailHogHost, mailHogPort);

        using var client = new SmtpClient(mailHogHost, mailHogPort)
        {
            EnableSsl = false,
            UseDefaultCredentials = true
        };

        using var message = new MailMessage
        {
            From = new MailAddress(_from),
            Subject = mailRequest.Subject,
            Body = mailRequest.Body,
            IsBodyHtml = false
        };

        message.To.Add(mailRequest.To);

        await client.SendMailAsync(message);

        logger.LogInformation("Email sent successfully to {To}", mailRequest.To);
    }

    public override async Task StopAsync(CancellationToken cancellationToken)
    {
        logger.LogInformation("Stopping mailing service...");
        
        if (_channel != null)
        {
            await _channel.CloseAsync();
            _channel.Dispose();
        }

        if (_connection != null)
        {
            await _connection.CloseAsync();
            _connection.Dispose();
        }

        await base.StopAsync(cancellationToken);
    }
}
