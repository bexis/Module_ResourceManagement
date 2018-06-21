using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vaiona.Entities.Common;

namespace BExIS.Rbm.Entities.ResourceStructure
{
    public class ResourceAttributeValue : BusinessEntity
    {
        #region Associations

        /// <summary>
        /// The Value is related to a <see cref="Resource"/>. 
        /// </summary>
        public virtual Resource.Resource Resource { get; set; }

        /// <summary>
        /// The <see cref="ResourceAttributeUsage"/> for that the value is defined.
        /// </summary>
        public virtual ResourceAttributeUsage ResourceAttributeUsage { get; set; }

        #endregion

        #region Attributes

        #endregion
    }

    public class TextValue : ResourceAttributeValue
    {
        #region Attributes

        /// <summary>
        /// The value.
        /// </summary>
        public virtual string Value { get; set; }

        #endregion
    }

    public class FileValue : ResourceAttributeValue
    {
        #region Attributes

        public virtual string Name { get; set; }

        public virtual string Extention { get; set; }

        public virtual string Minmetype { get; set; }

        public virtual byte[] Data { get; set; }

        public virtual bool NeedConfirmation { get; set; }


        #endregion
    }
}
