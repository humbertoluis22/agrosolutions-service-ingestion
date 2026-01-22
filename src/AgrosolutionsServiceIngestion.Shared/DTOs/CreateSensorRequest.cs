using AgrosolutionsServiceIngestion.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace AgrosolutionsServiceIngestion.Shared.DTOs
{
    public record CreateSensorRequest(
        Guid TalhaoId,
        Guid SensorId,
        SensorType SensorType
    );
}
