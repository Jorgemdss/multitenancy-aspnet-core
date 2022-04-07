using Core.Interfaces;
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
using System.Threading.Tasks;

namespace Infrastructure.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddAndMigrateTenantDatabases(this IServiceCollection services, IConfiguration config)
        {
            var options = services.GetOptions<TenantSettings>(nameof(TenantSettings));

            var defaultDbProvider = options.Defaults?.DBProvider;

            // services.AddSingleton<IDbContextSchema>(new DbContextSchema("alpha"));
            //services.AddSingleton<IDbContextSchema>(new DbContextSchema("beta"));
            //services.AddSingleton<IDbContextSchema>(new DbContextSchema("charlie"));
            // services.AddSingleton<IDbContextSchema>(new DbContextSchema("java"));



            // if (defaultDbProvider.ToLower() == "mssql")
            // {
            //     services.AddDbContext<ApplicationDbContext>(builder =>
            //            builder
            //            .ReplaceService<ITenantService, TenantService>()
            //            .UseSqlServer(e => e.MigrationsAssembly(typeof(ApplicationContextFactory).Assembly.FullName))
            //            .UseSqlServer(e => e.MigrationsHistoryTable("__EFMigrationsHistory", "java"))
            //            .ReplaceService<IDesignTimeDbContextFactory<DbContext>, ApplicationContextFactory>()
            //            .ReplaceService<IModelCacheKeyFactory, DbSchemaAwareModelCacheKeyFactory>() //JG
            //            .ReplaceService<IMigrationsAssembly, DbSchemaAwareMigrationAssembly>()
            //         );
            // }

            var defaultConnectionString = options.Defaults?.ConnectionString;
            var tenants = options.Tenants;

            foreach (var tenant in tenants)
            {
                var schema = tenant?.TID;


                string connectionString;
                if (string.IsNullOrEmpty(tenant.ConnectionString))
                {
                    connectionString = defaultConnectionString;
                }
                else
                {
                    connectionString = tenant.ConnectionString;
                }
                //Console.WriteLine("Connection string - " + connectionString);
                Console.WriteLine("Tenant name - " + tenant?.Name);
                Console.WriteLine("Tenant schema (TID) - " + schema);

                services.AddSingleton<IDbContextSchema>(new DbContextSchema(schema));

                if (defaultDbProvider.ToLower() == "mssql")
                {
                    services.AddDbContext<ApplicationDbContext>(builder =>
                           builder
                           .ReplaceService<ITenantService, TenantService>()
                           .UseSqlServer(e => e.MigrationsAssembly(typeof(ApplicationContextFactory).Assembly.FullName))
                           .UseSqlServer(e => e.MigrationsHistoryTable("__EFMigrationsHistory", schema))
                           .ReplaceService<IDesignTimeDbContextFactory<DbContext>, ApplicationContextFactory>()
                           .ReplaceService<IModelCacheKeyFactory, DbSchemaAwareModelCacheKeyFactory>() //JG
                           .ReplaceService<IMigrationsAssembly, DbSchemaAwareMigrationAssembly>()
                        );
                }

                using var scope = services.BuildServiceProvider().CreateScope();

                var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                dbContext.Database.SetConnectionString(connectionString);

                Console.WriteLine("------DB context schema: " + dbContext.TenantId);

                if (dbContext.Database.GetMigrations().Count() > 0)
                {
                    dbContext.Database.Migrate();
                }

                //scope.Dispose();                
                //dbContext.Dispose();
                // ↑ nada disto acima functiona

                // O services tem um Clear (porque é IEnumerable)
                // Isto funciona para as migrações, mas como dám erro não corre o projecto.
                
                services.Clear();
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
