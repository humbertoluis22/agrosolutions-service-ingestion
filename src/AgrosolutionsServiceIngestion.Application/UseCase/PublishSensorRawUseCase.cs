using AgrosolutionsServiceIngestion.Application.Interfaces;
using AgrosolutionsServiceIngestion.Shared.Events;
using System;
using System.Collections.Generic;
using System.Text;

namespace AgrosolutionsServiceIngestion.Application.UseCase
{
    public class PublishSensorRawUseCase
    {
        private readonly ISensorRawPublisher _publisher;

        public PublishSensorRawUseCase(ISensorRawPublisher publisher)
        {
            _publisher = publisher;
        }

        public async Task ExecuteAsync(SensorRawCollectedEvent data)
        {
            await _publisher.PublishAsync(data);
        }
    }

}
