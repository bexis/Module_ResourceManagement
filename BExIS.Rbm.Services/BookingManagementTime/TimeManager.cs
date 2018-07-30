using BExIS.Rbm.Entities.BookingManagementTime;
using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vaiona.Persistence.Api;

namespace BExIS.Rbm.Services.BookingManagementTime
{
    public class TimeManager : IDisposable
    {
        private readonly IUnitOfWork _guow;
        private bool _isDisposed;

        public TimeManager()
        {
            _guow = this.GetIsolatedUnitOfWork();
            this.TimeInstantRepo = _guow.GetReadOnlyRepository<TimeInstant>();
            this.TimeIntervalRepo = _guow.GetReadOnlyRepository<TimeInterval>();
            this.TimeDurationRepo = _guow.GetReadOnlyRepository<TimeDuration>();
            this.PeriodicTimeInstantRepo = _guow.GetReadOnlyRepository<PeriodicTimeInstant>();
            this.PeriodicTimeIntervalRepo = _guow.GetReadOnlyRepository<PeriodicTimeInterval>();
        }

        ~TimeManager()
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

        public IReadOnlyRepository<TimeInstant> TimeInstantRepo { get; private set; }
        public IReadOnlyRepository<TimeInterval> TimeIntervalRepo { get; private set; }
        public IReadOnlyRepository<TimeDuration> TimeDurationRepo { get; private set; }
        public IReadOnlyRepository<PeriodicTimeInterval> PeriodicTimeIntervalRepo { get; private set; }
        public IReadOnlyRepository<PeriodicTimeInstant> PeriodicTimeInstantRepo { get; private set; }

        #endregion

        #region TimeInstant

        public TimeInstant CreateTimeInstant(SystemDefinedUnit precision, DateTime? instant)
        {
            TimeInstant timeInstant = new TimeInstant()
            {
                Precision = precision,
                Instant = instant
            };

            using (IUnitOfWork uow = this.GetUnitOfWork())
            {
                IRepository<TimeInstant> repo = uow.GetRepository<TimeInstant>();
                repo.Put(timeInstant);
                uow.Commit();
            }
            return timeInstant;
        }

        public bool DeleteTimeInstant(TimeInstant timeInstant)
        {
            Contract.Requires(timeInstant != null);
            Contract.Requires(timeInstant.Id >= 0);

            using (IUnitOfWork uow = this.GetUnitOfWork())
            {
                IRepository<TimeInstant> repo = uow.GetRepository<TimeInstant>();
                timeInstant = repo.Reload(timeInstant);
                repo.Delete(timeInstant);
                uow.Commit();
            }
            return true;
        }

        public TimeInstant UpdateTimeInstant(TimeInstant timeInstant)
        {
            Contract.Requires(timeInstant != null);
            using (IUnitOfWork uow = this.GetUnitOfWork())
            {
                IRepository<TimeInstant> repo = uow.GetRepository<TimeInstant>();
                repo.Merge(timeInstant);
                var merged = repo.Get(timeInstant.Id);
                repo.Put(merged);
                uow.Commit();
            }
            return timeInstant;
        }

        #endregion

        #region TimeInterval

        public TimeInterval CreateTimeInterval(TimeInstant startTime, TimeInstant endTime)
        {
            TimeInterval timeInterval = new TimeInterval()
            {
                StartTime = startTime,
                EndTime = endTime
            };

            using (IUnitOfWork uow = this.GetUnitOfWork())
            {
                IRepository<TimeInterval> repo = uow.GetRepository<TimeInterval>();
                repo.Put(timeInterval);
                uow.Commit();
            }
            return timeInterval;
        }

        public bool DeleteTimeInstant(TimeInterval timeInterval)
        {
            Contract.Requires(timeInterval != null);
            Contract.Requires(timeInterval.Id >= 0);

            using (IUnitOfWork uow = this.GetUnitOfWork())
            {
                IRepository<TimeInterval> repo = uow.GetRepository<TimeInterval>();
                timeInterval = repo.Reload(timeInterval);
                repo.Delete(timeInterval);
                uow.Commit();
            }
            return true;
        }

        public TimeInterval UpdateTimeInterval(TimeInterval timeInterval)
        {
            Contract.Requires(timeInterval != null);
            using (IUnitOfWork uow = this.GetUnitOfWork())
            {
                IRepository<TimeInterval> repo = uow.GetRepository<TimeInterval>();
                repo.Merge(timeInterval);
                var merged = repo.Get(timeInterval.Id);
                repo.Put(merged);
                uow.Commit();
            }
            return timeInterval;
        }

        #endregion

        #region TimeDuration

        public TimeDuration CreateTimeInstant(SystemDefinedUnit unit, int value)
        {
            TimeDuration timeDuration = new TimeDuration()
            {
                TimeUnit = unit,
                Value = value
            };

            using (IUnitOfWork uow = this.GetUnitOfWork())
            {
                IRepository<TimeDuration> repo = uow.GetRepository<TimeDuration>();
                repo.Put(timeDuration);
                uow.Commit();
            }
            return timeDuration;
        }

        public bool DeleteTimeInstant(TimeDuration timeDuration)
        {
            Contract.Requires(timeDuration != null);
            Contract.Requires(timeDuration.Id >= 0);

            using (IUnitOfWork uow = this.GetUnitOfWork())
            {
                IRepository<TimeDuration> repo = uow.GetRepository<TimeDuration>();
                timeDuration = repo.Reload(timeDuration);
                repo.Delete(timeDuration);
                uow.Commit();
            }
            return true;
        }

        public TimeDuration UpdateTimeInterval(TimeDuration timeDuration)
        {
            Contract.Requires(timeDuration != null);
            using (IUnitOfWork uow = this.GetUnitOfWork())
            {
                IRepository<TimeDuration> repo = uow.GetRepository<TimeDuration>();
                repo.Merge(timeDuration);
                var merged = repo.Get(timeDuration.Id);
                repo.Put(merged);
                uow.Commit();
            }
            return timeDuration;
        }

        #endregion

        #region PeriodicTimeInstant

        public PeriodicTimeInstant CreatePeriodicTimeInstantt(ResetFrequency resetFrequency, int resetInterval, int offset, SystemDefinedUnit offsetUnit)
        {
            PeriodicTimeInstant periodicTimeInstant = new PeriodicTimeInstant()
            {
                ResetFrequency = resetFrequency,
                ResetInterval = resetInterval,
                Off_Set = offset,
                Off_Set_Unit = offsetUnit
            };

            using (IUnitOfWork uow = this.GetUnitOfWork())
            {
                IRepository<PeriodicTimeInstant> repo = uow.GetRepository<PeriodicTimeInstant>();
                repo.Put(periodicTimeInstant);
                uow.Commit();
            }
            return periodicTimeInstant;
        }

        public bool DeletePeriodicTimeInstant(PeriodicTimeInstant periodicTimeInstant)
        {
            Contract.Requires(periodicTimeInstant != null);
            Contract.Requires(periodicTimeInstant.Id >= 0);

            using (IUnitOfWork uow = this.GetUnitOfWork())
            {
                IRepository<PeriodicTimeInstant> repo = uow.GetRepository<PeriodicTimeInstant>();
                periodicTimeInstant = repo.Reload(periodicTimeInstant);
                repo.Delete(periodicTimeInstant);
                uow.Commit();
            }
            return true;
        }

        public PeriodicTimeInstant UpdateTimeInterval(PeriodicTimeInstant periodicTimeInstant)
        {
            Contract.Requires(periodicTimeInstant != null);
            using (IUnitOfWork uow = this.GetUnitOfWork())
            {
                IRepository<PeriodicTimeInstant> repo = uow.GetRepository<PeriodicTimeInstant>();
                repo.Merge(periodicTimeInstant);
                var merged = repo.Get(periodicTimeInstant.Id);
                repo.Put(merged);
                uow.Commit();
            }
            return periodicTimeInstant;
        }

        #endregion

        #region PeriodicTimeInterval

        public PeriodicTimeInterval CreatePeriodicTimeInterval(PeriodicTimeInstant periodicTimeInstant, TimeDuration duration)
        {
            PeriodicTimeInterval periodicTimeInterval = new PeriodicTimeInterval()
            {
                PeriodicTimeInstant = periodicTimeInstant,
                Duration = duration
            };

            using (IUnitOfWork uow = this.GetUnitOfWork())
            {
                IRepository<PeriodicTimeInterval> repo = uow.GetRepository<PeriodicTimeInterval>();
                repo.Put(periodicTimeInterval);
                uow.Commit();
            }
            return periodicTimeInterval;
        }

        public bool DeletePeriodicTimeInterval(PeriodicTimeInterval periodicTimeInterval)
        {
            Contract.Requires(periodicTimeInterval != null);
            Contract.Requires(periodicTimeInterval.Id >= 0);

            using (IUnitOfWork uow = this.GetUnitOfWork())
            {
                IRepository<PeriodicTimeInterval> repo = uow.GetRepository<PeriodicTimeInterval>();
                periodicTimeInterval = repo.Reload(periodicTimeInterval);
                repo.Delete(periodicTimeInterval);
                uow.Commit();
            }
            return true;
        }

        public PeriodicTimeInterval UpdatePeriodicTimeInterval(PeriodicTimeInterval periodicTimeInterval)
        {
            Contract.Requires(periodicTimeInterval != null);
            using (IUnitOfWork uow = this.GetUnitOfWork())
            {
                IRepository<PeriodicTimeInterval> repo = uow.GetRepository<PeriodicTimeInterval>();
                repo.Put(periodicTimeInterval);
                uow.Commit();
            }
            return periodicTimeInterval;
        }

        public PeriodicTimeInterval GetPeriodicTimeIntervalById(long id)
        {
            return PeriodicTimeIntervalRepo.Query(a => a.Id == id).FirstOrDefault();
        }


        #endregion
    }
}
