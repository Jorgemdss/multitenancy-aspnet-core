using System;
using System.Reflection;
using Core.Interfaces;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Migrations.Internal;

public class DbSchemaAwareMigrationAssembly : MigrationsAssembly
{
    private readonly DbContext _context;

    public DbSchemaAwareMigrationAssembly(ICurrentDbContext currentContext, 
          IDbContextOptions options, IMigrationsIdGenerator idGenerator, 
          IDiagnosticsLogger<DbLoggerCategory.Migrations> logger)
      : base(currentContext, options, idGenerator, logger)
    {
      _context = currentContext.Context;
    }

    public override Migration CreateMigration(TypeInfo migrationClass, 
          string activeProvider)
    {
      if (activeProvider == null)
        throw new ArgumentNullException(nameof(activeProvider));

      var hasCtorWithSchema = migrationClass
              .GetConstructor(new[] { typeof(ApplicationDbContext) }) != null;

      if (hasCtorWithSchema && _context is ApplicationDbContext schema)
      {
        var instance = (Migration)Activator.CreateInstance(migrationClass.AsType(), schema);
        instance.ActiveProvider = activeProvider;
        return instance;
      }

      return base.CreateMigration(migrationClass, activeProvider);
    }

    //******************************************************************************************

    // private readonly DbContext _context;

    // public DbSchemaAwareMigrationAssembly(ICurrentDbContext currentContext,
    //       IDbContextOptions options, IMigrationsIdGenerator idGenerator,
    //       IDiagnosticsLogger<DbLoggerCategory.Migrations> logger)
    //   : base(currentContext, options, idGenerator, logger)
    // {
    //     _context = currentContext.Context;
    // }

    // public override Migration CreateMigration(TypeInfo migrationClass,
    //       string activeProvider)
    // {
    //     if (activeProvider == null)
    //         throw new ArgumentNullException(nameof(activeProvider));

    //     var hasCtorWithSchema = migrationClass
    //             .GetConstructor(new[] { typeof(IDbContextSchema) }) != null;

    //     if (hasCtorWithSchema && _context is IDbContextSchema schema)
    //     {
    //         var instance = (Migration)Activator.CreateInstance(migrationClass.AsType(), schema);
    //         instance.ActiveProvider = activeProvider;
    //         return instance;
    //     }

    //     return base.CreateMigration(migrationClass, activeProvider);
    // }
}