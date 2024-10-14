using System;
using System.Diagnostics;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Simulation.Business.Dal;
using Simulation.Business.Dal.Tables;

namespace SimulationServer.Business.Dal;

public sealed class SimulationDbContext : DbContext
{
    private readonly ILoggerFactory _loggerFactory;

    public SimulationDbContext(DbContextOptions options, ILoggerFactory loggerFactory) : base(options)
    {
        _loggerFactory = loggerFactory;
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<ActorRow>().ToTable("Actor").HasKey(e => e.Id);
        modelBuilder.Entity<ActorRow>().Property(e => e.Name).HasColumnName("Name");
        modelBuilder.Entity<ActorRow>().Property(e => e.Informations).HasColumnName("Informations");
        modelBuilder.Entity<ActorRow>().Property(e => e.InsertTime).HasColumnName("InsertTime");
        modelBuilder.Entity<ActorRow>().Property(e => e.ModifiedAt).HasColumnName("ModifiedAt");


        modelBuilder.Entity<SensorDataRow>().ToTable("SensorData").HasKey(e => e.Id);
        modelBuilder.Entity<SensorDataRow>().Property(e => e.SensorId).HasColumnName("SensorId");
        modelBuilder.Entity<SensorDataRow>().Property(e => e.ActorId).HasColumnName("ActorId");
        modelBuilder.Entity<SensorDataRow>().Property(e => e.TimeStamp).HasColumnName("TimeStamp");
        modelBuilder.Entity<SensorDataRow>().Property(e => e.Coordinates).HasColumnName("Coordinates");


        modelBuilder.Entity<StackRow>().ToTable("Stack").HasKey(e => e.Id);
        modelBuilder.Entity<StackRow>().Property(e => e.Value).HasColumnName("Value");
        modelBuilder.Entity<StackRow>().Property(e => e.InsertTime).HasColumnName("InsertedAt");
    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        //// Allow null if you are using an IDesignTimeDbContextFactory
        /*  if (_loggerFactory != null && Debugger.IsAttached)
         {
             //Probably shouldn't log sql statements in production
             optionsBuilder.UseLoggerFactory(_loggerFactory);
             optionsBuilder.EnableSensitiveDataLogging();
         }
         base.OnConfiguring(optionsBuilder); */
    }
    public DbSet<ActorRow> Actors { get; set; }
    public DbSet<SensorDataRow> SensorsData { get; set; }

    public DbSet<StackRow> Stack { get; set; }

}


