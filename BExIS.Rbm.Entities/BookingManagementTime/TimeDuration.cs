using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vaiona.Entities.Common;
using R = BExIS.Rbm.Entities.Resource;

namespace BExIS.Rbm.Entities.BookingManagementTime
{
    public class TimeDuration : BaseEntity
    {
        //year, month, hour ...
        public virtual SystemDefinedUnit TimeUnit { get; set; }

        //value of duration (e.g 1 month)
        public virtual int Value { get; set; }

        /// <summary>
        /// This is a workaround according to NHibernate's Lazy loading proxy creation!
        /// It should not be mapped!
        /// </summary>        
        /// <remarks></remarks>
        /// <seealso cref=""/>        
        public virtual TimeDuration Self { get { return this; } }

        public virtual R.Resource Resource { get; set; }
    }
}
