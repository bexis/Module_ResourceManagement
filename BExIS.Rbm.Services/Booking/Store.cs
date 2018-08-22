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
            using (var uow = this.GetUnitOfWork())
            {
                ActivityManager activityManager = new ActivityManager();

                try
                {
                    var entities = activityManager.GetAllActivities().Select(r => new EntityStoreItem() { Id = r.Id, Title = r.Name });
                    return entities.ToList();
                }
                finally
                {
                    activityManager.Dispose();
                }
            }

        }
    }

    public class BookingEventStore : IEntityStore
    {
        public List<EntityStoreItem> GetEntities()
        {
            using (var uow = this.GetUnitOfWork())
            {
                BookingEventManager bookingEventManager  = new BookingEventManager();

                try
                {
                    var entities = bookingEventManager.GetAllBookingEvents().Select(r => new EntityStoreItem() { Id = r.Id, Title = r.Name });
                    return entities.ToList();
                }
                finally
                {
                    bookingEventManager.Dispose();
                }
            }

        }
    }

    public class NotificationStore : IEntityStore
    {
        public List<EntityStoreItem> GetEntities()
        {
            using (var uow = this.GetUnitOfWork())
            {
                NotificationManager notificationManager = new NotificationManager();

                try
                {
                    var entities = notificationManager.GetAllNotifications().Select(r => new EntityStoreItem() { Id = r.Id, Title = r.Subject });
                    return entities.ToList();
                }
                finally
                {
                    notificationManager.Dispose();
                }
            }

        }
    }

    public class ScheduleStore : IEntityStore
    {
        public List<EntityStoreItem> GetEntities()
        {
            using (var uow = this.GetUnitOfWork())
            {
                ScheduleManager scheduleManager = new ScheduleManager();

                try
                {
                    var entities = scheduleManager.GetAllSchedules().Select(r => new EntityStoreItem() { Id = r.Id, Title = r.Id.ToString() });
                    return entities.ToList();
                }
                finally
                {
                    scheduleManager.Dispose();
                }
            }

        }
    }


}
