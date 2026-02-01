using AgrosolutionsServiceIngestion.Shared.DTOs.Request;
using System;
using System.Collections.Generic;
using System.Text;

namespace AgrosolutionsServiceIngestion.Application.Interfaces
{
    public interface ISensorRawPublisher
    {
        Task PublishAsync(SensorRawRequest data);
    }
}
