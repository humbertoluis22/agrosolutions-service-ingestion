using AgrosolutionsServiceIngestion.Application.Interfaces;
using AgrosolutionsServiceIngestion.Shared.Events;
using RabbitMQ.Client;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;

namespace AgrosolutionsServiceIngestion.Infrastructure.Messaging
{
    public class RabbitMqSensorRawPublisher : ISensorRawPublisher
    {
        private readonly IModel _channel;

        public RabbitMqSensorRawPublisher(IModel channel)
        {
            _channel = channel;
        }

        public Task PublishAsync(SensorRawCollectedEvent data)
        {
            var body = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(data));

            _channel.BasicPublish(
                exchange: "sensor.raw.fanout",
                routingKey: "",
                basicProperties: null,
                body: body
            );

            return Task.CompletedTask;
        }
    }

}
