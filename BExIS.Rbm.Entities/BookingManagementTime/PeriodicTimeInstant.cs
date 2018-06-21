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
    public enum ResetFrequency
    {
        Daily,
        Weekly,
        Monthly,
        Yearly
    }

    public class PeriodicTimeInstant : BaseEntity
    {
        #region Attributes

        /// <summary>
        /// Frequency of Repetition. Can be Daily, Weekly etc.
        /// </summary>
        public virtual ResetFrequency ResetFrequency { get; set; }

        /// <summary>
        /// Interval of Repetition. Time unit of this value dependes on the selectet frequency.
        /// </summary>
        public virtual int ResetInterval { get; set; }

        /// <summary>
        /// Offset is starting point, e.g. day of week, hour
        /// </summary>
        public virtual int Off_Set { get; set; }

        public virtual SystemDefinedUnit Off_Set_Unit { get; set; }

        #endregion

        #region Associations

        #endregion

        #region Methods

        #endregion
    }
}
