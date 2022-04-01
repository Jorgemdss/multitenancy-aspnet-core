using System;
using Infrastructure.Persistence;

public class DbContextSchema : IDbContextSchema
{
  public string TenantId { get; }

  public DbContextSchema(string tSchema)
  {
    TenantId = tSchema ?? throw new ArgumentNullException(nameof(tSchema));
  }
}