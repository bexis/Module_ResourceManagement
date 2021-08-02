using BExIS.Rbm.Entities.Booking;
using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Text;
using Vaiona.Persistence.Api;

namespace BExIS.Rbm.Services.Booking
{
    public class NotificationManager : IDisposable
    {
        private readonly IUnitOfWork _guow;
        private bool _isDisposed;

        public NotificationManager()
        {
            _guow = this.GetIsolatedUnitOfWork();
            this.NotificationRepo = _guow.GetReadOnlyRepository<Notification>();
            this.NotificationDependencyRepo = _guow.GetReadOnlyRepository<NotificationDependency>();
        }

        ~NotificationManager()
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

        #region Data Readers

        public IReadOnlyRepository<Notification> NotificationRepo { get; private set; }
        public IReadOnlyRepository<NotificationDependency> NotificationDependencyRepo { get; private set; }
       
        #endregion

        #region Notification Methods

        public Notification CreateNotification(string subject, DateTime startDate, DateTime endDate, string message)
        {
            DateTime insertDate = DateTime.Now;
            Notification newNotification = new Notification()
            {
                Subject = subject,
                StartDate = startDate,
                EndDate = endDate,
                Message = message,
                InsertDate = insertDate
            };

            using (IUnitOfWork uow = this.GetUnitOfWork())
            {
                IRepository<Notification> repo = uow.GetRepository<Notification>();
                repo.Put(newNotification);
                uow.Commit();
            }

            return newNotification;
        }

        public Notification UpdateNotification(Notification notification)
        {
            Contract.Requires(notification != null);
            using (IUnitOfWork uow = this.GetUnitOfWork())
            {
                IRepository<Notification> repo = uow.GetRepository<Notification>();
                repo.Merge(notification);
                var merged = repo.Get(notification.Id);
                repo.Put(merged);
                uow.Commit();
            }
            return notification;

        }

        public bool DeleteNotification(Notification notification)
        {
            Contract.Requires(notification != null);
            Contract.Requires(notification.Id >= 0);

            using (IUnitOfWork uow = this.GetUnitOfWork())
            {
                IRepository<Notification> repo = uow.GetRepository<Notification>();
                notification = repo.Reload(notification);
                repo.Delete(notification);
                uow.Commit();
            }

            return true;
        }

        public IQueryable<Notification> GetAllNotifications()
        {
            return NotificationRepo.Query().OrderByDescending(a => a.InsertDate);
        }

        public Notification GetNotificationById(long id)
        {
            return NotificationRepo.Query(a => a.Id == id).FirstOrDefault();
        }

        public List<Notification> GetNotificationsFromTime(DateTime startDate)
        {
            return NotificationRepo.Query(a => startDate <= a.EndDate).ToList();
        }

        public List<Notification> GetNotificationsByTimePeriod(DateTime startDate, DateTime endDate)
        {
            return NotificationRepo.Query(a=> startDate >=a.StartDate  && startDate <= a.EndDate || a.EndDate >= a.StartDate && endDate <= a.EndDate).ToList();
        }

        //public IQueryable<Notification> GetEventsBySchedule(long scheduleId)
        //{
        //    return NotificationRepo.Query(a => a.Schedules.Any(u => u.Id == scheduleId));
        //}

        #endregion

        #region Notification Dependency Methods

        public NotificationDependency CreateNotificationDependency(Notification notification, long attibuteId, string domainItem)
        {
            DateTime insertDate = DateTime.Now;
            NotificationDependency newNotificationDependency = new NotificationDependency()
            {
               Notification = notification,
               AttributeId = attibuteId,
               DomainItem = domainItem
            };

            using (IUnitOfWork uow = this.GetUnitOfWork())
            {
                IRepository<NotificationDependency> repo = uow.GetRepository<NotificationDependency>();
                repo.Put(newNotificationDependency);
                uow.Commit();
            }

            return newNotificationDependency;
        }

        public bool DeleteNotificationDependency(NotificationDependency notificationDependency)
        {
            Contract.Requires(notificationDependency != null);
            Contract.Requires(notificationDependency.Id >= 0);

            using (IUnitOfWork uow = this.GetUnitOfWork())
            {
                IRepository<NotificationDependency> repo = uow.GetRepository<NotificationDependency>();
                notificationDependency = repo.Reload(notificationDependency);
                repo.Delete(notificationDependency);
                uow.Commit();
            }

            return true;
        }

        public List<NotificationDependency> GetNotificationDependenciesByNotification(long id)
        {
            return NotificationDependencyRepo.Query(a => a.Notification.Id == id).ToList();
        }

        #endregion
    }
}
