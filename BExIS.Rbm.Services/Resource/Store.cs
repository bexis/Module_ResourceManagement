using BExIS.Security.Services.Objects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Resources;
using System.Text;
using System.Threading.Tasks;
using Vaiona.Persistence.Api;

namespace BExIS.Rbm.Services.Resource
{
    public class SingleResourceStore : IEntityStore
    {
        public List<EntityStoreItem> GetEntities()
        {
            using (var uow = this.GetUnitOfWork())
            {
                ResourceManager resourceManager = new ResourceManager();

                try
                {
                    var entities = resourceManager.GetAllResources().Select(r => new EntityStoreItem() { Id = r.Id, Title = r.Name});
                    return entities.ToList();
                }
                finally
                {
                    resourceManager.Dispose();
                }
            }

        }
    }

    public class ResourceGroupStore : IEntityStore
    {
        public List<EntityStoreItem> GetEntities()
        {
            using (var uow = this.GetUnitOfWork())
            {
                ResourceManager resourceManager = new ResourceManager();

                try
                {
                    var entities = resourceManager.GetAllResourceGroups().Select(r => new EntityStoreItem() { Id = r.Id, Title = r.Name });
                    return entities.ToList();
                }

                finally
                {
                    resourceManager.Dispose();
                }
            }

        }
    }
}
