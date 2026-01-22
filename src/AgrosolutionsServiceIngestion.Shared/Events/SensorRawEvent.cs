using System;
using System.Collections.Generic;
using System.Text;
using AgrosolutionsServiceIngestion.Domain.Enums;

namespace AgrosolutionsServiceIngestion.Shared.Events
{
    public record SensorRawEvent(
        Guid TalhaoId,
        Guid SensorId,
        SensorType SensorType,
        DateTime CreatedAt
    );
}
