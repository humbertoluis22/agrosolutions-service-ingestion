using AgrosolutionsServiceIngestion.Application.Interfaces;
using AgrosolutionsServiceIngestion.Shared.DTOs.Request;
using RabbitMQ.Client;
using System.Text;
using System.Text.Json;

namespace AgrosolutionsServiceIngestion.Infrastructure.Messaging
{
    public class RabbitMqSensorRawPublisher : ISensorRawPublisher
    {
        private readonly IConnection _connection;
        private const string ExchangeName = "sensor.raw.fanout";

        public RabbitMqSensorRawPublisher(IConnection connection)
        {
            _connection = connection;
        }

        public Task PublishAsync(SensorRawRequest data)
        {
            // Channel NÃO é thread-safe → criar por uso
            using var channel = _connection.CreateModel();

            channel.ExchangeDeclare(
                exchange: ExchangeName,
                type: ExchangeType.Fanout,
                durable: true
            );

            var body = Encoding.UTF8.GetBytes(
                JsonSerializer.Serialize(data)
            );

            channel.BasicPublish(
                exchange: ExchangeName,
                routingKey: "",
                basicProperties: null,
                body: body
            );

            return Task.CompletedTask;
        }
    }
}
