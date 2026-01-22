using AgrosolutionsServiceIngestion.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace AgrosolutionsServiceIngestion.Domain.Interfaces
{
    public interface ISensorRepository
    {
        Task AddAsync(Sensor sensor);
        Task<bool> ExistsAsync(Guid sensorId);
    }
}
