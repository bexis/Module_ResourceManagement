using BExIS.Rbm.Entities.Booking;
using BExIS.Rbm.Entities.ResourceStructure;
using BExIS.Rbm.Services.Booking;
using BExIS.Web.Shell.Areas.RBM.Models.Resource;
using BExIS.Web.Shell.Areas.RBM.Models.ResourceStructure;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace BExIS.Web.Shell.Areas.RBM.Models.Booking
{

    #region Notification Models

    public class NotificationModel
    {
        public long Id { get; set; }
        public string Subject { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string Message { get; set; }

        //define if user has edit access to the attribute
        public bool EditAccess { get; set; }

        //define if user has delete access to the attribute
        public bool DeleteAccess { get; set; }

        //public List<ScheduleModel> Schedules { get; set; }


        public NotificationModel()
        {
            //Schedules = new List<ScheduleModel>();
        }

        public NotificationModel(Notification notification)
        {
            Id = notification.Id;
            Subject = notification.Subject;
            StartDate = notification.StartDate;
            EndDate = notification.EndDate;
            Message = notification.Message;
            //Schedules = new List<ScheduleModel>();
            //notification.Schedules.ToList().ForEach(r => Schedules.Add(new ScheduleModel(r)));
        }

    }

    //public class CreateNotificationModel
    //{
    //    public string Subject { get; set; }
    //    public DateTime StartDate { get; set; }
    //    public DateTime EndDate { get; set; }
    //    public string Message { get; set; }

    //    public List<AttributeDomainItemsModel> AttributeDomainItems { get; set; }
    //    public List<DomainItemModel> DomainItems { get; set; }
    //    public List<ScheduleModel> Schedules { get; set; }

    //    public CreateNotificationModel()
    //    {

    //    }

    //    public CreateNotificationModel(List<ResourceModel> resources)
    //    {
    //        Schedules = new List<ScheduleModel>();
    //        AttributeDomainItems = new List<AttributeDomainItemsModel>();
    //        DomainItems = new List<DomainItemModel>();
    //        StartDate = new DateTime();
    //        EndDate = new DateTime();

    //        foreach (ResourceModel r in resources)
    //        {
    //            foreach (ResourceAttributeUsage usage in r.ResourceStructure.ResourceAttributeUsages)
    //            {
    //                ResourceStructureAttribute attr = usage.ResourceStructureAttribute;
    //                AttributeDomainItemsModel item = new AttributeDomainItemsModel(attr);
    //                if (!AttributeDomainItems.Any(a => a.AttrId == item.AttrId))
    //                    AttributeDomainItems.Add(item);
    //            }
    //        }
    //    }
    //}

    public class EditNotificationModel
    {
        public long Id { get; set; }

        [Display(Name = "Subject")]
        [StringLength(50, ErrorMessage = "The subject must be {2} - {1} characters long.", MinimumLength = 3)]
        [Required]
        public string Subject { get; set; }

        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }

        [Display(Name = "Message")]
        [StringLength(500, ErrorMessage = "The message must be {2} - {1} characters long.", MinimumLength = 10)]
        [Required]
        public string Message { get; set; }


        public List<AttributeDomainItemsModel> AttributeDomainItems { get; set; }
        public List<DomainItemModel> DomainItems { get; set; }
        public List<NotificationDependencyModel> NotificationDependencies { get; set; }

        public EditNotificationModel()
        {
            NotificationDependencies = new List<NotificationDependencyModel>();
        }


        public EditNotificationModel(List<ResourceModel> resources)
        {
            //Schedules = new List<ScheduleModel>();
            AttributeDomainItems = new List<AttributeDomainItemsModel>();
            DomainItems = new List<DomainItemModel>();
            StartDate = new DateTime();
            EndDate = new DateTime();
            NotificationDependencies = new List<NotificationDependencyModel>();

            foreach (ResourceModel r in resources)
            {
                foreach (ResourceAttributeUsage usage in r.ResourceStructure.ResourceAttributeUsages)
                {
                    ResourceStructureAttribute attr = usage.ResourceStructureAttribute;
                    AttributeDomainItemsModel item = new AttributeDomainItemsModel(attr);
                    if (item.DomainItems.Count != 0)
                    {
                        if (!AttributeDomainItems.Any(a => a.AttrId == item.AttrId))
                            AttributeDomainItems.Add(item);
                    }
                }
            }
        }


        public EditNotificationModel(List<ResourceModel> resources, Notification notification)
        {
            Subject = notification.Subject;
            StartDate = notification.StartDate;
            EndDate = notification.EndDate;
            Message = notification.Message;

            AttributeDomainItems = new List<AttributeDomainItemsModel>();
            DomainItems = new List<DomainItemModel>();
            NotificationDependencies = new List<NotificationDependencyModel>();

            //Add Attributes with domain items
            foreach (ResourceModel r in resources)
            {
                foreach (ResourceAttributeUsage usage in r.ResourceStructure.ResourceAttributeUsages)
                {
                    ResourceStructureAttribute attr = usage.ResourceStructureAttribute;
                    AttributeDomainItemsModel item = new AttributeDomainItemsModel(attr);
                    if (item.DomainItems.Count != 0)
                    {
                        if (!AttributeDomainItems.Any(a => a.AttrId == item.AttrId))
                            AttributeDomainItems.Add(item);
                    }
                }
            }

            //Get all dependencies for the notification
            NotificationDependencies = new List<NotificationDependencyModel>();
            using (NotificationManager nManager = new NotificationManager())
            {
                List<NotificationDependency> ndList = nManager.GetNotificationDependenciesByNotification(notification.Id);
                ndList.ToList().ForEach(r => NotificationDependencies.Add(new NotificationDependencyModel(r)));
            }
        }
    }

    #endregion

    #region NotificationBlackboard Model

    public class NotificationBlackboardModel
    {
        public ICollection<NotificationDependency> NotificationDependency { get; private set; }
        public string Subject { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public DateTime InsertDate { get; set; }
        public string Message { get; set; }
        public List<string> DomainItems { get; set; }



        public NotificationBlackboardModel(Notification notification)
        {
            NotificationDependency = notification.NotificationDependency;
            Subject = notification.Subject;
            StartDate = notification.StartDate;
            EndDate = notification.EndDate;
            Message = notification.Message;
            InsertDate = notification.InsertDate;
        }
    }

    #endregion Model

    #region NotificationDependency Models

    public class NotificationDependencyModel
    {
        public Notification Notification { get; set; }
        public string DomainItem { get; set; }
        public long AttributeId { get; set; }


        public NotificationDependencyModel(NotificationDependency notificationDependency)
        {
            Notification = notificationDependency.Notification;
            DomainItem = notificationDependency.DomainItem;
            AttributeId = notificationDependency.AttributeId;
        }
    }


    #endregion
}