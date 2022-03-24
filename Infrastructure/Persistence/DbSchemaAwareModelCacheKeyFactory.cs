
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;

namespace Infrastructure.Persistence;

// used from from 
// https://github.com/xerere/EntityFrameworkCore-Demos/blob/master/src/EntityFramework.Demo/SchemaChange/DbSchemaAwareModelCacheKeyFactory.cs
public class DbSchemaAwareModelCacheKeyFactory : IModelCacheKeyFactory
{
// ! avoid the "The requested configuration is not stored in the read-optimized model, please use DbContext.GetService<IDesignTimeModel>().Model."
// read https://nicolaiarocci.com/my-asp.net-5-migration-to-.net-6/
// in the -> New IModelCacheKeyFactory.Create() overload

//   public object Create(DbContext context)
//   {
//     return new {
//         Type = context.GetType(),
//         Schema = context is ApplicationDbContext applicationDbContext ? (context as ApplicationDbContext).TenantId : null
//     };
//   }

  public object Create(DbContext context, bool designTime) 
  {
    if (context is ApplicationDbContext applicationDbContext)
    {
      var r = (context.GetType(), applicationDbContext.TenantId, designTime);
      return r;
    }
    return  (object)context.GetType();

    //  context is ApplicationDbContext applicationDbContext
    //    ? (context.GetType(), applicationDbContext.TenantId, designTime)
    //    : (object)context.GetType();
  }

    public object Create(DbContext context) => Create(context, false);
}