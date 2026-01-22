using AgrosolutionsServiceIngestion.Application.Interfaces;
using AgrosolutionsServiceIngestion.Domain.Entities;
using AgrosolutionsServiceIngestion.Domain.Interfaces;
using AgrosolutionsServiceIngestion.Shared.DTOs;
using AgrosolutionsServiceIngestion.Shared.Events;
using System;
using System.Collections.Generic;
using System.Text;

namespace AgrosolutionsServiceIngestion.Application.Handlers
{
    public class CreateSensorHandler
    {
        private readonly ISensorRepository _repository;
        private readonly IMessageBus _bus;

        public CreateSensorHandler(
            ISensorRepository repository,
            IMessageBus bus)
        {
            _repository = repository;
            _bus = bus;
        }

        public async Task ExecuteAsync(CreateSensorRequest request)
        {
            if (await _repository.ExistsAsync(request.SensorId))
                return;

            var sensor = new Sensor(
                request.SensorId,
                request.TalhaoId,
                request.SensorType
            );

            await _repository.AddAsync(sensor);

            var evt = new SensorRawEvent(
                request.TalhaoId,
                request.SensorId,
                request.SensorType,
                DateTime.UtcNow
            );

            _bus.Publish(evt);
        }
    }
}
