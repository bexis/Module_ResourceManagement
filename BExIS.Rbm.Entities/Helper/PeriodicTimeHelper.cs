using BExIS.Rbm.Entities.BookingManagementTime;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BExIS.Rbm.Entities.ResourceConstraint
{
    public class PeriodicTimeHelper
    {
        public List<DateTime> PeriodicTimeMatch(PeriodicTimeInterval periodicTimeInterval, TimeInterval timeInterval, DateTime bookingStart, DateTime bookingEnd)
        {
            switch (periodicTimeInterval.PeriodicTimeInstant.ResetFrequency)
            {
                case ResetFrequency.Daily:
                   
                    return GetAffectedDaysInTimePeriodDaily(timeInterval.StartTime.Instant, timeInterval.EndTime.Instant, bookingStart, bookingEnd, periodicTimeInterval.PeriodicTimeInstant.ResetInterval);

                case ResetFrequency.Weekly:

                    List<DayOfWeek> daysInWeek = GetDays(periodicTimeInterval.PeriodicTimeInstant.Off_Set, periodicTimeInterval.Duration.Value);
                    return GetAffectedDaysInTimePeriodWeekly(timeInterval.StartTime.Instant, timeInterval.EndTime.Instant, bookingStart, bookingEnd, periodicTimeInterval.PeriodicTimeInstant.ResetInterval, daysInWeek);

                case ResetFrequency.Monthly:

                    return GetAffectedDaysInTimePeriodMonthly(timeInterval.StartTime.Instant, timeInterval.EndTime.Instant, bookingStart, bookingEnd, periodicTimeInterval.PeriodicTimeInstant.ResetInterval, periodicTimeInterval.Duration.Value, periodicTimeInterval.PeriodicTimeInstant.Off_Set);

                default:
                    return new List<DateTime>();
            }
        }

        private List<DateTime> GetAffectedDaysInTimePeriodDaily(DateTime? startDate, DateTime? endDate, DateTime bookingStart, DateTime bookingEnd, int interval)
        {
            List<DateTime> affectedDays = new List<DateTime>();

            DateTime tempStart = new DateTime();
            DateTime endCondition = new DateTime();


            if (startDate.HasValue)
            {
                tempStart = (DateTime)startDate;
            }

            if (!endDate.HasValue)
                endCondition = bookingEnd;
            else
            {
                if (endDate < bookingEnd)
                    endCondition = (DateTime)endDate;
                else
                    endCondition = bookingEnd;
            }


            while (tempStart <= endCondition)
            {
                if (tempStart >= bookingStart && tempStart <= bookingEnd)
                {
                    affectedDays.Add(tempStart);
                }

                tempStart = tempStart.AddDays(interval);

                //tempStart.AddDays(interval);
                //if (tempStart >= bookingStart)
                //        affectedDays.Add(tempStart); 
            }
            

            return affectedDays;
        }

        private List<DateTime> GetAffectedDaysInTimePeriodWeekly(DateTime? startDate, DateTime? endDate, DateTime bookingStart, DateTime bookingEnd, int interval, List<DayOfWeek> daysInWeek)
        {
            List<DateTime> affectedDays = new List<DateTime>();

            DateTime tempStart = new DateTime();
            DateTime endCondition = new DateTime();

            if (!endDate.HasValue)
                endCondition = bookingEnd;
            else
            {
                if (endDate < bookingEnd)
                    endCondition = (DateTime)endDate;
                else
                    endCondition = bookingEnd;
            }

            int intervallInDays = interval * 7;
                
            //all Dates which are affected until endcondition
            List<DateTime> affectedDates = new List<DateTime>();

            foreach (DayOfWeek weekDay in daysInWeek)
            {
                //start with startday for every affected day in week
                if (startDate.HasValue)
                {
                    tempStart = (DateTime)startDate;
                }

                while (tempStart <= endCondition)
                {

                    if (weekDay == tempStart.DayOfWeek)
                    {
                        affectedDates.Add(tempStart);
                        tempStart = tempStart.AddDays(intervallInDays);
                    }
                    else
                    {
                        tempStart = tempStart.AddDays(1);
                    }

                }
            }

            //get affected day to return
            foreach(DateTime d in affectedDates)
            {
                if(d >= bookingStart && d<= bookingEnd)
                {
                    affectedDays.Add(d);
                }
            }

            return affectedDays;
        }

        private List<DateTime> GetAffectedDaysInTimePeriodMonthly(DateTime? startDate, DateTime? endDate, DateTime bookingStart, DateTime bookingEnd, int interval, int duration, int offset)
        {
            List<DateTime> affectedDays = new List<DateTime>();
            DateTime tempStart = new DateTime();
            DateTime endCondition = new DateTime();


            if (startDate.HasValue)
            {
                tempStart = (DateTime)startDate;
            }

            //get end condition, is the endend of constraint or enddate of booking
            if (!endDate.HasValue)
                endCondition = bookingEnd;
            else
            {
                if (endDate < bookingEnd)
                    endCondition = (DateTime)endDate;
                else
                    endCondition = bookingEnd;
            }
                

                while (tempStart <= endCondition)
                {
                    //check day of month
                    if(tempStart.Day == offset)
                    {
                        //if day of month in the booking time perid than add the day
                        if(tempStart >= bookingStart && tempStart <= bookingEnd)
                        {
                            affectedDays.Add(tempStart);
                        }

                        tempStart = tempStart.AddMonths(interval);
                    }
                }

                return affectedDays;
        }


        private List<DayOfWeek> GetDays(int offset, int duration)
        {
            List<DayOfWeek> affectedDays = new List<DayOfWeek>();
            int max = offset + duration;
            for(int i = offset; i<max; i++)
            {
                int dayInt = i;

                //Sunday is 0 in DayOfWeek enum
                if (dayInt == 7)
                    dayInt = 0;

                affectedDays.Add((DayOfWeek)dayInt);
            }

            return affectedDays;
        }

    }
}
