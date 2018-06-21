using BExIS.Rbm.Entities.Users;
using BExIS.Security.Entities.Subjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace BExIS.Web.Shell.Areas.RBM.Models.Booking
{
    public class PersonUserModel
    {
        public long UserId { get; set; }
        public string UserFullName { get; set; }
    }

    public class PersonInConstraint : PersonUserModel
    {
        public long Id { get; set; }
        public int Index { get; set; }
        public bool IsSelected { get; set; }


        public PersonInConstraint()
        {

        }

        public PersonInConstraint(User user, long id, int index)
        {
            Id = id;
            Index = index;
            UserId = user.Id;
            UserFullName = user.FullName;
        }
    }

    //for user in Schedules
    public class PersonInSchedule: PersonUserModel
    {
        public long Id { get; set; }
        public int Index { get; set; }
        public bool IsContactPerson { get; set; }
        public bool IsSelected { get; set; }
        public bool EditMode { get; set; }

        //define if user has edit access to the schedule
        public bool EditAccess { get; set; }

        public PersonInSchedule()
        {

        }

        public PersonInSchedule(long id, User user, bool isContactPerson)
        {
            Id = id;
            UserId = user.Id;
            UserFullName = user.FullName;
            IsContactPerson = isContactPerson;
            IsSelected = false;
        }
    }
}