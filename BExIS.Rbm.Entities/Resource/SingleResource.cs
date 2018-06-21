using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BExIS.Rbm.Entities.Resource
{
    public class SingleResource : Resource
    {
         #region Associations

        /// <summary>
        /// The <see cref="ResourceStructure"/> which is used for the single resource.
        /// </summary>
        public virtual ResourceStructure.ResourceStructure ResourceStructure { get; set; }

        /// <summary>
        /// List of <see cref="ResourceGroup"/>s in which the resource is included.
        /// </summary>
        public virtual ICollection<ResourceGroup> ResourceGroups { get; set; }

        //optional notification ref

        #endregion

        #region Attributes

        /// <summary>
        /// Quantity of the single resource
        /// </summary>
        public virtual int Quantity { get; set; }

        ///// <summary>
        ///// Maximal number of possible shares of the resource.
        ///// </summary>
        //public virtual int MaxCountOfNoOfShares { get; set; }

        /// <summary>
        /// On the calendar displayed color of the resource.
        /// </summary>.
        public virtual string Color { get; set; }


        // public virtual string Owner { get; set; }

        #endregion

        #region Methods

        public SingleResource(ResourceStructure.ResourceStructure resourceStructure)
        {
            ResourceStatus = Status.created;
            StatusChangeDate = new DateTime();
            ResourceStructure = resourceStructure;
            //resourceStructure.Resources.Add(this);
        }

        public SingleResource()
        {
            ResourceStatus = Status.created;
            StatusChangeDate = new DateTime();
            ResourceStructure = new ResourceStructure.ResourceStructure();
            Color = "";
        }

        #endregion
    }
}
