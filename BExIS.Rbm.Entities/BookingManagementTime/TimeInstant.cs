using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vaiona.Entities.Common;

namespace BExIS.Rbm.Entities.BookingManagementTime
{
    /// <summary>
    /// 
    /// </summary>
    /// <remarks></remarks>        
    public enum SystemDefinedUnit
    {
        day,
        week,
        month,
        year,
        hour,
        minute,
        second
    }

    public class TimeInstant : BaseEntity
    {
        //default precision -> year
        public virtual SystemDefinedUnit Precision { get; set; }

        public virtual DateTime? Instant { get; set; }

        /// <summary>
        /// This is a workaround according to NHibernate's Lazy loading proxy creation!
        /// It should not be mapped!
        /// </summary>        
        /// <remarks></remarks>
        /// <seealso cref=""/>        
        public virtual TimeInstant Self { get { return this; } }


        #region Associations

        public virtual TimeInterval TimeInterval { get; set; }

        #endregion

        #region Methods

        public TimeInstant()
        {
            Precision = SystemDefinedUnit.day;
            //Instant = new DateTime();
        }

        public TimeInstant(DateTime instant)
        {
            Instant = instant;
        }

            #endregion
        }
}
