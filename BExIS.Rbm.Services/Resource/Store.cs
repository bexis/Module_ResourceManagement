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
            return GetEntities(0, -1);
        }

        public List<EntityStoreItem> GetEntities(int skip, int take)
        {
            bool withPaging = (take >= 0);

            using (var uow = this.GetUnitOfWork())
            using (ResourceManager resourceManager = new ResourceManager())
            {
                if (withPaging)
                {
                    return resourceManager.GetAllResources().Select(r => new EntityStoreItem() { Id = r.Id, Title = r.Name }).Take(take).Skip(skip).ToList();
                }
                else
                {
                    return resourceManager.GetAllResources().Select(r => new EntityStoreItem() { Id = r.Id, Title = r.Name }).ToList();
                } 
            }
        }

        public int CountEntities()
        {
            return GetEntities(0, -1).Count();
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

    public class ResourceGroupStore : IEntityStore
    {
        public List<EntityStoreItem> GetEntities()
        {
            return GetEntities(0, -1);
        }

        public List<EntityStoreItem> GetEntities(int skip, int take)
        {
            bool withPaging = (take >= 0);

            using (var uow = this.GetUnitOfWork())
            using (ResourceManager resourceManager = new ResourceManager())
            {
                if (withPaging)
                {
                    return resourceManager.GetAllResourceGroups().Select(r => new EntityStoreItem() { Id = r.Id, Title = r.Name }).Skip(skip).Take(take).ToList();
                }
                else
                {
                    return resourceManager.GetAllResourceGroups().Select(r => new EntityStoreItem() { Id = r.Id, Title = r.Name }).ToList();
                }
            }

        }

        public int CountEntities()
        {
            return GetEntities(0, -1).Count();
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
