using AgrosolutionsServiceIngestion.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace AgrosolutionsServiceIngestion.Domain.Entities
{
    public class Sensor
    {
        public Guid Id { get; private set; }
        public Guid TalhaoId { get; private set; }
        public SensorType Type { get; private set; }
        public bool Active { get; private set; }

        private Sensor() { }

        public Sensor(Guid id, Guid talhaoId, SensorType type)
        {
            Id = id;
            TalhaoId = talhaoId;
            Type = type;
            Active = true;
        }

        public void Disable() => Active = false;
    }
}
