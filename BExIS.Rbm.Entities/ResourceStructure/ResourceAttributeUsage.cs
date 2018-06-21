using BExIS.Dlm.Entities.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BExIS.Rbm.Entities.ResourceStructure
{
    public class ResourceAttributeUsage : BaseUsage
    {

        //public virtual bool IsValueOptional
        //{
        //    get { return MinCardinality < 1; } //if MinCardinality cardinality is zero (less than 1), the parameter value is optional
        //    set { MinCardinality = value ? 0 : 1; } // if value is optional, set the min cardinality to zero
        //}

        public virtual bool IsFileDataType { get; set; }

        #region Associations

        /// <summary>
        /// Usage of <see cref="ResourceStructureAttribute"/> in defined  <see cref="ResourceStructure"/>.
        /// </summary>
        public virtual ResourceStructureAttribute ResourceStructureAttribute { get; set; }

        /// <summary>
        ///<see cref="ResourceStructure"/> which include a uage of a  <see cref="ResourceStructureAttribute"/>.
        /// </summary>
        public virtual ResourceStructure ResourceStructure { get; set; }

        #endregion

    }
}
