using BExIS.Rbm.Entities.BookingManagementTime;
using BExIS.Rbm.Entities.Resource;
using BExIS.Rbm.Entities.ResourceConstraint;
using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Text;
using Vaiona.Persistence.Api;
using R = BExIS.Rbm.Entities.Resource;

namespace BExIS.Rbm.Services.ResourceConstraints
{
    public class ResourceConstraintManager
    {
        public ResourceConstraintManager()
        {
            IUnitOfWork uow = this.GetUnitOfWork();
            this.ResourceConstraintRepo = uow.GetReadOnlyRepository<ResourceConstraint>();
            this.DependencyConstraintRepo = uow.GetReadOnlyRepository<DependencyConstraint>();
            this.BlockingConstraintRepo = uow.GetReadOnlyRepository<BlockingConstraint>();
            this.QuantityConstraintRepo = uow.GetReadOnlyRepository<QuantityConstraint>();
            this.TimeCapacityConstraintRepo = uow.GetReadOnlyRepository<TimeCapacityConstraint>();
        }

        #region Data Readers

        public IReadOnlyRepository<ResourceConstraint> ResourceConstraintRepo { get; private set; }
        public IReadOnlyRepository<DependencyConstraint> DependencyConstraintRepo { get; private set; }
        public IReadOnlyRepository<BlockingConstraint> BlockingConstraintRepo { get; private set; }
        public IReadOnlyRepository<QuantityConstraint> QuantityConstraintRepo { get; private set; }
        public IReadOnlyRepository<TimeCapacityConstraint> TimeCapacityConstraintRepo { get; private set; }

        #endregion

        #region ResourceConstraint

        public ResourceConstraint GetResourceConstraintById(long id)
        {
            return ResourceConstraintRepo.Query(a => a.Id == id).FirstOrDefault();
        }

        #endregion

        #region DependencyConstraint

        public DependencyConstraint SaveConstraint(DependencyConstraint constraint)
        {
            
            //if(timePeriod != null)
            //    constraint.TimePeriod = timePeriod;

            using (IUnitOfWork uow = this.GetUnitOfWork())
            {
                IRepository<DependencyConstraint> repo = uow.GetRepository<DependencyConstraint>();
                repo.Put(constraint);
                uow.Commit();
            }
            return constraint;
        }


        public bool DeleteConstraint(DependencyConstraint constraint)
        {
            Contract.Requires(constraint != null);
            Contract.Requires(constraint.Id >= 0);

            using (IUnitOfWork uow = this.GetUnitOfWork())
            {
                IRepository<DependencyConstraint> repo = uow.GetRepository<DependencyConstraint>();
                constraint = repo.Reload(constraint);
                repo.Delete(constraint);
                uow.Commit();
            }
            return (true);
        }

        public DependencyConstraint UpdateConstraint(DependencyConstraint constraint)
        {
            Contract.Requires(constraint != null, "provided entity can not be null");
            Contract.Requires(constraint.Id >= 0, "provided entity must have a permanent ID");

            Contract.Ensures(Contract.Result<DependencyConstraint>() != null && Contract.Result<DependencyConstraint>().Id >= 0, "No entity is persisted!");

            using (IUnitOfWork uow = this.GetUnitOfWork())
            {
                IRepository<DependencyConstraint> repo = uow.GetRepository<DependencyConstraint>();
                repo.Put(constraint);
                uow.Commit();
            }
            return constraint;
        }

        public DependencyConstraint GetDependencyConstraintById(long id)
        {
            return DependencyConstraintRepo.Query(a => a.Id == id).FirstOrDefault();
        }


        #endregion

        #region BlockingConstraint

        public BlockingConstraint SaveConstraint(BlockingConstraint constraint)
        {
            //if (timePeriod != null)
            //    constraint.TimePeriod = timePeriod;

            using (IUnitOfWork uow = this.GetUnitOfWork())
            {
                IRepository<BlockingConstraint> repo = uow.GetRepository<BlockingConstraint>();
                repo.Put(constraint);
                uow.Commit();
            }
            return constraint;
        }


        public bool DeleteConstraint(BlockingConstraint constraint)
        {
            Contract.Requires(constraint != null);
            Contract.Requires(constraint.Id >= 0);

            using (IUnitOfWork uow = this.GetUnitOfWork())
            {
                IRepository<BlockingConstraint> repo = uow.GetRepository<BlockingConstraint>();
                constraint = repo.Reload(constraint);
                repo.Delete(constraint);
                uow.Commit();
            }
            return (true);
        }

        public BlockingConstraint UpdateConstraint(BlockingConstraint constraint)
        {
            Contract.Requires(constraint != null, "provided entity can not be null");
            Contract.Requires(constraint.Id >= 0, "provided entity must have a permanent ID");

            Contract.Ensures(Contract.Result<BlockingConstraint>() != null && Contract.Result<BlockingConstraint>().Id >= 0, "No entity is persisted!");

            using (IUnitOfWork uow = this.GetUnitOfWork())
            {
                IRepository<BlockingConstraint> repo = uow.GetRepository<BlockingConstraint>();
                repo.Put(constraint);
                uow.Commit();
            }
            return constraint;
        }

        public BlockingConstraint GetBlockingConstraintById(long id)
        {
            return BlockingConstraintRepo.Query(a => a.Id == id).FirstOrDefault();
        }

        #endregion

        #region QuantityConstraint

        public QuantityConstraint SaveConstraint(QuantityConstraint constraint)
        {

            using (IUnitOfWork uow = this.GetUnitOfWork())
            {
                IRepository<QuantityConstraint> repo = uow.GetRepository<QuantityConstraint>();
                repo.Put(constraint);
                uow.Commit();
            }
            return constraint;
        }


        public bool DeleteConstraint(QuantityConstraint constraint)
        {
            Contract.Requires(constraint != null);
            Contract.Requires(constraint.Id >= 0);

            using (IUnitOfWork uow = this.GetUnitOfWork())
            {
                IRepository<QuantityConstraint> repo = uow.GetRepository<QuantityConstraint>();
                constraint = repo.Reload(constraint);
                repo.Delete(constraint);
                uow.Commit();
            }
            return (true);
        }

        public QuantityConstraint UpdateConstraint(QuantityConstraint constraint)
        {
            Contract.Requires(constraint != null, "provided entity can not be null");
            Contract.Requires(constraint.Id >= 0, "provided entity must have a permanent ID");

            Contract.Ensures(Contract.Result<QuantityConstraint>() != null && Contract.Result<QuantityConstraint>().Id >= 0, "No entity is persisted!");

            using (IUnitOfWork uow = this.GetUnitOfWork())
            {
                IRepository<QuantityConstraint> repo = uow.GetRepository<QuantityConstraint>();
                repo.Put(constraint);
                uow.Commit();
            }
            return constraint;
        }

        public QuantityConstraint GetQuantityConstraintById(long id)
        {
            return QuantityConstraintRepo.Query(a => a.Id == id).FirstOrDefault();
        }


        #endregion

        #region TimeCapacityConstraint

        public TimeCapacityConstraint SaveConstraint(TimeCapacityConstraint constraint)
        {

            using (IUnitOfWork uow = this.GetUnitOfWork())
            {
                IRepository<TimeCapacityConstraint> repo = uow.GetRepository<TimeCapacityConstraint>();
                repo.Put(constraint);
                uow.Commit();
            }
            return constraint;
        }


        public bool DeleteConstraint(TimeCapacityConstraint constraint)
        {
            Contract.Requires(constraint != null);
            Contract.Requires(constraint.Id >= 0);

            using (IUnitOfWork uow = this.GetUnitOfWork())
            {
                IRepository<TimeCapacityConstraint> repo = uow.GetRepository<TimeCapacityConstraint>();
                constraint = repo.Reload(constraint);
                repo.Delete(constraint);
                uow.Commit();
            }
            return (true);
        }

        public TimeCapacityConstraint UpdateConstraint(TimeCapacityConstraint constraint)
        {
            Contract.Requires(constraint != null, "provided entity can not be null");
            Contract.Requires(constraint.Id >= 0, "provided entity must have a permanent ID");

            Contract.Ensures(Contract.Result<TimeCapacityConstraint>() != null && Contract.Result<TimeCapacityConstraint>().Id >= 0, "No entity is persisted!");

            using (IUnitOfWork uow = this.GetUnitOfWork())
            {
                IRepository<TimeCapacityConstraint> repo = uow.GetRepository<TimeCapacityConstraint>();
                repo.Put(constraint);
                uow.Commit();
            }
            return constraint;
        }

        public TimeCapacityConstraint GetTimeCapacityConstraintById(long id)
        {
            return TimeCapacityConstraintRepo.Query(a => a.Id == id).FirstOrDefault();
        }

        #endregion

    }
}
