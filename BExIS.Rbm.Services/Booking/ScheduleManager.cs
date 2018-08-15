using BExIS.Rbm.Entities.Booking;
using R = BExIS.Rbm.Entities.Resource;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Vaiona.Persistence.Api;
using System.Diagnostics.Contracts;
using BExIS.Rbm.Entities.Users;
using BExIS.Rbm.Services.Users;

namespace BExIS.Rbm.Services.Booking
{
    public class ScheduleManager : IDisposable
    {
        private readonly IUnitOfWork _guow;
        private bool _isDisposed;

        public ScheduleManager()
        {
            _guow = this.GetIsolatedUnitOfWork();
            this.ScheduleRepo = _guow.GetReadOnlyRepository<Schedule>();
        }

        ~ScheduleManager()
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

        public IReadOnlyRepository<Schedule> ScheduleRepo { get; private set; }

        #endregion

        #region Methods

        public Schedule CreateSchedule(DateTime startDate, DateTime endDate, RealEvent thisEvent, R.SingleResource resource, Person forPerson, Person byPerson, List<Activity> activities,int quantity, int index)
        {
            Schedule schedule = new Schedule();
            schedule.StartDate = startDate;
            schedule.EndDate = endDate;
            schedule.Event = thisEvent;
            schedule.Resource = resource;
            schedule.Activities = activities;
            schedule.Index = index;
            schedule.Quantity = quantity;

            if (forPerson is PersonGroup)
            {
                PersonGroup pGroup = (PersonGroup)forPerson;
                schedule.ForPerson = pGroup;
            }
            else
            {
                IndividualPerson iPerson = (IndividualPerson)forPerson;
                schedule.ForPerson = iPerson;
            }

            IndividualPerson iPersonBy = (IndividualPerson)byPerson;
            schedule.ByPerson = iPersonBy;

            using (IUnitOfWork uow = this.GetUnitOfWork())
            {
                IRepository<Schedule> repo = uow.GetRepository<Schedule>();
                repo.Put(schedule);
                uow.Commit();
            }

            return schedule;
        }

        public bool DeleteSchedule(Schedule schedule)
        {
            Contract.Requires(schedule != null);
            Contract.Requires(schedule.Id >= 0);

            using (IUnitOfWork uow = this.GetUnitOfWork())
            {
                IRepository<Schedule> repo = uow.GetRepository<Schedule>();
                schedule = repo.Reload(schedule);
                repo.Delete(schedule);
                uow.Commit();
            }

            return true;
        }


        public Schedule UpdateSchedule(Schedule schedule)
        {
            Contract.Requires(schedule != null);
            //Update ForPerson before


            using (IUnitOfWork uow = this.GetUnitOfWork())
            {
                IRepository<Schedule> repo = uow.GetRepository<Schedule>();
                repo.Merge(schedule);
                var merged = repo.Get(schedule.Id);
                repo.Put(merged);
                uow.Commit();
            }
            return schedule;

        }

        public IQueryable<Schedule> GetAllSchedules()
        {
            return ScheduleRepo.Query();
        }

        public Schedule GetScheduleById(long id)
        {
            return ScheduleRepo.Query(a => a.Id == id).FirstOrDefault();
        }

        public List<Schedule> GetAllSchedulesByEvent(long eventId)
        {
            return ScheduleRepo.Query(a=>a.Event.Id == eventId).ToList();
        }

        public bool RemoveAllSchedulesByEvent(long eventId)
        {
            PersonManager pManager = new PersonManager();
            List<Schedule> list = ScheduleRepo.Query(a => a.Event.Id == eventId).ToList();
            foreach (Schedule s in list)
            {
                using (IUnitOfWork uow = this.GetUnitOfWork())
                {
                    Person tempForPerson = s.ForPerson;
                    Person tempCreatedBy = s.ByPerson;
                    IRepository<Schedule> repo = uow.GetRepository<Schedule>();
                    Schedule deletedSchedule = s;
                    deletedSchedule = repo.Reload(s);
                    repo.Delete(deletedSchedule);
                    uow.Commit();
                    //Delete Persons
                    pManager.DeletePerson(tempForPerson);
                    pManager.DeletePerson(tempCreatedBy);
                }
            }

            return true;
        }

        public List<Schedule> GetAllSchedulesByResource(long resourceId)
        {
            return ScheduleRepo.Query(a=>a.Resource.Id == resourceId).ToList();
        }

        public List<Schedule> GetSchedulesBetweenStartAndEndDate(DateTime startDate, DateTime endDate)
        {
            return ScheduleRepo.Query(a => ((DateTime)a.StartDate >= startDate && (DateTime)a.StartDate <= endDate) || ((DateTime)a.EndDate >= startDate && (DateTime)a.EndDate <= endDate)).ToList();
        }

        public List<Schedule> GetSchedulesBetweenStartAndEndDate(DateTime startDate, DateTime endDate, List<long> eventIds)
        {
            return ScheduleRepo.Query(a => (eventIds.Contains(a.Event.Id)) && (((DateTime)a.StartDate >= startDate && (DateTime)a.StartDate <= endDate) || ((DateTime)a.EndDate >= startDate && (DateTime)a.EndDate <= endDate))).ToList();
        }

        #endregion
    }
}
