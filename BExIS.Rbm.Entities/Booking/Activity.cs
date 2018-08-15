using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Vaiona.Entities.Common;

namespace BExIS.Rbm.Entities.Booking
{
    public class Activity : BaseEntity
    {
        #region Attributes

        /// <summary>
        /// Name if the activity
        /// </summary> 
        public virtual string Name { get; set; }

        /// <summary>
        /// A free form description of the activity.
        /// </summary>
        public virtual string Description { get; set; }

        /// <summary>
        /// If activity is disabled you can not use it in the booking area
        /// </summary>
        public virtual bool Disable { get; set; }



        #endregion

        #region Associations

        /// <summary>
        /// The activities are used on <see cref="RealEvent"/>s
        /// </summary>
        public virtual ICollection<RealEvent> Events { get; set; }

        /// <summary>
        /// The activities are used on <see cref="Schedule"/>s
        /// </summary>
        public virtual ICollection<Schedule> Schedules { get; set; }


        #endregion

        #region Methods

        public Activity()
        {
            Name = "";
            Disable = false;
            Events = new List<RealEvent>();
            Schedules = new List<Schedule>();
        }

        #endregion

    }
}
