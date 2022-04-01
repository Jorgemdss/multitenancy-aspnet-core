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

            // services.AddSingleton<IDbContextSchema>(new DbContextSchema("alpha"));
            //services.AddSingleton<IDbContextSchema>(new DbContextSchema("beta"));
            services.AddSingleton<IDbContextSchema>(new DbContextSchema("charlie"));
            //services.AddSingleton<IDbContextSchema>(new DbContextSchema("java"));           
            

            if (defaultDbProvider.ToLower() == "mssql")
            {
                // TODO JS: meter dentro do foreach?
                services.AddDbContext<ApplicationDbContext>(builder =>
                       builder
                       .ReplaceService<ITenantService, TenantService>()
                       .UseSqlServer(e => e.MigrationsAssembly(typeof(ApplicationContextFactory).Assembly.FullName))
                       .UseSqlServer(e => e.MigrationsHistoryTable("__EFMigrationsHistory", "charlie"))
                       .ReplaceService<IDesignTimeDbContextFactory<DbContext>, ApplicationContextFactory>()
                       .ReplaceService<IModelCacheKeyFactory, DbSchemaAwareModelCacheKeyFactory>() //JG
                       .ReplaceService<IMigrationsAssembly, DbSchemaAwareMigrationAssembly>()
                    );
            }

            var tenants = options.Tenants;
            foreach (var tenant in tenants)
            {
                Console.WriteLine("Tenant name - " + tenant?.Name);
                Console.WriteLine("Tenant schema (TID) - " + tenant?.TID);
               
                string connectionString;
                if (string.IsNullOrEmpty(tenant.ConnectionString))
                {
                    connectionString = defaultConnectionString;
                }
                else
                {
                    connectionString = tenant.ConnectionString;
                }
                Console.WriteLine("Connection string - " + connectionString);

                using var scope = services.BuildServiceProvider().CreateScope();
                var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                dbContext.Database.SetConnectionString(connectionString);

                //dbContext.TenantId = tenant.TID;          
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
