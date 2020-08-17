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
            return GetEntities(0, -1);
        }

        public int CountEntities()
        {
            return GetEntities().Count();
        }

        public List<EntityStoreItem> GetEntities(int skip, int take)
        {
            bool withPaging = (take >= 0);

            using (var uow = this.GetUnitOfWork())
            using (ResourceStructureManager resourceStructureManager = new ResourceStructureManager())
            {
                if (withPaging)
                {
                    return resourceStructureManager.GetAllResourceStructures().Select(r => new EntityStoreItem() { Id = r.Id, Title = r.Name }).Skip(skip).Take(take).ToList();
                }
                else
                {
                    return resourceStructureManager.GetAllResourceStructures().Select(r => new EntityStoreItem() { Id = r.Id, Title = r.Name }).ToList();
                }
            }
        }

        public string GetTitleById(long id)
        {
            throw new System.NotImplementedException();
        }

        public bool HasVersions()
        {
            throw new NotImplementedException();
        }

        public int CountVersions(long id)
        {
            throw new NotImplementedException();
        }

        public List<EntityStoreItem> GetVersionsById(long id)
        {
            throw new NotImplementedException();
        }

        public bool Exist(long id)
        {
            throw new NotImplementedException();
        }
    }

    public class ResourceStructureAttributeStore : IEntityStore
    {
        public List<EntityStoreItem> GetEntities()
        {
            return GetEntities(0, -1);
        }

        public int CountEntities()
        {
            return GetEntities().Count();
        }

        public List<EntityStoreItem> GetEntities(int skip, int take)
        {
            bool withPaging = (take >= 0);

            using (var uow = this.GetUnitOfWork())
            using (ResourceStructureAttributeManager resourceStructureAttributeManager = new ResourceStructureAttributeManager())
            {
                if (withPaging)
                {
                    return resourceStructureAttributeManager.GetAllResourceStructureAttributes().Select(r => new EntityStoreItem() { Id = r.Id, Title = r.Name }).Skip(skip).Take(take).ToList();
                }
                else
                {
                    return resourceStructureAttributeManager.GetAllResourceStructureAttributes().Select(r => new EntityStoreItem() { Id = r.Id, Title = r.Name }).ToList();
                } 
            }
        }

        public string GetTitleById(long id)
        {
            throw new System.NotImplementedException();
        }

        public bool HasVersions()
        {
            throw new NotImplementedException();
        }

        public int CountVersions(long id)
        {
            throw new NotImplementedException();
        }

        public List<EntityStoreItem> GetVersionsById(long id)
        {
            throw new NotImplementedException();
        }

        public bool Exist(long id)
        {
            throw new NotImplementedException();
        }
    }

}
