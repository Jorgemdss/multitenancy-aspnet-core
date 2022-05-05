using Core.Contracts;
using Core.Entities;
using Core.Interfaces;
using Core.Options;
using Infrastructure.Extensions;
using Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Infrastructure.Persistence
{
    public interface IDbContextSchema
    {
        string TenantId { get; }
    }

    public class ApplicationDbContext : DbContext, IDbContextSchema
    {
        public string TenantId { get; set; }
        private readonly ITenantService _tenantService;

        // public ApplicationDbContext(DbContextOptions options) : base(options)
        // {
        //     _tenantService = new TenantService();
        //     TenantId = "dbo";
        // }

        // public ApplicationDbContext(
        //     DbContextOptions<ApplicationDbContext> options,
        //     IDbContextSchema schema)
        //     : base(options)
        // {
        //     _tenantService = new TenantService();
        //     TenantId = schema.TenantId;
        // }

        public ApplicationDbContext(DbContextOptions options, ITenantService tenantService, IDbContextSchema schema) : base(options)
        {
            TenantId = schema.TenantId;
            if (tenantService == null)
            {
                _tenantService = new TenantService();
            }
            else
            {

                _tenantService = tenantService;
                var tid = tenantService.GetTenant()?.TID;
                TenantId = tid != null ? tid : schema.TenantId;
            }            
        }

        public DbSet<Product> Products { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            //modelBuilder.Entity<Product>().HasQueryFilter(a => a.TenantId == TenantId);            
            modelBuilder.Entity<Product>().ToTable(nameof(Products), TenantId);
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            var tenantConnectionString = _tenantService?.GetConnectionString();
            if (!string.IsNullOrEmpty(tenantConnectionString))
            {
                var DBProvider = _tenantService.GetDatabaseProvider();
                if (DBProvider.ToLower() == "mssql")
                {
                    optionsBuilder
                        .UseSqlServer(_tenantService.GetConnectionString())
                        .LogTo(Console.WriteLine);

                }
            }
        }

        public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = new CancellationToken())
        {
            foreach (var entry in ChangeTracker.Entries<IMustHaveTenant>().ToList())
            {
                switch (entry.State)
                {
                    case EntityState.Added:
                    case EntityState.Modified:
                        entry.Entity.TenantId = TenantId;
                        break;
                }
            }
            var result = await base.SaveChangesAsync(cancellationToken);
            return result;
        }
    }
}