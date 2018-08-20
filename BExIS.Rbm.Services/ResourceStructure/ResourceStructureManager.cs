using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vaiona.Persistence.Api;
using BExIS.Rbm.Entities;
using System.Diagnostics.Contracts;
using RS = BExIS.Rbm.Entities.ResourceStructure;
using System.Collections;
using BExIS.Rbm.Entities.ResourceStructure;
using R= BExIS.Rbm.Entities.Resource;

namespace BExIS.Rbm.Services.ResourceStructure
{
   public class ResourceStructureManager : IDisposable
    {
        private readonly IUnitOfWork _guow;
        private bool _isDisposed;

        public ResourceStructureManager()
       {
            _guow = this.GetIsolatedUnitOfWork();
           this.ResourceStructureRepo = _guow.GetReadOnlyRepository<RS.ResourceStructure>();
           this.ResourceRepo = _guow.GetReadOnlyRepository<R.SingleResource>();
       }

        ~ResourceStructureManager()
        {
            Dispose(true);
        }

        public void Dispose()
        {
            Dispose(true);
        }

        public void Dispose(bool disposing)
        {
            if (!_isDisposed)
            {
                if (disposing)
                {
                    if (_guow != null)
                        _guow.Dispose();
                    _isDisposed = true;
                }
            }
        }

        #region ResourceStructure

        public RS.ResourceStructure Create(string name, string description, List<RS.ResourceAttributeUsage> resourceAttributeUsage, RS.ResourceStructure parent)
       {
           Contract.Requires(!string.IsNullOrWhiteSpace(name));
           Contract.Requires(!String.IsNullOrWhiteSpace(description));

           RS.ResourceStructure resourceStructure = new RS.ResourceStructure()
           {
               ResourceAttributeUsages = resourceAttributeUsage,
               Parent = parent,
               Name = name,
               Description = description
           };

           using (IUnitOfWork uow = this.GetUnitOfWork())
           {
               IRepository<RS.ResourceStructure> repo = uow.GetRepository<RS.ResourceStructure>();
               repo.Put(resourceStructure);
               uow.Commit();
           }

           return (resourceStructure);
       }

       public bool Delete(RS.ResourceStructure resoureStruc)
       {
           Contract.Requires(resoureStruc != null);
           Contract.Requires(resoureStruc.Id >= 0);

            bool deleted = false;

            if (resoureStruc.ResourceAttributeUsages != null)
            {
                using (var rsaManager = new ResourceStructureAttributeManager())
                {
                    deleted = rsaManager.DeleteUsagesByRSId(resoureStruc.Id);
                }
            }
            else
                deleted = true;

           if (deleted)
           {

               using (IUnitOfWork uow = this.GetUnitOfWork())
               {
                   IRepository<RS.ResourceStructure> repoStruc = uow.GetRepository<RS.ResourceStructure>();

                   resoureStruc = repoStruc.Reload(resoureStruc);
                   repoStruc.Delete(resoureStruc);

                   uow.Commit();
               }
               return (true);
           }

           return false;
       }

       public bool Delete(IEnumerable<RS.ResourceStructure> resoureStrucs)
       {
           using (IUnitOfWork uow = this.GetUnitOfWork())
           {
               IRepository<RS.ResourceStructure> repo = uow.GetRepository<RS.ResourceStructure>();
               foreach (var resourceStruc in resoureStrucs)
               {
                   var latest = repo.Reload(resourceStruc);
                   repo.Delete(latest);
               }

               uow.Commit();
           }

           return (true);
       }

       public RS.ResourceStructure Update(RS.ResourceStructure resourceStructure)
       {
           Contract.Requires(resourceStructure != null);

           using (IUnitOfWork uow = this.GetUnitOfWork())
           {
               IRepository<RS.ResourceStructure> repo = uow.GetRepository<RS.ResourceStructure>();
                repo.Merge(resourceStructure);
                var merged = repo.Get(resourceStructure.Id);
                repo.Put(merged); uow.Commit();
           }

           return resourceStructure;
       }

       public IQueryable<RS.ResourceStructure> GetAllResourceStructures()
       {
           return (ResourceStructureRepo.Query());
       }

       public RS.ResourceStructure GetResourceStructureById(long id)
       {
           return ResourceStructureRepo.Query(u => u.Id == id).FirstOrDefault();
       }

        public RS.ResourceStructure GetResourceStructureByName(string name)
        {
            return ResourceStructureRepo.Query(u => u.Name.ToLower() == name.ToLower()).FirstOrDefault();
        }

        public IQueryable<RS.ResourceStructure> GetResourceStructuresFromResource(long rId)
       {
           return ResourceStructureRepo.Query(rs => rs.Resources.Any(r => r.Id == rId));
       }
       
       public IQueryable<R.Resource> GetRSFromResource(long rsId)
       {
           return ResourceRepo.Query(r => r.ResourceStructure.Id == rsId);
       }


       public bool IsResourceStructureInUse(long rsId)
       {
           IQueryable<R.Resource> rList = this.GetRSFromResource(rsId);
           if (rList.Count() > 0)
               return true;
           else
               return false;
       }

        #endregion

        #region Data Readers

        public IReadOnlyRepository<RS.ResourceStructure> ResourceStructureRepo { get; private set; }
        public IReadOnlyRepository<R.SingleResource> ResourceRepo { get; private set; }

        #endregion

    }
}
