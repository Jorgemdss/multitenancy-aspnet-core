using System;
using Infrastructure.Persistence;
using Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

public class ApplicationContextFactory : IDesignTimeDbContextFactory<ApplicationDbContext>, IDbContextSchema
{
    public string TenantId { get; set; }


    public ApplicationDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<ApplicationDbContext>();
        optionsBuilder.UseSqlServer("Data Source=(localdb)\\mssqllocaldb;Initial Catalog=mig_sharedTenantDb;Integrated Security=True;MultipleActiveResultSets=True");
        
        Console.WriteLine("Create factory db context: " + TenantId);
        return new ApplicationDbContext(optionsBuilder.Options, this);
    }

    

}