using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BExIS.Rbm.Entities.Booking
{
    public class BookingEvent : RealEvent
    {

        #region Attributes



        #endregion

        #region Associations

        /// <summary>
        /// The <see cref="Activity"/> wich are related to the event.
        /// </summary>
        public virtual ICollection<Activity> Activities { get; set; }

        #endregion

        #region Methods

        public BookingEvent()
        {
            Activities = new List<Activity>();
        }

        #endregion

    }
}
