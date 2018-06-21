using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vaiona.Entities.Common;
using R = BExIS.Rbm.Entities.Resource;

namespace BExIS.Rbm.Entities.ResourceStructure
{
    public class ResourceStructure : BusinessEntity
    {
        #region Associations

        public virtual ICollection<ResourceStructure> Children { get; set; }

        /// <summary>
        /// ResourceStructure is based on another ResourceStructure.
        /// </summary>
        public virtual ResourceStructure Parent { get; set; }

        /// <summary>
        /// ResourceStructure have a list of <see cref="ResourceAttributeUsage"/>s. This is the connection to <see cref="ResourceStructureAttribute"/>.
        /// </summary>
        public virtual ICollection<ResourceAttributeUsage> ResourceAttributeUsages { get; set; }

        /// <summary>
        /// List of <see cref="Resource"/>s where the ResourceStructure is used.
        /// </summary>
        public virtual ICollection<R.Resource> Resources { get; set; }

        #endregion

        #region Attributes

        /// <summary>
        /// Name of the resource structure.
        /// </summary>
        public virtual string Name { get; set; }

        /// <summary>
        /// A free form description of the resource structure.
        /// </summary>
        public virtual string Description { get; set; }


        #endregion

        #region Methods

        public ResourceStructure()
        {
            Name = "";
            Description = "";
            ResourceAttributeUsages = new List<ResourceAttributeUsage>();
            Children = new List<ResourceStructure>();
            //Parent = new ResourceStructure();
            Resources = new List<R.Resource>();
        }

        #endregion


    }
}
