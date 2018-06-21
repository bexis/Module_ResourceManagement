using BExIS.Rbm.Entities.BookingManagementTime;
using BExIS.Web.Shell.Areas.RBM.Models.BookingManagementTime;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace BExIS.Web.Shell.Areas.RBM.Helpers
{
    public class TimeHelper
    {
        public static DateTime GetEndDateOfDuration(int duration, SystemDefinedUnit unit, DateTime startdate)
        {
            DateTime endDate = new DateTime();

            switch(unit)
            {
                case SystemDefinedUnit.second:
                    endDate = startdate.AddSeconds(duration);
                    break;
                case SystemDefinedUnit.minute:
                    endDate = startdate.AddMinutes(duration);
                    break;
                case SystemDefinedUnit.hour:
                    endDate = startdate.AddHours(duration);
                    break;
                case SystemDefinedUnit.day:
                    endDate = startdate.AddDays(duration);
                    break;
                case SystemDefinedUnit.week:
                    endDate = startdate.AddDays(7 * duration);
                    break;
                case SystemDefinedUnit.month:
                    endDate = startdate.AddMonths(duration);
                    break;
                case SystemDefinedUnit.year:
                    endDate = startdate.AddYears(duration);
                    break;
            }
            return endDate;
        }

        public static int GetDifferenceInDays(DateTime startDate, DateTime endDate)
        {
            TimeSpan ts = endDate - startDate;
            return ts.Days;
        }

        public static int GetDifferenceInMonths(DateTime startDate, DateTime endDate)
        {
            return (endDate.Month + endDate.Year * 12) - (startDate.Month + startDate.Year * 12);
        }

        public static int GetDifferenceInSeconds(DateTime startDate, DateTime endDate)
        {
            TimeSpan ts = endDate - startDate;
            return ts.Seconds;
        }

        public static int GetDifferenceInMinutes(DateTime startDate, DateTime endDate)
        {
            TimeSpan ts = endDate - startDate;
            return ts.Minutes;
        }

        public static int GetDifferenceInHours(DateTime startDate, DateTime endDate)
        {
            TimeSpan ts = endDate - startDate;
            return ts.Hours;
        }

        public static List<CheckModel> GetDays()
        {
            List<CheckModel> days = new List<CheckModel>
            {
                 new CheckModel{Id = 1, Name = "Monday", DisplayName="M", Checked = false},
                 new CheckModel{Id = 2, Name = "Tuesday", DisplayName="T", Checked = false},
                 new CheckModel{Id = 3, Name = "Wednesday", DisplayName="W", Checked = false},
                 new CheckModel{Id = 4, Name = "Thursday", DisplayName="T", Checked = false},
                 new CheckModel{Id = 5, Name = "Friday", DisplayName="F", Checked = false},
                 new CheckModel{Id = 6, Name = "Saturday", DisplayName="S", Checked = false},
                 new CheckModel{Id = 7, Name = "Sunday", DisplayName="S", Checked = false},
            };

            return days;
        }

        //public static int CalculateDuration(DateTime startTime, DateTime endTime)
        //{
        //    TimeSpan duration = DateTime.Parse(endTime).Subtract(DateTime.Parse(startTime));

        //}
    }
}