using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vaiona.Entities.Common;

namespace BExIS.Rbm.Entities.Resource
{
    /// <summary>
    /// Each or all together mode. In each mode, the classifier is treated as a single resource. in the "all" mode , n * classifier = n quantity of each of the  resources in the classifier.
    /// </summary>
    /// <remarks></remarks> 
    public enum Mode
    {
        each = 0,
        all = 1
    }

    public class ResourceGroup : Resource
    {
        
        #region Associations

        /// <summary>
        /// List of <see cref="SingleResource"/>s which are group togehter
        /// </summary>
        public virtual ICollection<SingleResource> SingleResources { get; set; }

        #endregion

        #region Attributes

        /// <summary>
        /// Mode of this grouping.
        /// </summary>
        public virtual Mode GroupMode { get; set; }

        #endregion

        #region Methods

        #endregion
    }
}
