using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Vaiona.Entities.Common;

namespace BExIS.Rbm.Entities.Booking
{
    public class Notification : BaseEntity
    {
        #region Attributes

        /// <summary>
        /// The subject if the notification.
        /// </summary> 
        public virtual string Subject { get; set; }


        /// <summary>
        /// Startdate (day and time) where the notification apply.
        /// </summary> 
        public virtual DateTime StartDate { get; set; }

        /// <summary>
        /// Enddate (day and time) where the notification apply.
        /// </summary> 
        public virtual DateTime EndDate { get; set; }

        public virtual DateTime InsertDate { get; set; }

        /// <summary>
        /// The Notification it self.
        /// </summary> 
        public virtual string Message { get; set; }

        //not implement jet
        //public virtual string Owner { get; set; }

        #endregion

        #region Associations

        //It seems we dont need to cave that
        /// <summary>
        /// The <see cref="Schedule"/>s where where the notification apply on.
        /// </summary> 
        //public virtual ICollection<Schedule> Schedules { get; set; }


        public virtual ICollection<NotificationDependency> NotificationDependency { get; set; }

        #endregion

        #region Methods

        #endregion

    }


    public class NotificationDependency
    {
        #region Attributes

        public virtual long Id { get; set; }

        public virtual string DomainItem { get; set; }

        public virtual long AttributeId { get; set; }

        #endregion

        #region Associations

        public virtual Notification Notification { get; set; }

        #endregion

        #region Methods

        #endregion




    }
}
