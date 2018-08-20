using BExIS.Rbm.Entities.Booking;
using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Text;
using Vaiona.Persistence.Api;
using E = BExIS.Rbm.Entities.Booking;
using R = BExIS.Rbm.Entities.Resource;

namespace BExIS.Rbm.Services.Booking
{
    public class EventManager : IDisposable
    {
        private readonly IUnitOfWork _guow;
        private bool _isDisposed;

        public EventManager()
        {
            _guow = this.GetIsolatedUnitOfWork();
            this.EventRepo = _guow.GetReadOnlyRepository<E.BookingEvent>();
            this.BookingTemplateRepo = _guow.GetReadOnlyRepository<E.BookingTemplate>();
        }

        ~EventManager()
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

        public IReadOnlyRepository<E.BookingEvent> EventRepo { get; private set; }
        public IReadOnlyRepository<E.BookingTemplate> BookingTemplateRepo { get; private set; }

        #endregion

        #region Methods

        public E.BookingEvent CreateEvent(string name, string description, List<Activity> activities, DateTime minDate, DateTime maxDate)
        {
            BookingEvent newEvent = new BookingEvent()
            {
                Name = name,
                Description = description,
                //Activities = activities,
                MinDate = minDate,
                MaxDate = maxDate
            };

            using (IUnitOfWork uow = this.GetUnitOfWork())
            {
                IRepository<E.BookingEvent> repo = uow.GetRepository<E.BookingEvent>();
                repo.Put(newEvent);
                uow.Commit();
            }

            return newEvent;
        }

        public bool DeleteEvent(E.BookingEvent deleteEvent)
        {
            Contract.Requires(deleteEvent != null);
            Contract.Requires(deleteEvent.Id >= 0);

            ScheduleManager sManager = new ScheduleManager();
            bool deleteSchedules = sManager.RemoveAllSchedulesByEvent(deleteEvent.Id);

            if (deleteSchedules)
            {
                using (IUnitOfWork uow = this.GetUnitOfWork())
                {
                    IRepository<E.BookingEvent> repo = uow.GetRepository<E.BookingEvent>();
                    deleteEvent = repo.Reload(deleteEvent);
                    repo.Delete(deleteEvent);
                    uow.Commit();
                }
            }

            return true;
        }

        public E.BookingEvent UpdateEvent(E.BookingEvent eEvent)
        {
            Contract.Requires(eEvent != null);
            using (IUnitOfWork uow = this.GetUnitOfWork())
            {
                IRepository<E.BookingEvent> repo = uow.GetRepository<E.BookingEvent>();
                repo.Merge(eEvent);
                var merged = repo.Get(eEvent.Id);
                repo.Put(merged);
                uow.Commit();
            }

            return eEvent;
        }

        public IQueryable<E.BookingEvent> GetAllEvents()
        {
            return EventRepo.Query();
        }

        public List<E.BookingEvent> GetAllEventByTimePeriod(DateTime startDate, DateTime endDate)
        {
            return EventRepo.Query(a => ((DateTime)a.MinDate >= startDate && (DateTime)a.MinDate <= endDate) || ((DateTime)a.MaxDate >= startDate && (DateTime)a.MaxDate <= endDate)).ToList(); 
        }

        public BookingEvent GetEventById(long id)
        {
            BookingEvent e = EventRepo.Query(a => a.Id == id).FirstOrDefault();
            EventRepo.LoadIfNot(e.Schedules);
            return e;
        }

        //public IQueryable<RealEvent> GetEventsWhereActivity(long activityId)
        //{
        //    return EventRepo.Query(a => a.Activities.Any(u=>u.Id == activityId));
        //}

        #endregion
    }
}
