using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vaiona.Entities.Common;

namespace BExIS.Rbm.Entities.BookingManagementTime
{
    public class TimeInterval : BaseEntity
    {
        public virtual TimeInstant StartTime { get; set; }

        public virtual TimeInstant EndTime { get; set; }

        /// <summary>
        /// This is a workaround according to NHibernate's Lazy loading proxy creation!
        /// It should not be mapped!
        /// </summary>        
        /// <remarks></remarks>
        /// <seealso cref=""/>        
        public virtual TimeInterval Self { get { return this; } }

        #region Associations


        #endregion

        #region Methods

        public TimeInterval()
        {
            StartTime = new TimeInstant();
            EndTime = new TimeInstant();
        }

        public TimeInterval(DateTime startTime, DateTime endTime)
        {
            StartTime = new TimeInstant(startTime);
            EndTime = new TimeInstant(endTime);
        }

        #endregion

    }
}
