using AgrosolutionsServiceIngestion.Application.Interfaces;
using AgrosolutionsServiceIngestion.Shared.DTOs.Request;
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

        public async Task ExecuteAsync(SensorRawRequest data)
        {
            await _publisher.PublishAsync(data);
        }
    }

}
