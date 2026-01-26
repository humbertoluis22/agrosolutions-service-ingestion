using AgrosolutionsServiceIngestion.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace AgrosolutionsServiceIngestion.Domain.Entities
{
    public class SensorRawData
    {
        public Guid SensorId { get; set; }
        public Guid TalhaoId { get; set; }
        public decimal Value { get; set; }
        public DateTime CollectedAt { get; set; }
        public SensorType Type { get; set; }
    }

}
