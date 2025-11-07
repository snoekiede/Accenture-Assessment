using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Accenture_Assessment.Contracts.Enums;
using Accenture_Assessment.Data.Models;
using Microsoft.EntityFrameworkCore;

namespace Accenture_Assessment.Data.Contexts
{
    public class HolidayDbContext : DbContext
    {
        public HolidayDbContext(DbContextOptions<HolidayDbContext> options) : base(options)
        {
        }
        public DbSet<Holiday> Holidays { get; set; }
        public DbSet<Country> Countries { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Country>(entity =>
                {
                    entity.HasKey(c => c.Id);
                    entity.Property(c => c.Name).IsRequired().HasMaxLength(200);
                    entity.Property(c => c.Code).IsRequired().HasMaxLength(10);
                    entity.HasIndex(c => c.Code).IsUnique();
                }
            );

            modelBuilder.Entity<Holiday>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Name).IsRequired().HasMaxLength(200);
                entity.Property(e => e.LocalName).IsRequired().HasMaxLength(200);
                entity.Property(e => e.CountryCode).HasMaxLength(10);
                entity.Property(e => e.Date).IsRequired();

                // Store Counties as JSON
                entity.Property(e => e.Counties)
                    .HasConversion(
                        v => System.Text.Json.JsonSerializer.Serialize(v, (System.Text.Json.JsonSerializerOptions)null),
                        v => System.Text.Json.JsonSerializer.Deserialize<List<string>>(v, (System.Text.Json.JsonSerializerOptions)null) ?? new List<string>());

                // Store Types as JSON
                entity.Property(e => e.Type)
                    .HasConversion(
                        v => System.Text.Json.JsonSerializer.Serialize(v, (System.Text.Json.JsonSerializerOptions?)null),
                        v => System.Text.Json.JsonSerializer.Deserialize<HolidayType?>(v, (System.Text.Json.JsonSerializerOptions?)null) ?? HolidayType.Public);

                entity.HasIndex(e => new { e.CountryCode, e.Date });
            });
        }
    }
}
