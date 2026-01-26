using AgrosolutionsServiceIngestion.Shared.Events;
using System;
using System.Collections.Generic;
using System.Text;

namespace AgrosolutionsServiceIngestion.Application.Interfaces
{
    public interface ISensorRawPublisher
    {
        Task PublishAsync(SensorRawCollectedEvent data);
    }
}
