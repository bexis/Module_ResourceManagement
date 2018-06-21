using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vaiona.Entities.Common;
using RC = BExIS.Rbm.Entities.ResourceConstraint;

namespace BExIS.Rbm.Entities.BookingManagementTime
{
    public class PeriodicTimeInterval : BaseEntity
    {
        #region Attributes

        /// <summary>
        /// PeriodicTimeInstant of the Interval
        /// /// </summary>
        public virtual PeriodicTimeInstant PeriodicTimeInstant { get; set; }

        public virtual TimeDuration Duration { get; set; }

        public virtual RC.ResourceConstraint ResourceConstraint { get; set; }

        #endregion

        #region Associations

        #endregion

        #region Methods

        #endregion
    }
}
