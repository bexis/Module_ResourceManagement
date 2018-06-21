using BExIS.Security.Entities.Subjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Vaiona.Entities.Common;
using RC = BExIS.Rbm.Entities.ResourceConstraint;

namespace BExIS.Rbm.Entities.Users
{
    public class Person : BaseEntity
    {
         #region Attributes

        public virtual User Contact{ get; set; }

        /// <summary>
        /// This is a workaround according to NHibernate's Lazy loading proxy creation!
        /// It should not be mapped!
        /// </summary>        
        /// <remarks></remarks>
        /// <seealso cref=""/>        
        public virtual Person Self { get { return this; } }

        #endregion


        #region Associations

        //public virtual RC.ResourceConstraint ResourceConstraint { get; set; }


        #endregion


        #region Methods


        #endregion
    }
}
