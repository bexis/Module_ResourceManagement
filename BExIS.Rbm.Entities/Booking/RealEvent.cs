using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Vaiona.Entities.Common;
using R = BExIS.Rbm.Entities.Resource;

namespace BExIS.Rbm.Entities.Booking
{
    public class RealEvent : BaseEntity
    {
        #region Attributes

        /// <summary>
        /// Name of the event
        /// </summary> 
        public virtual string Name { get; set; }

        /// <summary>
        /// A optional description to the Event
        /// </summary> 
        public virtual string Description { get; set; }

        //min date from schedules
        public virtual DateTime MinDate { get; set; }

        //max date from schedules
        public virtual DateTime MaxDate { get; set; }


        #endregion

        #region Associations

        /// <summary>
        /// List of <see cref="Schedule"/>s, the booked resources with resource and time frame, are related to this event.
        /// </summary>
        public virtual ICollection<Schedule> Schedules { get; set; }

        #endregion

        #region Methods
      
        public RealEvent()
        {
            Schedules = new List<Schedule>(); 
        }

        #endregion
    }
}
