using AgrosolutionsServiceIngestion.Application.Interfaces;
using AgrosolutionsServiceIngestion.Shared.DTOs.Request;
using Amazon.SimpleNotificationService;
using Amazon.SimpleNotificationService.Model;
using Microsoft.Extensions.Configuration;
using System.Text.Json;

namespace AgrosolutionsServiceIngestion.Infrastructure.Messaging;

public class SnsSensorRawPublisher : ISensorRawPublisher
{
    private readonly IAmazonSimpleNotificationService _snsClient;
    private readonly string _topicArn;

    public SnsSensorRawPublisher(IAmazonSimpleNotificationService snsClient, IConfiguration configuration)
    {
        _snsClient = snsClient;
        // Pega o ARN do tópico do appsettings (ex: "AWS:SnsTopicArn")
        _topicArn = configuration["AWS:SnsTopicArn"]
                    ?? throw new ArgumentNullException("AWS:SnsTopicArn não configurado no appsettings.");
    }

    public async Task PublishAsync(SensorRawRequest sensorRaw)
    {
        // Serializa o objeto para JSON
        var message = JsonSerializer.Serialize(sensorRaw);

        var request = new PublishRequest
        {
            TopicArn = _topicArn,
            Message = message
        };

        // Envia para o tópico SNS na AWS
        await _snsClient.PublishAsync(request);
    }
}