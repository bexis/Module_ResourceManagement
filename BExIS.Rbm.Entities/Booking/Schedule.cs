using BExIS.Rbm.Entities.Users;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Vaiona.Entities.Common;
using R = BExIS.Rbm.Entities.Resource;

namespace BExIS.Rbm.Entities.Booking
{
    public class Schedule : BaseEntity
    {
        #region Attributes

        /// <summary>
        /// Startdate (day and time) of the resource schedule.
        /// </summary> 
        public virtual DateTime StartDate { get; set; }

        /// <summary>
        /// Enddate (day and time) of the resource schedule.
        /// </summary> 
        public virtual DateTime EndDate { get; set; }

        /// <summary>
        /// Quantity of booked resource instances.
        /// </summary> 
        public virtual int Quantity { get; set; }

        /// <summary>
        /// Index for the sequence of the schedules in one event.
        /// </summary> 
        public virtual int Index { get; set; }

        #endregion

        #region Associations

        /// <summary>
        /// The <see cref="SingleResource"/> which is scheduled.
        /// </summary> 
        public virtual R.SingleResource Resource { get; set; }

        /// <summary>
        /// The <see cref="BookingEvent"/> where is this schedule related.
        /// </summary> 
        public virtual BookingEvent BookingEvent { get; set; }

        /// <summary>
        /// The <see cref="Person"/> which created the schedule.
        /// </summary> 
        public virtual IndividualPerson ByPerson { get; set; }

        /// <summary>
        /// The <see cref="Person"/> for who the schedule is created.
        /// </summary> 
        public virtual Person ForPerson { get; set; }

        /// <summary>
        /// The <see cref="Activity"/> wich are related to the schedule.
        /// </summary>
        public virtual ICollection<Activity> Activities { get; set; }

        #endregion

        #region Methods

        //public Schedule()
        //{
        //    StartDate = new DateTime();
        //    EndDate = new DateTime();
        //    Resource = new R.SingleResource();
        //    Event = new Event();
        //    ForPerson = new Person();
        //}

        #endregion
    }
}
