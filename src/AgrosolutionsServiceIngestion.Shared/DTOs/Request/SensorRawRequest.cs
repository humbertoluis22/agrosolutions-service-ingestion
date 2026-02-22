using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json.Nodes;
using AgrosolutionsServiceIngestion.Shared.Enums;

namespace AgrosolutionsServiceIngestion.Shared.DTOs.Request
{
    public class SensorRawRequest
    {
        public Guid FieldId { get; set; }
        public Guid SensorId { get; set; }
        public JsonNode Data { get; set; } // Recebe como Node para validação manual
        public SensorType TypeSensor { get; set; }

        public DateTime TimeStamp { get; set; }
    }
}
