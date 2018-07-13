using BExIS.Dlm.Entities.DataStructure;
using BExIS.Rbm.Entities.BookingManagementTime;
using BExIS.Rbm.Entities.ResourceConstraint;
using BExIS.Rbm.Entities.Users;
using BExIS.Web.Shell.Areas.RBM.Models.Booking;
using BExIS.Web.Shell.Areas.RBM.Models.BookingManagementTime;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using R = BExIS.Rbm.Entities.Resource;

namespace BExIS.Web.Shell.Areas.RBM.Models.Resource
{

    public class ResourceConstraintModel
    {
        public long Id { get; set; }
        public R.Resource Resource { get; set; }
        public int Index { get; set; }
        public bool Negated { get; set; }

        public List<ConstraintMode> Mode {get; set;}
        public ConstraintMode SelectedMode { get; set; }

        public string SelectedType { get; set; }
        public string Description { get; set; }

        public List<PersonInConstraint> ForPersons { get; set; }

        public PeriodicTimeIntervalModel ForPeriodicTimeInterval { get; set; }

        public TimeInterval ForTimeInterval { get; set; }

        public bool AllUsers { get; set; }
        public bool ForEver { get; set; }

        public List<string> ConstraintType { get; set; }
        public string ConstraintObjectType { get; set; }

        public bool Deleted { get; set; }

        public ResourceConstraintModel()
        {
            Index = 0;
            ConstraintType = new List<string>();
            ConstraintType.Add("Dependency");
            ConstraintType.Add("Blocking");
            ConstraintType.Add("Quantity");

            Mode = new List<ConstraintMode>();
            Mode.Add(ConstraintMode.all);
            Mode.Add(ConstraintMode.each);

            ForPersons = new List<PersonInConstraint>();
            ForPeriodicTimeInterval = new PeriodicTimeIntervalModel();

        }

        public ResourceConstraintModel(ResourceConstraint resourceConstraint)
        {
            Id = resourceConstraint.Id;
            Index = resourceConstraint.Index;
            Negated = resourceConstraint.Negated;
            SelectedMode = resourceConstraint.Mode;
            Description = resourceConstraint.Description;

            ForPersons = new List<PersonInConstraint>();


            ConstraintType = new List<string>();
            ConstraintType.Add("Dependency");
            ConstraintType.Add("Blocking");
            ConstraintType.Add("Quantity");


            Mode = new List<ConstraintMode>();
            Mode.Add(ConstraintMode.all);
            Mode.Add(ConstraintMode.each);
        }

    }


    public class DependencyConstraintModel : ResourceConstraintModel
    {

        public bool Implicit { get; set; }

        public int Quantity { get; set; }
        public ComparisonOperator QuantityComparisonOperator { get; set; }

        public R.Resource OnResource { get; set; }

        public List<string> ResourceObjects { get; set; }
        public string ObjectName { get; set; }
        public string ObjectId { get; set; }

        public DependencyConstraintModel()
        {
            Implicit = false;
            Quantity = 1;
            QuantityComparisonOperator = ComparisonOperator.Equals;

            ResourceObjects = new List<string>();
            ResourceObjects.Add("Single Resource");
            //ResourceObjects.Add("Resource Group");
        }

        public DependencyConstraintModel(DependencyConstraint constraint)
        {
            Id = constraint.Id;
            Index = constraint.Index;
            Negated = constraint.Negated;
            SelectedMode = constraint.Mode;
            Description = constraint.Description;

            if (constraint.ForPeriodicTimeInterval != null)
            {
                if (constraint.ForPeriodicTimeInterval.Id != 0)
                {
                    ForPeriodicTimeInterval = new PeriodicTimeIntervalModel(constraint.ForPeriodicTimeInterval, (DateTime)constraint.ForTimeInterval.StartTime.Instant, (DateTime)constraint.ForTimeInterval.EndTime.Instant);
                    ForPeriodicTimeInterval.IsSet = true;
                    ForPeriodicTimeInterval.Index = constraint.Index;
                }
            }
            else
                ForPeriodicTimeInterval = new PeriodicTimeIntervalModel();

            ResourceObjects = new List<string>();
            ResourceObjects.Add("Single Resource");
            Implicit = constraint.Implicit;
            Quantity = constraint.Quantity;
            QuantityComparisonOperator = constraint.QuantityComparisonOperator;
            ObjectName = constraint.ForResource.Name;
            ObjectId = constraint.ForResource.Id.ToString();


            
        }
    }

    public class BlockingConstraintModel : ResourceConstraintModel
    {
        public BlockingConstraintModel()
        {
            ForTimeInterval = new TimeInterval();
            ForPeriodicTimeInterval = new PeriodicTimeIntervalModel();
        }

        public BlockingConstraintModel(BlockingConstraint constraint)
        {
            Id = constraint.Id;
            Index = constraint.Index;
            Negated = constraint.Negated;
            SelectedMode = constraint.Mode;
            Description = constraint.Description;

            AllUsers = constraint.AllUsers;
            ForEver = constraint.ForEver;

            ForTimeInterval = new TimeInterval();

            if (constraint.ForPeriodicTimeInterval != null)
            {
                if (constraint.ForPeriodicTimeInterval.Id != 0)
                {
                    ForPeriodicTimeInterval = new PeriodicTimeIntervalModel(constraint.ForPeriodicTimeInterval, (DateTime)constraint.ForTimeInterval.StartTime.Instant, constraint.ForTimeInterval.EndTime.Instant);
                    ForPeriodicTimeInterval.IsSet = true;
                    ForPeriodicTimeInterval.Index = constraint.Index;
                }
                else
                    ForPeriodicTimeInterval = new PeriodicTimeIntervalModel();
            }

            if (constraint.ForPerson != null)
            {
                if (constraint.ForPerson.Self is PersonGroup)
                {
                    PersonGroup pGroup = (PersonGroup)constraint.ForPerson.Self;
                   // pGroup.Users.ToList().ForEach(u => ForPersons.Add(new PersonInConstraint(u, pGroup.Id, constraint.Index)));
                }
                else if (constraint.ForPerson.Self is IndividualPerson)
                {
                    IndividualPerson iPerson = (IndividualPerson)constraint.ForPerson.Self;
                    //ForPersons.Add(new PersonInConstraint(iPerson.Person, iPerson.Id, constraint.Index));
                }
            }

            if (constraint.ForTimeInterval != null)
            {
                TimeInterval timeInterval = new TimeInterval();
                timeInterval = constraint.ForTimeInterval.Self;
                timeInterval.StartTime = constraint.ForTimeInterval.StartTime.Self;
                timeInterval.EndTime = constraint.ForTimeInterval.EndTime.Self;
                ForTimeInterval = timeInterval;
            }
            else
                ForTimeInterval = new TimeInterval();
        }

    }

    public class QuantityConstraintModel : ResourceConstraintModel
    {
        public int Quantity { get; set; }
        public ComparisonOperator ComparisonOperator { get; set; }

        public QuantityConstraintModel()
        {
            ForTimeInterval = new TimeInterval();
            Quantity = 0;
            ComparisonOperator = ComparisonOperator.Equals;
        }

        public QuantityConstraintModel(QuantityConstraint constraint)
        {
            Id = constraint.Id;
            Index = constraint.Index;
            Quantity = constraint.Quantity;
            ComparisonOperator = constraint.ComparisonOperator;
            Negated = constraint.Negated;
            SelectedMode = constraint.Mode;
            Description = constraint.Description;

            AllUsers = constraint.AllUsers;
            ForEver = constraint.ForEver;

            if (constraint.ForPeriodicTimeInterval != null)
            {
                ForPeriodicTimeInterval = new PeriodicTimeIntervalModel(constraint.ForPeriodicTimeInterval, (DateTime)constraint.ForTimeInterval.StartTime.Instant, constraint.ForTimeInterval.EndTime.Instant);
                ForPeriodicTimeInterval.IsSet = true;
                ForPeriodicTimeInterval.Index = constraint.Index;
            }
            else
                ForPeriodicTimeInterval = new PeriodicTimeIntervalModel();

            ForTimeInterval = new TimeInterval();

            if (constraint.ForPerson != null)
            {
                if (constraint.ForPerson.Self is PersonGroup)
                {
                    PersonGroup pGroup = (PersonGroup)constraint.ForPerson.Self;
                    //pGroup.Users.ToList().ForEach(u => ForPersons.Add(new PersonInConstraint(u, pGroup.Id, constraint.Index)));
                }
                else if (constraint.ForPerson.Self is IndividualPerson)
                {
                    IndividualPerson iPerson = (IndividualPerson)constraint.ForPerson.Self;
                    //ForPersons.Add(new PersonInConstraint(iPerson.Person, iPerson.Id, constraint.Index));
                }
            }

            if (constraint.ForTimeInterval != null)
            {
                TimeInterval timeInterval = new TimeInterval();
                timeInterval = constraint.ForTimeInterval.Self;
                timeInterval.StartTime = constraint.ForTimeInterval.StartTime.Self;
                timeInterval.EndTime = constraint.ForTimeInterval.EndTime.Self;
                ForTimeInterval = timeInterval;
            }
    
        }

    }


    public class TimeCapacityConstraintModel : ResourceConstraintModel
    {
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }

        //public TimePeriod MaxTimePeriod { get; set; }

        public TimeCapacityConstraintModel()
        {
            StartDate = new DateTime();
            EndDate = new DateTime();
            //MaxTimePeriod = new TimePeriod();
        }

        public TimeCapacityConstraintModel(TimeCapacityConstraint constraint)
        {
            //StartDate = constraint.StartDate;
            //EndDate = constraint.EndDate;
           // MaxTimePeriod = constraint.MaxTimePeriod;
        }

    }

}