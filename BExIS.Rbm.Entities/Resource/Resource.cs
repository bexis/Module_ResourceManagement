using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vaiona.Entities.Common;
using BExIS.Rbm.Entities.Resource;
using E = BExIS.Rbm.Entities.Booking;
using RC = BExIS.Rbm.Entities.ResourceConstraint;
using BExIS.Rbm.Entities.ResourceStructure;
using BExIS.Rbm.Entities.BookingManagementTime;

namespace BExIS.Rbm.Entities.Resource
{

    /// <summary>
    /// Current status of the resource, set automatically by the system.
    /// </summary>
    /// <remarks></remarks>        
    public enum Status
    {
        created = 0, 
        available = 1, 
        checkout = 2, 
        deleted = 3, 
        lost = 4, 
        modifyBooking = 5, 
        cancelBooking = 6
    }

   
    public abstract class Resource : BusinessEntity
    {
      
        #region Associations

        /// <summary>
        /// A List of <see cref="ResourceConstraint"/>s act on the resource.
        /// </summary>
        public virtual ICollection<RC.ResourceConstraint> ResourceConstraints { get; set; }

        public virtual ICollection<ResourceAttributeValue> ResourceAttributeValues { get; set; }

        //public virtual ICollection<ResourceGroup> ResourceGroup { get; set; }

        #endregion

        #region Attributes

        /// <summary>
        /// Name of the resource.
        /// </summary>
        public virtual string Name { get; set; }


        /// <summary>
        /// A free form description of the resource.
        /// </summary>
        public virtual string Description { get; set; }

        /// <summary>
        /// Is a Activity needed to book that resource. 
        /// </summary>
        public virtual bool WithActivity { get; set; }

        /// <summary>
        /// BookingTimeGranularity -> The duration when a resource can be booked. e.g. 1 week, 1 hour ...
        /// </summary>
        public virtual TimeDuration Duration { get; set; }

        /// <summary>
        /// BookingTimeGranularity-> Starttime for the BookingTimeGranularity e.g.
        /// </summary>
        //public virtual int StartTime { get; set; }

        /// <summary>
        /// The current status of the resource.
        /// </summary>
        public virtual Status ResourceStatus { get; set; }

        /// <summary>
        /// The date when current status of the resource was changed.
        /// </summary>
        public virtual DateTime StatusChangeDate { get; set; }

        #endregion

        #region Methods


        #endregion
    }

    //public class BookingTimeGranularity : BaseEntity
    //{
    //    #region Associations

    //    /// <summary>
    //    /// The <see cref="Resource"/> for that the Booking Time Granularity was defined.
    //    /// </summary>
    //    public virtual Resource Resource { get; set; }

    //    #endregion


    //    #region Attributes

    //    //Dauer
    //    /// <summary>
    //    /// The duration when a resource can be booked. e.g. 1 week, 1 hour ...
    //    /// </summary>
    //    public virtual TimeDuration Duration { get; set; }

    //    /// <summary>
    //    /// Starttime for the BookingTimeGranularity e.g.
    //    /// </summary>
    //    public virtual int StartTime { get; set; }


    //    #endregion
    //}
    
}
