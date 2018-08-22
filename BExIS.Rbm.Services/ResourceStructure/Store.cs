using BExIS.Security.Services.Objects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vaiona.Persistence.Api;

namespace BExIS.Rbm.Services.ResourceStructure
{
    public class ResourceStructureStore : IEntityStore
    {
        public List<EntityStoreItem> GetEntities()
        {
            using (var uow = this.GetUnitOfWork())
            {
                ResourceStructureManager resourceStructureManager = new ResourceStructureManager();

                try
                {
                    var entities = resourceStructureManager.GetAllResourceStructures().Select(r => new EntityStoreItem() { Id = r.Id, Title = r.Name });
                    return entities.ToList();
                }
                finally
                {
                    resourceStructureManager.Dispose();
                }
            }

        }
    }

    public class ResourceStructureAttributeStore : IEntityStore
    {
        public List<EntityStoreItem> GetEntities()
        {
            using (var uow = this.GetUnitOfWork())
            {
                ResourceStructureAttributeManager resourceStructureAttributeManager = new ResourceStructureAttributeManager();

                try
                {
                    var entities = resourceStructureAttributeManager.GetAllResourceStructureAttributes().Select(r => new EntityStoreItem() { Id = r.Id, Title = r.Name });
                    return entities.ToList();
                }
                finally
                {
                    resourceStructureAttributeManager.Dispose();
                }
            }

        }
    }

}
