using BExIS.Rbm.Entities.Booking;
using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Text;
using Vaiona.Persistence.Api;

namespace BExIS.Rbm.Services.Booking
{
    /// <summary>
    /// ActivityManager class is responsible for CRUD (Create, Read, Update, Delete) operations on the aggregate area of the activity.
    /// </summary>
    public class ActivityManager
    {
        public ActivityManager()
        {
            IUnitOfWork uow = this.GetUnitOfWork();
            this.ActivityRepo = uow.GetReadOnlyRepository<Activity>();
        }

        #region Data Readers

        public IReadOnlyRepository<Activity> ActivityRepo { get; private set; }

        #endregion

        #region Methods

        /// <summary>
        /// Creates an activity <seealso cref="Activity"/> and persists the entity in the database.
        /// </summary>
        public Activity CreateActivity(string name, string description, bool disable)
        {
            Activity activity = new Activity();
            activity.Name = name;
            activity.Disable = disable;
            activity.Description = description;

            using (IUnitOfWork uow = this.GetUnitOfWork())
            {
                IRepository<Activity> repo = uow.GetRepository<Activity>();
                repo.Put(activity);
                uow.Commit();
            }

            return activity;
        }

        /// <summary>
        /// If the <paramref name="Activity"/> is not associated to any <see cref="BookingEvent"/>, the method deletes it from the database.
        /// </summary>
        public bool DeleteActivity(Activity activity)
        {
            Contract.Requires(activity != null);
            Contract.Requires(activity.Id >= 0);

            using (IUnitOfWork uow = this.GetUnitOfWork())
            {
                IRepository<Activity> repo = uow.GetRepository<Activity>();
                activity = repo.Reload(activity);
                repo.Delete(activity);
                uow.Commit();
            }

            return true;
        }

        public Activity UpdateActivity(Activity activity)
        {
            Contract.Requires(activity != null);
            using (IUnitOfWork uow = this.GetUnitOfWork())
            {
                IRepository<Activity> repo = uow.GetRepository<Activity>();
                repo.Put(activity);
                uow.Commit();
            }
            return activity;

        }

        public IQueryable<Activity> GetAllActivities()
        {
            return ActivityRepo.Query();
        }

        public Activity GetActivityById(long id)
        {
            return ActivityRepo.Query(a => a.Id == id).FirstOrDefault();
        }


        public List<Activity> GetAllAvailableActivityById()
        {
            return ActivityRepo.Query(a => a.Disable == false).ToList();
        }

        public Activity GetActivityByName(string name)
        {
            return ActivityRepo.Query(a => a.Name.ToLower() == name.ToLower()).FirstOrDefault();
        }

        /// <summary>
        /// Checks if activity is in use in a event.
        /// </summary>
        public bool IsInEvent(long id)
        {
            EventManager eManager = new EventManager();
            List<BookingEvent> eventList = eManager.GetEventsWhereActivity(id).ToList();
            if (eventList.Count() > 0)
                return true;
            else
                return false;
        }


        #endregion

    }
}
