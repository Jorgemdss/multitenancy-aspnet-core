﻿using Core.Interfaces;
using Core.Options;
using Infrastructure.Persistence;
using Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Linq;

namespace Infrastructure.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddAndMigrateTenantDatabases(this IServiceCollection services, IConfiguration config)
        {
            var options = services.GetOptions<TenantSettings>(nameof(TenantSettings));
            var defaultConnectionString = options.Defaults?.ConnectionString;
            var defaultDbProvider = options.Defaults?.DBProvider;

            // if (defaultDbProvider.ToLower() == "mssql")
            // {
            //     services.AddDbContext<ApplicationDbContext>( builder => 
            //             builder
            //             .UseSqlServer(e => e.MigrationsAssembly(typeof(ApplicationDbContext).Assembly.FullName))
            //             .ReplaceService<IModelCacheKeyFactory, DbSchemaAwareModelCacheKeyFactory>()
            //             .ReplaceService<IDesignTimeDbContextFactory<DbContext>, ApplicationContextFactory>());
            // }

            var tenants = options.Tenants;
            foreach (var tenant in tenants)
            {
                var descriptor = services.SingleOrDefault(
                             d => d.ServiceType ==
                                 typeof(DbContextOptions<ApplicationDbContext>));

                if (descriptor != null)
                {
                    services.Remove(descriptor);
                }
                
                string connectionString;
                if (string.IsNullOrEmpty(tenant.ConnectionString))
                {
                    connectionString = defaultConnectionString;
                }
                else
                {
                    connectionString = tenant.ConnectionString;
                }

                Console.WriteLine("Tenant name - " + tenant?.Name);
                Console.WriteLine("Tenant schema (TID) - " + tenant?.TID);

                //services.AddSingleton<IDbContextSchema>(new DbContextSchema(tenant?.TID));
                services.AddTransient<IDbContextSchema>(f => new DbContextSchema(tenant?.TID));

                if (defaultDbProvider.ToLower() == "mssql")
                {
                    services.AddDbContext<ApplicationDbContext>(builder =>
                            builder
                            //.ReplaceService<ITenantService, TenantService>()
                            .UseSqlServer(connectionString)
                            .UseSqlServer(e => e.MigrationsAssembly(typeof(ApplicationDbContext).Assembly.FullName))
                            .UseSqlServer(e => e.MigrationsHistoryTable("__EFMigrationsHistory", tenant?.TID))
                            .ReplaceService<IDesignTimeDbContextFactory<DbContext>, ApplicationContextFactory>()
                            .ReplaceService<IModelCacheKeyFactory, DbSchemaAwareModelCacheKeyFactory>()                            
                            .ReplaceService<IMigrationsAssembly, DbSchemaAwareMigrationAssembly>());
                }


                using var scope = services.BuildServiceProvider().CreateScope();
                var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                //dbContext.Database.SetConnectionString(connectionString);

                Console.WriteLine("--DB context schema: " + dbContext.TenantId);

                if (dbContext.Database.GetMigrations().Count() > 0)
                {
                    dbContext.Database.Migrate();
                }
            }
            return services;
        }
        public static T GetOptions<T>(this IServiceCollection services, string sectionName) where T : new()
        {
            using var serviceProvider = services.BuildServiceProvider();
            var configuration = serviceProvider.GetRequiredService<IConfiguration>();
            var section = configuration.GetSection(sectionName);
            var options = new T();
            section.Bind(options);
            return options;
        }
    }
}
