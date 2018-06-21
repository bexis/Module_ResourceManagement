using BExIS.Security.Entities.Subjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BExIS.Rbm.Entities.Users
{
    public class PersonGroup : Person
    {
        #region Attributes

        //PersonGroup can be a group of persons, a Project, organisation
        //public virtual string Type { get; set; }

        #endregion


        #region Associations

        public virtual ICollection<User> Users { get; set; }

        #endregion


        #region Methods

        public PersonGroup()
        {
            Users = new List<User>();
        }


        #endregion
    }
}
