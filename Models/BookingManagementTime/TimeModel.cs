using BExIS.Rbm.Entities.BookingManagementTime;
using BExIS.Web.Shell.Areas.RBM.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace BExIS.Web.Shell.Areas.RBM.Models.BookingManagementTime
{
    public class TimeModel
    {

    }

    public class TimeIntervalModel
    {
        public DateTime StartTime { get; set; }
        public DateTime? EndTime { get; set; }

        public TimeIntervalModel()
        {
        }

        public TimeIntervalModel(TimeInterval timeInterval)
        {
            StartTime = (DateTime)timeInterval.StartTime.Instant;
            EndTime = timeInterval.EndTime.Instant;
        }

    }

    public class PeriodicTimeIntervalModel
    {

        public long Id { get; set; }

        public PeriodicTimeInstant PeriodicTimeInstant { get; set; }

        public TimeDuration Duration { get; set; }

        public bool IsSet { get; set; }

        public List<ResetFrequency> ResetFrequencies { get; set; }

        public ResetFrequency ResetFrequency { get; set; }

        public int Index { get; set; }

        public bool ForEverIsSet { get; set; }

        public string ConstraintType { get; set; }

        //Needed for monthly
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }

        //Needed for daily
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }

        //needed for weekly, Ids for days
        public List<int> SelectedDays { get; set; }

        //all days contrainer 
        public List<CheckModel> Days { get; set; }

        //display summary
        public string Summary { get; set; }

        public PeriodicTimeIntervalModel()
        {
            PeriodicTimeInstant = new PeriodicTimeInstant();
            Duration = new TimeDuration();
            StartDate = null;
            EndDate = null;

            Days = new List<CheckModel>();
            Days = TimeHelper.GetDays();

            SelectedDays = new List<int>();

            ResetFrequencies = new List<ResetFrequency>();
            ResetFrequencies.Add(ResetFrequency.Daily);
            ResetFrequencies.Add(ResetFrequency.Weekly);
            ResetFrequencies.Add(ResetFrequency.Monthly);
        }

        public PeriodicTimeIntervalModel(PeriodicTimeInterval periodicTimeInterval, DateTime statDateConstraint, DateTime? endDateConstraint)
        {
            Id = periodicTimeInterval.Id;
            PeriodicTimeInstant = periodicTimeInterval.PeriodicTimeInstant;
            Duration = periodicTimeInterval.Duration;

            Days = new List<CheckModel>();
            Days = TimeHelper.GetDays();
            SelectedDays = new List<int>();

            switch (PeriodicTimeInstant.ResetFrequency)
            {
                case ResetFrequency.Daily: 
                    StartTime = new DateTime();
                    EndTime = new DateTime();
                    TimeSpan timeSpan = new TimeSpan(PeriodicTimeInstant.Off_Set, 00, 00);
                    StartTime.Add(timeSpan);
                    EndTime = StartTime.AddHours(periodicTimeInterval.Duration.Value);

                    if (PeriodicTimeInstant.ResetInterval <= 1)
                        Summary = "Daily";
                    else
                        Summary = String.Format("Every {0} days, start at {1} and end at {2}", PeriodicTimeInstant.ResetInterval, StartTime, EndTime);
                    break;
                case ResetFrequency.Weekly:

                    List<int> ids = new List<int>();

                    //ids.Add(periodicTimeInterval.PeriodicTimeInstant.Off_Set);

                    for(int i = 0; i< periodicTimeInterval.Duration.Value; i++ )
                    {
                        ids.Add(periodicTimeInterval.PeriodicTimeInstant.Off_Set +i);
                    }
                    string days = "";
                    int count = 0;
                    foreach(int id in ids)
                    {
                        count++;
                        var temp = Days.Where(a => a.Id == id).FirstOrDefault();
                        if (count == ids.Count())
                            days += temp.Name;
                        else
                            days += temp.Name + ", ";

                        Days.Where(w => w.Id == id).ToList().ForEach(s => s.Checked = true);
                    }
                    if (PeriodicTimeInstant.ResetInterval <= 1)
                        Summary = String.Format("Weekly on {0}", days);
                    else
                        Summary = String.Format("Every {0} weeks on {1}", PeriodicTimeInstant.ResetInterval, days);

                    break;
                case ResetFrequency.Monthly:
                    StartDate = new DateTime();
                    StartDate = statDateConstraint;
                    EndDate = new DateTime();
                    EndDate = endDateConstraint;
                    if (PeriodicTimeInstant.ResetInterval <= 1)
                        Summary = String.Format("Monthly on day {0}", PeriodicTimeInstant.Off_Set);
                    else
                        Summary = String.Format("Every {0} months on the {1}th for the duration of {2}", PeriodicTimeInstant.ResetInterval, PeriodicTimeInstant.Off_Set, Duration.Value);
                    break;
            }

            ResetFrequencies = new List<ResetFrequency>();
            ResetFrequencies.Add(ResetFrequency.Daily);
            ResetFrequencies.Add(ResetFrequency.Weekly);
            ResetFrequencies.Add(ResetFrequency.Monthly);
        }
    }

    public class CheckModel
    {
        public int Id { get; set;}
        public string Name { get; set; }
        public string DisplayName { get; set; }
        public bool Checked { get; set; }
    }

}