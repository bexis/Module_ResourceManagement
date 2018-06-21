using BExIS.Rbm.Entities.Users;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BExIS.Rbm.Entities.ResourceConstraint
{
    public class ConstraintCheckHelpers
    {
        public static bool PeopleMatch(Person forPerson, List<long> personsInSchedule)
        {
            List<long> personInConstraint = new List<long>();
            if (forPerson.Self is PersonGroup)
            {
                PersonGroup pGroup = (PersonGroup)forPerson.Self;
                personInConstraint = pGroup.Users.Select(a => a.Id).ToList();
            }
            else if (forPerson.Self is IndividualPerson)
            {
                IndividualPerson iPerson = (IndividualPerson)forPerson.Self;
                personInConstraint.Add(iPerson.Person.Id);

            }
            var diff = personInConstraint.Intersect(personsInSchedule);
            if (diff.Count() > 0)
                return true;
            else
                return false;
        }

        //check if time period within another timerperiod, Dabei ist startTime1 und EndTime1 der Zeitraum in den startTime2 und EndTime2 fallen sollen.
        public static bool TimeMatchComplete(DateTime? startTime1, DateTime? endTime1, DateTime startTime2, DateTime endTime2)
        {
            return startTime2 >= startTime1 && startTime2 <= endTime2 && endTime1 >= startTime1 && endTime2 <= endTime1;
        }

        //check if time period partially within another timerperiod -> not applies for the first implemation
        public bool TimeMatchPartially(DateTime? startTime1, DateTime? endTime1, DateTime startTime2, DateTime endTime2)
        {
            return startTime2 >= startTime1 && startTime2 <= endTime2 || endTime1 >= startTime1 && endTime2 <= endTime1;
        }
    }
}
