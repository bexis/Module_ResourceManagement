using BExIS.Security.Services.Objects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vaiona.Persistence.Api;

namespace BExIS.Rbm.Services.Booking
{
    public class ActivityStore : IEntityStore
    {
        public List<EntityStoreItem> GetEntities()
        {
            return GetEntities(0, -1);
        }

        public List<EntityStoreItem> GetEntities(int skip, int take)
        {
            bool withPaging = (take >= 0);

            using (var uow = this.GetUnitOfWork())
            using (ActivityManager activityManager = new ActivityManager())
            {
                if (withPaging)
                {
                    return activityManager.GetAllActivities().Select(r => new EntityStoreItem() { Id = r.Id, Title = r.Name }).Take(take).Skip(skip).ToList();
                }
                else
                {
                    return activityManager.GetAllActivities().Select(r => new EntityStoreItem() { Id = r.Id, Title = r.Name }).ToList();
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

    public class BookingEventStore : IEntityStore
    {
        public List<EntityStoreItem> GetEntities()
        {
            return GetEntities(0, -1);
        }

        public List<EntityStoreItem> GetEntities(int skip, int take)
        {
            bool withPaging = (take >= 0);

            using (var uow = this.GetUnitOfWork())
            using( BookingEventManager bookingEventManager = new BookingEventManager())
            {
                if (withPaging)
                {
                    return bookingEventManager.GetAllBookingEvents().Select(r => new EntityStoreItem() { Id = r.Id, Title = r.Name }).Take(take).Skip(skip).ToList();
                }
                else
                {
                    return bookingEventManager.GetAllBookingEvents().Select(r => new EntityStoreItem() { Id = r.Id, Title = r.Name }).ToList();
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

    public class NotificationStore : IEntityStore
    {
        public List<EntityStoreItem> GetEntities()
        {
            return GetEntities(0, -1);
        }

        public List<EntityStoreItem> GetEntities(int skip, int take)
        {
            bool withPaging = (take >= 0);

            using (var uow = this.GetUnitOfWork())
            using (NotificationManager notificationManager = new NotificationManager())
            {
                if (withPaging)
                {
                    return notificationManager.GetAllNotifications().Select(r => new EntityStoreItem() { Id = r.Id, Title = r.Subject }).Take(take).Skip(skip).ToList();
                }
                else
                {
                    return notificationManager.GetAllNotifications().Select(r => new EntityStoreItem() { Id = r.Id, Title = r.Subject }).ToList();
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

    public class ScheduleStore : IEntityStore
    {
        public List<EntityStoreItem> GetEntities()
        {
            return GetEntities(0, -1);
        }

        public List<EntityStoreItem> GetEntities(int skip, int take)
        {
            bool withPaging = (take >= 0);

            using (var uow = this.GetUnitOfWork())
            using (ScheduleManager scheduleManager = new ScheduleManager())
            {
                if (withPaging)
                {
                    return scheduleManager.GetAllSchedules().Select(r => new EntityStoreItem() { Id = r.Id, Title = r.Id.ToString() }).Skip(skip).Take(take).ToList();
                }
                else
                {
                    return scheduleManager.GetAllSchedules().Select(r => new EntityStoreItem() { Id = r.Id, Title = r.Id.ToString() }).ToList();
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
