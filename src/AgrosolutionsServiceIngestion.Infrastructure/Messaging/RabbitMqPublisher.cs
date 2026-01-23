using AgrosolutionsServiceIngestion.Application.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;
using RabbitMQ.Client;


namespace AgrosolutionsServiceIngestion.Infrastructure.Messaging
{
    public class RabbitMqPublisher : IMessageBus
    {
        private readonly IModel _channel;

        public RabbitMqPublisher(IConnection connection)
        {
            _channel = connection.CreateModel();
            _channel.ExchangeDeclare(
                "sensor.raw.exchange",
                ExchangeType.Fanout,
                durable: true
            );
        }

        public void Publish<T>(T message)
        {
            var body = JsonSerializer.SerializeToUtf8Bytes(message);

            _channel.BasicPublish(
                exchange: "sensor.raw.exchange",
                routingKey: "",
                body: body
            );
        }
    }
}
