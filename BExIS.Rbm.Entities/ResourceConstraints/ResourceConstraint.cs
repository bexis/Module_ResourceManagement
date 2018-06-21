using BExIS.Rbm.Entities.BookingManagementTime;
using R = BExIS.Rbm.Entities.Resource;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Vaiona.Entities.Common;
using BExIS.Rbm.Entities.Users;
using BExIS.Dlm.Entities.DataStructure;
using System.Linq.Expressions;
using BExIS.Rbm.Entities.Booking;

namespace BExIS.Rbm.Entities.ResourceConstraint
{

    public enum ConstraintMode
    {
        each = 0,
        all = 1
    }

    public abstract class ResourceConstraint : Constraint
    {
        #region Attributes

        /// <summary>
        /// Specify if this applies to each/any one of the group or the group alltogether
        /// </summary>
        public virtual ConstraintMode Mode { get; set; }

        /// <summary>
        /// Index for the sequence of the constrains in one resource.
        /// </summary> 
        public virtual int Index { get; set; }


        #endregion


        #region Associations

        /// <summary>
        /// The <see cref="Resource"/> the constraint applies on.
        /// </summary>
        public virtual R.Resource Resource { get; set; }

        /// <summary>
        /// Dependent <see cref="Resource"/> the constraint applies for.
        /// </summary>
        public virtual R.Resource ForResource { get; set; }

        /// <summary>
        /// The <see cref="TimeInterval"/> the constraint applies for.
        /// </summary>
        public virtual TimeInterval ForTimeInterval { get; set; }

        /// <summary>
        /// The <see cref="PeriodicTimeInterval"/> (Repetion) the constraint applies for.
        /// </summary>
        public virtual PeriodicTimeInterval ForPeriodicTimeInterval { get; set; }

        /// <summary>
        /// Set it true if the constraint applies from the time perspective for ever.
        /// </summary>
        public virtual bool ForEver { get; set; }

        public virtual bool AllUsers { get; set; }

        /// <summary>
        /// The <see cref="Person"/> the constraint applies for.
        /// </summary>
        public virtual Person ForPerson { get; set; }

        #endregion

        #region Methods



        #endregion
    }


    public class DependencyConstraint : ResourceConstraint
    {
        #region Attributes

        /// <summary>
        ///  Automatically books the dependant resources
        /// </summary>
        public virtual bool Implicit  { get; set; }

        /// <summary>
        ///  Quantity of the depended resources, e.g. if you book a car you need 2 safety vest
        /// </summary>
        public virtual int Quantity { get; set; }

        /// <summary>
        ///  Comparison operator for the quantity of the depended resources, e.g. if you book a car you exsact/min or max 2 safety vest
        /// </summary>
        public virtual ComparisonOperator QuantityComparisonOperator { get; set; }

        /// <summary>
        ///
        /// </summary>
        /// <remarks></remarks>
        /// <seealso cref=""/>        
        public override string ErrorMessage
        {
            get
            {
                if (Negated)
                {
                    return (string.Format(
                        (!string.IsNullOrWhiteSpace(NegatedMessageTemplate) ? NegatedMessageTemplate : defaultNegatedMessageTemplate),
                        (Implicit ? "implicit" : "not  implicit")));
                }
                else
                {
                    return (string.Format(
                        (!string.IsNullOrWhiteSpace(MessageTemplate) ? MessageTemplate : defaultMessageTemplate),
                         (Implicit ? "implicit" : "not  implicit")));
                }
            }
        }

        public override string FormalDescription
        {
            get
            {
                if (Negated)
                {
                    return (string.Format("You have to book this resource: '{0}' together with this resource: {1}. The dependency is {2}.", Resource, ForResource, (Implicit ? "Implicit" : "not implicit")));
                }
                else
                {
                    return (string.Format("You have to book this resource: '{0}' together with this resource: {1}. The dependency is {2}.", Resource, ForResource, (Implicit ? "Implicit" : "not implicit")));
                }
            }
        }

        #endregion

        #region Associations

        #endregion

        #region Methods

        public DependencyConstraint()
        {
            Mode = ConstraintMode.all;
            Description = "";
            Quantity = 1;
            defaultMessageTemplate = "If you book this resource you have to book additionally another resource(s)";
            defaultNegatedMessageTemplate = "If you book this resource you have to book additionally another resource(s), but the constraint is negated. You can not book the resource.";
        }

        /// <summary>
        ///
        /// </summary>
        /// <remarks></remarks>
        /// <seealso cref=""/>
        /// <param name="data"></param>
        /// <param name="auxiliary"></param>
        public override bool IsSatisfied(object data, object auxiliary = null)
        {
            bool isSatisfied = true;
            ResourceConstraintData cData = (ResourceConstraintData)data;

            int countdependentQuantity = 0;

            var dependentResources = cData.SchedulesInEvent.Where(a => a.Resource.Id == ForResource.Id).ToList();

            if (dependentResources.Count() > 0)
            {
                var q = dependentResources.Where(a => a.Quantity > 1).Select(a => a.Quantity).ToList().Sum();
                countdependentQuantity = dependentResources.Count() * q;
            }

            //check if the time period for the resource in this schedule match with the time period from the depented resources
            foreach(Schedule s in cData.SchedulesInEvent)
            {
                if(!ConstraintCheckHelpers.TimeMatchComplete(s.StartDate, s.EndDate, cData.TimePeriodInSchedule.StartTime.Instant.Value, cData.TimePeriodInSchedule.EndTime.Instant.Value))
                {
                    isSatisfied = false;
                    MessageTemplate += String.Format("The booking time period for the resource {0} must fit with this time period. \r\n", s.Resource.Name);
                }
            }

            if (Quantity == 1)
            {
                if (dependentResources.Count() == 0)
                {
                    isSatisfied = false;

                    //if you book more then 1 of the resouce which have a Dependency then you need also more depend resources
                    if (cData.QuantityInSchedule > 1)
                        MessageTemplate += String.Format("If you book this resource you have to book additionally this resource {0} {1} times.", ForResource.Name, cData.QuantityInSchedule);
                    else
                        MessageTemplate += String.Format("If you book this resource you have to book additionally this resource {0} one times.", ForResource.Name);
                }
                else if(countdependentQuantity != cData.QuantityInSchedule)
                {
                    isSatisfied = false;
                    MessageTemplate += String.Format("If you book this resource with a Quanity of {2} you have to book additionally this resource {0} {1} times.", ForResource.Name, cData.QuantityInSchedule, Quantity);
                }
            }
            else
            {
                //Constraint quantity multiply in schedule qunatity from the source resource result in the quantiry that you need from the dependent resource
                int neededQuantity = Quantity * cData.QuantityInSchedule;

                if (dependentResources.Count() > 0)
                {
                    switch (QuantityComparisonOperator)
                    {
                        case ComparisonOperator.Equals:
                            {
                                if (neededQuantity != countdependentQuantity)
                                {
                                    isSatisfied = false;
                                    MessageTemplate += String.Format("If you book this resource {2} times you have to book additionally this resource {0} with a Quantity of {1}", ForResource.Name, neededQuantity, cData.QuantityInSchedule);
                                }
                                break;
                            }
                    }
                }
                else
                {
                    isSatisfied = false;
                    MessageTemplate = String.Format("If you book this resource {2} times you have to book additionally this resource {0} with a Quantity of {1}", ForResource.Name, neededQuantity, cData.QuantityInSchedule);
                }
            }

            return isSatisfied;
        }

        #endregion
    }


    public class BlockingConstraint : ResourceConstraint
    {
        #region Attributes

        /// <summary>
        ///
        /// </summary>
        /// <remarks></remarks>
        /// <seealso cref=""/>        
        public override string ErrorMessage
        {
            get
            {
                if (Negated)
                {
                    return (string.Format(
                        (!string.IsNullOrWhiteSpace(NegatedMessageTemplate) ? NegatedMessageTemplate : defaultNegatedMessageTemplate)));
                }
                else
                {
                    return (string.Format(
                        (!string.IsNullOrWhiteSpace(MessageTemplate) ? MessageTemplate : defaultMessageTemplate)));
                }
            }
        }

        public override string FormalDescription
        {
            get
            {
                if (Negated)
                {
                    return (string.Format("This resource is blocked for thsis Persons: '{0}' and in this time period {1}", ForPerson, ForTimeInterval));
                }
                else
                {
                    return (string.Format("This resource is blocked for thsis Persons: '{0}' and in this time period {1}", ForPerson, ForTimeInterval));
                }
            }
        }

        #endregion

        #region Associations

        #endregion

        #region Methods

        public BlockingConstraint()
        {
            defaultMessageTemplate = "The resource is blocked.";
            defaultNegatedMessageTemplate = "The resource is blocked, but the constraint is negated. The value should not be one of these items: {0}.";
        }

        /// <summary>
        ///
        /// </summary>
        /// <remarks></remarks>
        /// <seealso cref=""/>
        /// <param name="data"></param>
        /// <param name="auxiliary"></param>
        public override bool IsSatisfied(object data, object auxiliary = null)
        {
            ResourceConstraintData cData = (ResourceConstraintData)data;

            bool isSatisfied = true;
            bool peopleMatch = false;
            bool timeMatchComplete = false;

            if (ForPerson != null && ForTimeInterval != null)
            {
                peopleMatch = ConstraintCheckHelpers.PeopleMatch(ForPerson, cData.PersonsInSchedule);
                timeMatchComplete = ConstraintCheckHelpers.TimeMatchComplete(ForTimeInterval.StartTime.Instant, ForTimeInterval.EndTime.Instant, (DateTime)cData.TimePeriodInSchedule.StartTime.Instant, (DateTime)cData.TimePeriodInSchedule.EndTime.Instant);

                if (peopleMatch && timeMatchComplete)
                {
                    isSatisfied = false;
                    MessageTemplate = "The resource is blocked for the choosen time period and selected people.";
                }
            }
            else if (ForPerson != null)
            {
                peopleMatch = ConstraintCheckHelpers.PeopleMatch(ForPerson, cData.PersonsInSchedule);
                if (peopleMatch)
                {
                    isSatisfied = false;
                    MessageTemplate = "The resource is blocked selected people.";
                }
            }
            else if (ForTimeInterval != null)
            {
                timeMatchComplete = ConstraintCheckHelpers.TimeMatchComplete(ForTimeInterval.StartTime.Instant, ForTimeInterval.EndTime.Instant, (DateTime)cData.TimePeriodInSchedule.StartTime.Instant, (DateTime)cData.TimePeriodInSchedule.EndTime.Instant);
                if (timeMatchComplete)
                {
                    isSatisfied = false;
                    MessageTemplate = "The resource is blocked for the choosen time period";
                }
            }

            if (ForPeriodicTimeInterval != null)
            {
                PeriodicTimeHelper pHelper = new PeriodicTimeHelper();
                List<DateTime> tempDates = pHelper.PeriodicTimeMatch(ForPeriodicTimeInterval, ForTimeInterval, (DateTime)cData.TimePeriodInSchedule.StartTime.Instant, (DateTime)cData.TimePeriodInSchedule.EndTime.Instant);
                if (tempDates.Count() > 0)
                {
                    isSatisfied = false;
                    string m = "";
                    foreach (DateTime d in tempDates)
                    {
                        m += d.ToString() + "\r\n";
                    }
                    MessageTemplate = String.Format("The resource is blocked on the followings day(s): {0}", m);
                }
            }


            return isSatisfied;
        }

        #endregion
    }


    public class QuantityConstraint : ResourceConstraint
    {
        #region Attributes

        /// <summary>
        /// For resources with Quantity you can set a Quantity Constraints
        /// </summary>
        public virtual int Quantity { get; set; }

        /// <summary>
        ///  Comparison operator for the quantity
        /// </summary>
        public virtual ComparisonOperator ComparisonOperator { get; set; }

        /// <summary>
        ///
        /// </summary>
        /// <remarks></remarks>
        /// <seealso cref=""/>        
        public override string ErrorMessage
        {
            get
            {
                if (Negated)
                {
                    return (string.Format(
                        (!string.IsNullOrWhiteSpace(NegatedMessageTemplate) ? NegatedMessageTemplate : defaultNegatedMessageTemplate)));
                }
                else
                {
                    return (string.Format(
                        (!string.IsNullOrWhiteSpace(MessageTemplate) ? MessageTemplate : defaultMessageTemplate)));
                }
            }
        }

        public override string FormalDescription
        {
            get
            {
                String comparer = "";
                if (Negated)
                {
                    switch (ComparisonOperator)
                    {
                        case ComparisonOperator.Equals:
                            comparer = "not equal to";
                            break;
                        case ComparisonOperator.NotEquals:
                            comparer = "equal to";
                            break;
                        case ComparisonOperator.GreaerThan:
                            comparer = "less than or equal to";
                            break;
                        case ComparisonOperator.GreaterThanOrEqual:
                            comparer = "less than";
                            break;
                        case ComparisonOperator.LessThan:
                            comparer = "greater than or equal to";
                            break;
                        case ComparisonOperator.LessThanOrEqual:
                            comparer = "greater than";
                            break;
                        default:
                            break;
                    }
                    return (string.Format("You must book this resource with a quantity {0} {1}", comparer, Quantity));
                }
                else
                {
                    switch (ComparisonOperator)
                    {
                        case ComparisonOperator.Equals:
                            comparer = "equal to";
                            break;
                        case ComparisonOperator.NotEquals:
                            comparer = "not equal to";
                            break;
                        case ComparisonOperator.GreaerThan:
                            comparer = "greater than";
                            break;
                        case ComparisonOperator.GreaterThanOrEqual:
                            comparer = "greater than or equal to";
                            break;
                        case ComparisonOperator.LessThan:
                            comparer = "less than";
                            break;
                        case ComparisonOperator.LessThanOrEqual:
                            comparer = "less than or equal to";
                            break;
                        default:
                            break;
                    }
                    return (string.Format("You must book this resource with a quantity {0} {1}", comparer, Quantity));
                }
            }
        }

        #endregion

        #region Associations

        #endregion

        #region Methods

        /// <summary>
        ///
        /// </summary>
        /// <remarks></remarks>
        /// <seealso cref=""/>
        /// <param name="data"></param>
        /// <param name="auxiliary"></param>
        public override bool IsSatisfied(object data, object auxiliary = null)
        {
            bool isSatisfied = true;
            bool peopleMatch = false;
            bool timeMatchComplete = false;
            ResourceConstraintData cData = (ResourceConstraintData)data;

            List<long> personInConstraint = new List<long>();

            //Case 1: Person AND Time Period restriction
            if ((ForPerson != null || AllUsers) && (ForTimeInterval != null || ForEver))
            {
                if(ForPerson != null)
                    peopleMatch = ConstraintCheckHelpers.PeopleMatch(ForPerson, cData.PersonsInSchedule);

                if(ForTimeInterval != null)
                    timeMatchComplete = ConstraintCheckHelpers.TimeMatchComplete(ForTimeInterval.StartTime.Instant, ForTimeInterval.EndTime.Instant, (DateTime)cData.TimePeriodInSchedule.StartTime.Instant, (DateTime)cData.TimePeriodInSchedule.EndTime.Instant);


                if ((peopleMatch && timeMatchComplete) || (peopleMatch && ForEver) || (timeMatchComplete && AllUsers) || (ForEver && AllUsers))
                {
                    switch (ComparisonOperator)
                    {
                        case ComparisonOperator.Equals:
                            {
                                if (Quantity != cData.QuantityInSchedule)
                                {
                                    isSatisfied = false;
                                    MessageTemplate = String.Format( "You must book a Quantity {0} {1}.", ComparisonOperator, Quantity); 
                                }
                                break;
                            }
                    }
                }
            }
            else if (ForPerson != null || AllUsers)
            {
                if(ForPerson != null)
                    peopleMatch = ConstraintCheckHelpers.PeopleMatch(ForPerson, cData.PersonsInSchedule);

                if (peopleMatch || AllUsers)
                {
                    switch (ComparisonOperator)
                    {
                        case ComparisonOperator.Equals:
                            {
                                if (Quantity != cData.QuantityInSchedule)
                                {
                                    isSatisfied = false;
                                    MessageTemplate = String.Format("You must book a Quantity {0} {1}.", ComparisonOperator, Quantity);
                                }
                                break;
                            }
                    }
                }
            }
            else if (ForTimeInterval != null || ForEver)
            {
                if(ForTimeInterval != null)
                    timeMatchComplete = ConstraintCheckHelpers.TimeMatchComplete(ForTimeInterval.StartTime.Instant, ForTimeInterval.EndTime.Instant, (DateTime)cData.TimePeriodInSchedule.StartTime.Instant, (DateTime)cData.TimePeriodInSchedule.EndTime.Instant);

                if (timeMatchComplete || ForEver)
                {
                    switch (ComparisonOperator)
                    {
                        case ComparisonOperator.Equals:
                            {
                                if (Quantity != cData.QuantityInSchedule)
                                {
                                    isSatisfied = false;
                                    MessageTemplate = String.Format("You must book a Quantity {0} {1}.", ComparisonOperator, Quantity);
                                }
                                break;
                            }
                    }
                }
            }
            //else
            //{
            //    switch (ComparisonOperator)
            //    {
            //        case ComparisonOperator.Equals:
            //            {
            //                if (Quantity != cData.Quantity)
            //                {
            //                    isSatisfied = false;
            //                    MessageTemplate = String.Format("You must book a Quantity {0} {1}.", ComparisonOperator, Quantity);
            //                }
            //                break;
            //            }
            //    }
            //}

            return isSatisfied;
        }

        #endregion
    }

    public class TimeCapacityConstraint : ResourceConstraint
    {
        #region Attributes

        /// <summary>
        /// Maximum time period you can book a resource for people, time frame ...
        /// </summary>
        public virtual TimeDuration Duration { get; set; }


        /// <summary>
        /// Maximum time period you can book a resource for people, time frame ...
        /// </summary>
        public virtual TimeInterval MaxTimeInterval { get; set; }

        /// <summary>
        ///
        /// </summary>
        /// <remarks></remarks>
        /// <seealso cref=""/>        
        public override string ErrorMessage
        {
            get
            {
                if (Negated)
                {
                    return "";
                }
                else
                {
                    return "";
                }
            }
        }

        public override string FormalDescription
        {
            get
            {
                if (Negated)
                {
                    //return (string.Format("You must book this with a quantity of {0}", Quantity));
                    return "";
                }
                else
                {
                    //return (string.Format("You must book this with a quantity of {0}", Quantity));
                    return "";
                }
            }
        }

        #endregion

        #region Associations

        #endregion

        #region Methods

        /// <summary>
        ///
        /// </summary>
        /// <remarks></remarks>
        /// <seealso cref=""/>
        /// <param name="data"></param>
        /// <param name="auxiliary"></param>
        public override bool IsSatisfied(object data, object auxiliary = null)
        {
            //need to be implemented
            return true;
        }

        #endregion
    }


    /// <summary>
    /// Helper Class Data from the Booking/Schedule
    /// </summary>
    public class ResourceConstraintData
    {
        /// <summary>
        ///  Person(s) for whom the booking is
        /// </summary>
        public List<long> PersonsInSchedule { get; set; }

        /// <summary>
        ///  TimePeriod booking
        /// </summary>
        public TimeInterval TimePeriodInSchedule { get; set; }

        /// <summary>
        ///  Quantity from the booking
        /// </summary>
        public int QuantityInSchedule { get; set; }

        public List<Schedule> SchedulesInEvent { get; set; }

        public bool Implicit { get; set; }

        public ResourceConstraintData()
        {
            PersonsInSchedule = new List<long>();
            SchedulesInEvent = new List<Schedule>();
        }
    }
}
