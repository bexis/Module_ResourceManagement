using BExIS.Security.Entities.Subjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BExIS.Rbm.Entities.Users
{
    public class IndividualPerson : Person
    {
        #region Attributes

        public virtual User Person { get; set; }

        #endregion


        #region Associations



        #endregion


        #region Methods

        public IndividualPerson()
        {
            Person = new User();
        }

        public IndividualPerson(User user)
        {
            Person = user;
            Contact = user;
        }

        #endregion
    }
}
