using BExIS.Rbm.Entities.BookingManagementTime;
using BExIS.Rbm.Entities.Resource;
using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Text;
using Vaiona.Persistence.Api;
using R = BExIS.Rbm.Entities.Resource;
using RS = BExIS.Rbm.Entities.ResourceStructure;

namespace BExIS.Rbm.Services.Resource
{
    public class SingleResourceManager : IDisposable
    {
        private readonly IUnitOfWork _guow;
        private bool _isDisposed;

        public SingleResourceManager()
        {
            _guow = this.GetIsolatedUnitOfWork();
            this.SingleResourceRepo = _guow.GetReadOnlyRepository<R.SingleResource>();
            //this.BookingTimeGranularityRepo = _guow.GetReadOnlyRepository<R.BookingTimeGranularity>();
            this.ResourceGroupRepo = _guow.GetReadOnlyRepository<R.ResourceGroup>();
        }

        ~SingleResourceManager()
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


        #region SingleResource

        public R.SingleResource CreateResource(string name, string description, int quantity, string color, bool withActivity, RS.ResourceStructure resourceStructure,
           TimeDuration duration)
        {
            //default status is created. In this status it is not visible.
            Status status = Status.created;
            DateTime statusChangeDate = DateTime.Now;

            R.SingleResource resource = new R.SingleResource(resourceStructure);

            //resource.BookingTimeGranularity = bookingTimeGranularity;
            

            resource.Name = name;
            resource.Description = description;
            resource.Quantity = quantity;
            resource.ResourceStatus = status;
            resource.Duration = duration;
            resource.Color = color;
            resource.WithActivity = withActivity;
            resource.StatusChangeDate = statusChangeDate;
            resource.ResourceStatus = status;

            //bookingTimeGranularity.Resource = resource;

            //bookingTimeGranularity.Resource = resource;

            using (IUnitOfWork uow = this.GetUnitOfWork())
            {
                IRepository<R.SingleResource> repo = uow.GetRepository<R.SingleResource>();
                //IRepository<R.BookingTimeGranularity> repoB = uow.GetRepository<R.BookingTimeGranularity>();
                //resource.BookingTimeGranularity = this.CreateBookingTimeGranularity(bookingTimeGranularity.Duration, bookingTimeGranularity.LargeUnitOfTime, bookingTimeGranularity.StartTime);
                //repoB.Put(bookingTimeGranularity);
                repo.Put(resource);
                uow.Commit();
            }

            return resource;
        }

        public bool DeleteResource(R.SingleResource resource)
        {
            Contract.Requires(resource != null);
            Contract.Requires(resource.Id >= 0);

            using (IUnitOfWork uow = this.GetUnitOfWork())
            {
                IRepository<R.SingleResource> repo = uow.GetRepository<R.SingleResource>();
                resource = repo.Reload(resource);
                repo.Delete(resource);
                uow.Commit();
            }

            return true;
        }

        public bool DeleteResource(IEnumerable<R.SingleResource> resources)
        {
            using (IUnitOfWork uow = this.GetUnitOfWork())
            {
                IRepository<R.SingleResource> repo = uow.GetRepository<R.SingleResource>();
                foreach (var resource in resources)
                {
                    var latest = repo.Reload(resource);
                    repo.Delete(latest);
                }

                uow.Commit();
            }

            return true;
        }

        public R.Resource UpdateResource(R.SingleResource resource)
        {
            Contract.Requires(resource != null);
            using (IUnitOfWork uow = this.GetUnitOfWork())
            {
                IRepository<R.SingleResource> repo = uow.GetRepository<R.SingleResource>();
                repo.Put(resource);
                uow.Commit();
            }

            return resource;
        }

        public IQueryable<R.SingleResource> GetAllResources()
        {
            
                return SingleResourceRepo.Query();
            
        }


        public R.SingleResource GetResourceById(long id)
        {
            R.SingleResource resource = new R.SingleResource();
            return resource = SingleResourceRepo.Query(u => u.Id == id).FirstOrDefault();
        }

        public R.SingleResource GetResourceByName(string name)
        {
          
            return SingleResourceRepo.Query(u => u.Name.ToLower() == name.ToLower()).FirstOrDefault();
        }

        public IQueryable<ResourceGroup> GetResourceGroupsFromResource(long id)
        {
            return ResourceGroupRepo.Query(r=>r.SingleResources.Any(u=>u.Id == id));
        }

        public bool IsResourceInSet(long id)
        {
            IQueryable<ResourceGroup> setList = GetResourceGroupsFromResource(id);
            if (setList.Count() > 0)
                return true;
            else
                return false;
        }

        #endregion

        #region Data Readers

        public IReadOnlyRepository<R.SingleResource> SingleResourceRepo { get; private set; }
        public IReadOnlyRepository<R.ResourceGroup> ResourceGroupRepo { get; private set; }

        #endregion

        #region ResourceGroup

        public R.ResourceGroup CreateResourceGroup(string name, R.Mode mode, ICollection<R.SingleResource> resources)
        {
            R.ResourceGroup resourceGroup = new R.ResourceGroup()
            {
                Name = name,
                GroupMode = mode,
                SingleResources = resources
            };

            using (IUnitOfWork uow = this.GetUnitOfWork())
            {
                IRepository<R.ResourceGroup> repo = uow.GetRepository<R.ResourceGroup>();
                repo.Put(resourceGroup);
                uow.Commit();
            }

            return resourceGroup;
        }

        public bool DeleteResourceGroup(R.ResourceGroup resourceGroup)
        {
            Contract.Requires(resourceGroup != null);
            Contract.Requires(resourceGroup.Id >= 0);

            using (IUnitOfWork uow = this.GetUnitOfWork())
            {
                IRepository<R.ResourceGroup> repo = uow.GetRepository<R.ResourceGroup>();
                resourceGroup = repo.Reload(resourceGroup);
                repo.Delete(resourceGroup);
                uow.Commit();
            }

            return true;
        }

        public bool DeleteResourceGroup(IEnumerable<R.ResourceGroup> resourceGroups)
        {
           
            using (IUnitOfWork uow = this.GetUnitOfWork())
            {
                IRepository<R.ResourceGroup> repo = uow.GetRepository<R.ResourceGroup>();
                foreach (var resourceGroup in resourceGroups)
                {
                    var latest = repo.Reload(resourceGroup);
                    repo.Delete(latest);
                }
                uow.Commit();
            }
            return true;
        }

        public R.ResourceGroup UpdateResourceGroup(R.ResourceGroup resourceGroup)
        {
            Contract.Requires(resourceGroup != null);

            using (IUnitOfWork uow = this.GetUnitOfWork())
            {
                IRepository<R.ResourceGroup> repo = uow.GetRepository<R.ResourceGroup>();
                repo.Put(resourceGroup);
                uow.Commit();
            }
            return resourceGroup;
        }

        public R.ResourceGroup GetResourceGroupById(long id)
        {
            return ResourceGroupRepo.Query(u => u.Id == id).FirstOrDefault();
        }

        public IQueryable<R.ResourceGroup> GetAllResourceClassifiers()
        {
            return ResourceGroupRepo.Query();
        }

        #endregion

        //#region BookingTimeGranularity

        //public R.BookingTimeGranularity CreateBookingTimeGranularity(TimeDuration duration, SystemDefinedUnit largeUnitOfTime, int startTime)
        //{
        //    R.BookingTimeGranularity bookingTimeGranularity = new R.BookingTimeGranularity()
        //    {
        //        Duration = duration,
        //        LargeUnitOfTime = largeUnitOfTime,
        //        StartTime = startTime,
        //    };

        //    using (IUnitOfWork uow = this.GetUnitOfWork())
        //    {
        //        IRepository<R.BookingTimeGranularity> repo = uow.GetRepository<R.BookingTimeGranularity>();
        //        repo.Put(bookingTimeGranularity);
        //        uow.Commit();
        //    }
        //    return bookingTimeGranularity;
        //}

        //public bool DeleteBookingTimeGranularity(R.BookingTimeGranularity bookingTimeGranularity)
        //{
        //    Contract.Requires(bookingTimeGranularity != null);
        //    Contract.Requires(bookingTimeGranularity.Id >= 0);

        //    using (IUnitOfWork uow = this.GetUnitOfWork())
        //    {
        //        IRepository<R.BookingTimeGranularity> repo = uow.GetRepository<R.BookingTimeGranularity>();
        //        bookingTimeGranularity = repo.Reload(bookingTimeGranularity);
        //        repo.Delete(bookingTimeGranularity);
        //        uow.Commit();
        //    }
        //    return true;
        //}

        //public R.BookingTimeGranularity UpdateBookingTimeGranularity(R.BookingTimeGranularity bookingTimeGranularity)
        //{
        //    Contract.Requires(bookingTimeGranularity != null);
        //    using (IUnitOfWork uow = this.GetUnitOfWork())
        //    {
        //        IRepository<R.BookingTimeGranularity> repo = uow.GetRepository<R.BookingTimeGranularity>();
        //        repo.Put(bookingTimeGranularity);
        //        uow.Commit();
        //    }
        //    return bookingTimeGranularity;
        //}

        //public R.BookingTimeGranularity GetBookingTimeGranularityById(long id)
        //{
        //    return BookingTimeGranularityRepo.Query(u => u.Id == id).FirstOrDefault();
        //}

        //public R.BookingTimeGranularity GetBookingTimeGranularityByResourceId(long id)
        //{
        //    R.SingleResource resource = SingleResourceRepo.Query(u => u.BookingTimeGranularity.Id == id).FirstOrDefault();
        //    return GetBookingTimeGranularityById(resource.BookingTimeGranularity.Id);
        //}

        //#endregion
    }
}
