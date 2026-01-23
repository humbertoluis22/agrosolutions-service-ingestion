using AgrosolutionsServiceIngestion.Domain.Entities;
using AgrosolutionsServiceIngestion.Domain.Interfaces;
using AgrosolutionsServiceIngestion.Infrastructure.Contexts;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace AgrosolutionsServiceIngestion.Infrastructure.Repositories
{
    public class SensorRepository : ISensorRepository
    {
        private readonly IngestionDbContext _context;

        public SensorRepository(IngestionDbContext context)
        {
            _context = context;
        }

        public async Task AddAsync(Sensor sensor)
        {
            await _context.Sensors.AddAsync(sensor);
            await _context.SaveChangesAsync();
        }

        public async Task<bool> ExistsAsync(Guid sensorId)
        {
            return await _context.Sensors
                .AsNoTracking()
                .AnyAsync(x => x.Id == sensorId);
        }
    }
}
