﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using R = BExIS.Rbm.Entities.Resource;
using E = BExIS.Rbm.Entities.Booking;
using BExIS.Rbm.Entities.Booking;
using BExIS.Rbm.Services.Booking;
using BExIS.Web.Shell.Areas.RBM.Models.Resource;
using BExIS.Web.Shell.Areas.RBM.Models.ResourceStructure;
using BExIS.Rbm.Entities.ResourceStructure;
using BExIS.Dlm.Entities.DataStructure;
using BExIS.Rbm.Services.Resource;
using BExIS.Rbm.Entities.Resource;
using BExIS.Security.Entities.Subjects;
using BExIS.Security.Services.Subjects;
using System.ComponentModel.DataAnnotations;
using BExIS.Rbm.Entities.BookingManagementTime;
using BExIS.Dlm.Services.Party;
using BExIS.Dlm.Entities.Party;

namespace BExIS.Web.Shell.Areas.RBM.Models.Booking
{
    public class EventModel
    {
        public long Id { get; set; }

        [StringLength(50, ErrorMessage = "The name must be {2} - {1} characters long.", MinimumLength = 3)]
        [Required]
        public string Name { get; set; }
        public string Description { get; set; }

        public bool EditMode { get; set; }

        //define if user has edit access to the attribute
        public bool EditAccess { get; set; }

        //define if user has delete access to the attribute
        public bool DeleteAccess { get; set; }

        public List<ScheduleEventModel> Schedules { get; set; }

        //store deleted schedule temporär
        public List<long> DeletedSchedules { get; set; }

        public EventModel()
        {
            Schedules = new List<ScheduleEventModel>();
            DeletedSchedules = new List<long>();
        }

        public EventModel(List<ResourceCart> cart)
        {
            using (var rManager = new SingleResourceManager())
            {
                Schedules = new List<ScheduleEventModel>();
                DeletedSchedules = new List<long>();

                foreach (ResourceCart rc in cart)
                {
                    SingleResource resource = rManager.GetResourceById(rc.Id);
                    ScheduleEventModel s = new ScheduleEventModel(resource);
                    s.ScheduleDurationModel.StartDate = rc.PreselectedStartDate;
                    s.ScheduleDurationModel.EndDate = rc.PreselectedEndDate;
                    s.ScheduleDurationModel.Index = rc.Index;
                    s.ScheduleQuantity = rc.PreselectdQuantity;
                    s.ResourceQuantity = resource.Quantity;
                    s.ByPerson = rc.ByPersonName;

                    //add as default resvered by user as reserved for user
                    UserManager userManager = new UserManager();
                    var userTask = userManager.FindByIdAsync(rc.ByPersonUserId);
                    userTask.Wait();
                    var user = userTask.Result;

                    PersonInSchedule byPerson = new PersonInSchedule(0, user, false);
                    s.ForPersons.Add(byPerson);

                    s.Index = rc.Index;
                    Schedules.Add(s);
                }
            }
        }

        public EventModel(BookingEvent e)
        {
            Id = e.Id;
            Name = e.Name;
            Description = e.Description;
            Schedules = new List<ScheduleEventModel>();
            DeletedSchedules = new List<long>();

            EditMode = false;
            EditAccess = false;
            DeleteAccess = false;

            //foreach(Schedule s in e.Schedules)
            //{
            //    ScheduleEventModel seM = new ScheduleEventModel(s);
            //    seM.Index = s.Index;
            //    seM.EditMode = false;
            //    seM.EventId = e.Id;
            //    seM.Activities = new List<ActivityEventModel>();
            //    s.Activities.ToList().ForEach(r => seM.Activities.Add(new ActivityEventModel(r)));
            //    Schedules.Add(seM);
            //}

            ScheduleManager schManager = new ScheduleManager();
            List<Schedule> schedules = schManager.GetAllSchedulesByEvent(e.Id);

            ActivityManager aManager = new ActivityManager();

            Schedules = new List<ScheduleEventModel>();
            foreach (Schedule s in schedules)
            {
                ScheduleEventModel seM = new ScheduleEventModel(s);
                seM.Index = s.Index;
                seM.EditMode = false;
                seM.EventId = e.Id;
                seM.Activities = new List<ActivityEventModel>();
                s.Activities.ToList().ForEach(r => seM.Activities.Add(new ActivityEventModel(r)));
                Schedules.Add(seM);

            }
        }
    }


    public class CalendarItemsModel
    {
        public long eventId { get; set; }
        public string title { get; set; }
        public DateTime start { get; set; }
        public DateTime end { get; set; }
        public string color { get; set; }
        public bool allDay { get; set; }

        public CalendarItemsModel()
        {
            eventId = 0;
            title = "";
            start = new DateTime();
            end = new DateTime();
            color = "";
            //Resource = new R.Resource();
        }

        public CalendarItemsModel(BookingEvent e)
        {
            using (var schManager = new ScheduleManager())
            {
                List<Schedule> schedules = schManager.GetAllSchedulesByEvent(e.Id);

                eventId = e.Id;
                title = e.Name;
                start = (from d in schedules select d.StartDate).Min();
                end = (from d in schedules select d.EndDate).Max();
                color = "#3868c8"; // fix color, maybe later a other solution for the event color
            }
        }

        public CalendarItemsModel(R.SingleResource resource, DateTime startDate, DateTime endDate, long eId)
        {
            eventId = eId;
            title = resource.Name;
            color = resource.Color;
            start = startDate;
            end = endDate;

            if (resource.Duration.Self.TimeUnit == SystemDefinedUnit.day || resource.Duration.Self.TimeUnit == SystemDefinedUnit.week || resource.Duration.Self.TimeUnit == SystemDefinedUnit.year)
                allDay = true;
            else
                allDay = false;
        }
    }


        public class ShowEventModel
        {
            public long Id { get; set; }
            public List<ScheduleEventModel> Schedules { get; set; }
            public List<ActivityModel> Activities { get; set; }
            public List<PersonInSchedule> ForPersons { get; set; }

            //define if user has edit access to the attribute
            public bool EditAccess { get; set; }

            //define if user has delete access to the attribute
            public bool DeleteAccess { get; set; }

            public ShowEventModel()
            {
                EditAccess = false;
                DeleteAccess = false;
            }

            public ShowEventModel(BookingEvent eEvent)
            {
                ScheduleManager schManager = new ScheduleManager();
                List<Schedule> schedules = schManager.GetAllSchedulesByEvent(eEvent.Id);

                Schedules = new List<ScheduleEventModel>();

                foreach (Schedule s in schedules)
                {
                    ScheduleEventModel rEventM = new ScheduleEventModel(s);
                    Schedules.Add(rEventM);
                }

                Activities = new List<ActivityModel>();
                eEvent.Activities.ToList().ForEach(r => Activities.Add(new ActivityModel(r)));
            }
        }

        public class SelectResourceForEventModel
        {
            public long Id { get; set; }
            public string Name { get; set; }
            public string Description { get; set; }
            public string ResourceQuantity { get; set; }
            public int AvailableQuantity { get; set; }


            public SelectResourceForEventModel()
            {
            }

            public SelectResourceForEventModel(SingleResource resource)
            {
                Id = resource.Id;
                Name = resource.Name;
                Description = resource.Description;

                if (resource.Quantity == 0)
                    ResourceQuantity = "no limitation";
                else
                    ResourceQuantity = resource.Quantity.ToString();

            }

        }

        public class AttributeDomainItemsModel
        {
            public long AttrId { get; set; }
            public string AttrName { get; set; }
            public List<DomainItemModel> DomainItems { get; set; }

            public AttributeDomainItemsModel(ResourceStructureAttribute attr)
            {
                AttrId = attr.Id;
                AttrName = attr.Name;
                DomainItems = new List<DomainItemModel>();

                if (attr.Constraints != null)
                {
                    foreach (Constraint constraint in attr.Constraints)
                    {
                        if (constraint is DomainConstraint)
                        {
                            DomainConstraint dc = (DomainConstraint)constraint;
                            dc.Materialize();
                            List<DomainItemModel> domainItemModelList = new List<DomainItemModel>();
                            dc.Items.ToList().ForEach(r => domainItemModelList.Add(new DomainItemModel(r)));
                            DomainItems = domainItemModelList;
                        }
                    }
                }
            }
        }


        //Model for the TreeView to Filter the Resources by Domain Constrains in the Create/Edit Event view
        public class ResourceFilterModel
        {
            public DateTime? StartDate { get; set; }
            public DateTime? EndDate { get; set; }
            public int Quantity { get; set; }
            public bool IsPreSelected { get; set; }
            public bool ShowMyBookings { get; set; }
            public List<AttributeDomainItemsModel> TreeItems { get; set; }


            public ResourceFilterModel()
            {
                StartDate = null;
                EndDate = null;
                Quantity = 0;
                IsPreSelected = false;
                ShowMyBookings = false;
                TreeItems = new List<AttributeDomainItemsModel>();
            }
        }
 }