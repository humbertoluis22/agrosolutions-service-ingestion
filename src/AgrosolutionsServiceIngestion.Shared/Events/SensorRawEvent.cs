using System;
using System.Collections.Generic;
using System.Text;
using AgrosolutionsServiceIngestion.Domain.Enums;

namespace AgrosolutionsServiceIngestion.Shared.Events
{
    public class SensorRawCollectedEvent
    {
        public Guid SensorId { get; set; }
        public Guid TalhaoId { get; set; }
        public SensorType Type { get; set; }
        public decimal Value { get; set; }
        public DateTime CollectedAt { get; set; }
    }
}
