﻿using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using react_app.Services;
using react_app.Wmprojack.Entities;

namespace react_app.Wmprojack
{
    public class WmprojackDbContext : DbContext
    {
        public WmprojackDbContext(DbContextOptions<WmprojackDbContext> options) : base(options)
        {

        }

        public DbSet<Order> Orders { get; set; }
        public DbSet<Log> Logs { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Log>().HasNoKey().ToView(null);

            modelBuilder.Entity<Order>()
                .Property(p => p.Id)
                .ValueGeneratedOnAdd();

            var converter = new EnumToStringConverter<OrderProvider>();
            modelBuilder
                .Entity<Order>()
                .Property(e => e.ProviderType)
                .HasConversion(converter);
        }
    }
}