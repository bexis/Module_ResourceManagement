using BExIS.Rbm.Entities.Booking;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace BExIS.Web.Shell.Areas.RBM.Models.Booking
{
    public class ActivityModel
    {
        public long Id { get; set; }

        [Display(Name = "Name")]
        [StringLength(50, ErrorMessage = "The activity name must be {2} - {1} characters long.", MinimumLength = 3)]
        [Required]
        public string Name { get; set; }

        public string Description { get; set; }

        public bool Disable { get; set; }

        public bool InUse { get; set; }

        //define if user has edit access to the attribute
        public bool EditAccess { get; set; }

        //define if user has delete access to the attribute
        public bool DeleteAccess { get; set; }

        List<BookingEventModel> Events { get; set; }

        public ActivityModel()
        {
            Name = "";
            Disable = false;
            InUse = false;
        }

        public ActivityModel(Activity activity)
        {
            Id = activity.Id;
            Name = activity.Name;
            Description = activity.Description;
            Disable = activity.Disable;
            InUse = false;
        }
    }

    public class ActivityEventModel
    {
        public long Id { get; set; }
        public string Name { get; set; }
        //public bool InEvent { get; set; }
        public string Description { get; set; }
        public bool Disable { get; set; }
        public int Index { get; set; }

        public bool IsSelected { get; set; }

        public bool EditMode { get; set; }

        //define if user has edit access to the schedule
        public bool EditAccess { get; set; }

        public ActivityEventModel(Activity activity)
        {
            Id = activity.Id;
            Name = activity.Name;
            Description = activity.Description;
            Disable = activity.Disable;
            IsSelected = false;
        }

        public ActivityEventModel(ActivityEventModel activityEventModel, int newIndex)
        {
            Id = activityEventModel.Id;
            Name = activityEventModel.Name;
            Description = activityEventModel.Description;
            Disable = activityEventModel.Disable;
            IsSelected = activityEventModel.IsSelected;
            EditMode = activityEventModel.EditMode;
            EditAccess = activityEventModel.EditAccess;
            Index = newIndex;
        }

    }
}