using AgrosolutionsServiceIngestion.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace AgrosolutionsServiceIngestion.Infrastructure.Contexts
{
    public class IngestionDbContext : DbContext
    {
        public IngestionDbContext(DbContextOptions<IngestionDbContext> options)
            : base(options) { }

        public DbSet<Sensor> Sensors => Set<Sensor>();
        public DbSet<Talhao> Talhoes => Set<Talhao>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.ApplyConfigurationsFromAssembly(
                typeof(IngestionDbContext).Assembly
            );
        }
    }
}
