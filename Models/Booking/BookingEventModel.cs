using System;
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
using Vaiona.Persistence.Api;

namespace BExIS.Web.Shell.Areas.RBM.Models.Booking
{
    public class BookingEventModel
    {
        public DateTime startDate;
        public DateTime endDate;

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

        public string ResourceName { get; set; }
        public string ResourceAttributes { get; set; }

        public BookingEventModel()
        {
            Schedules = new List<ScheduleEventModel>();
            DeletedSchedules = new List<long>();
        }

        public BookingEventModel(List<ResourceCart> cart)
        {
            using (var userManager = new UserManager())
            using (var rManager = new ResourceManager())
            {
                Schedules = new List<ScheduleEventModel>();
                DeletedSchedules = new List<long>();

                foreach (ResourceCart rc in cart)
                {
                    SingleResource resource = rManager.GetResourceById(rc.Id);
                    ScheduleEventModel s = new ScheduleEventModel(resource);
                    s.ScheduleDurationModel.Index = rc.Index;
                    s.ScheduleQuantity = 1; // allways selct one by default
                    s.ResourceAttributeValues = resource.ResourceAttributeValues;
                    s.ResourceQuantity = resource.Quantity;
                    s.ScheduleDurationModel.StartDate = DateTime.Now;
                    s.ScheduleDurationModel.EndDate = DateTime.Now;
                    s.ByPerson = rc.ByPersonName;
                    s.EditAccess = true;
                    s.EditMode = true;


                    //add as default resvered by user as reserved for user
                    var userTask = userManager.FindByIdAsync(rc.ByPersonUserId);
                    userTask.Wait();
                    var user = userTask.Result;

                    PersonInSchedule byPerson = new PersonInSchedule(0, user, false);
                    byPerson.IsContactPerson = true;
                    s.Contact = byPerson;
                    s.ContactName = rc.ByPersonName;
                    s.ForPersons.Add(byPerson);

                    s.Index = rc.Index;
                    Schedules.Add(s);
                }
            }
        }

        public BookingEventModel(BookingEvent e)
        {
            Id = e.Id;
            Name = e.Name;
            Description = e.Description;
            Schedules = new List<ScheduleEventModel>();
            DeletedSchedules = new List<long>();

            EditMode = false;
            EditAccess = false;
            DeleteAccess = false;

            using (BookingEventManager em = new BookingEventManager())
            {
                //Get event again as not everything needed already fetched
                var event_ = em.GetBookingEventById(e.Id);


                foreach (Schedule s in event_.Schedules)
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
    }

    public class CalendarItemsStoryModel
    {
        public long eventId { get; set; }
        public string title { get; set; }
        public string start { get; set; }
        public string end { get; set; }
        public string color { get; set; }
        public bool allDay { get; set; }


        public CalendarItemsStoryModel(string resourceName, string resourceColor, SystemDefinedUnit timeUnit, DateTime startDate, DateTime endDate, long eId, long rQuantity, long sQuantity, string byPerson)
        {
            eventId = eId;
            if (rQuantity > 0)
            {
                title = resourceName + " (" + sQuantity + "/" + rQuantity + ")";
            }
            else
            {
                title = resourceName;
            }

            title = title + " (" + byPerson + ")";

            color = resourceColor;
            start = startDate.ToString("yyyy-MM-dd");
            end = endDate.ToString("yyyy-MM-dd");


            if (timeUnit == SystemDefinedUnit.day || timeUnit == SystemDefinedUnit.week || timeUnit == SystemDefinedUnit.year)
                allDay = true;
            else
                allDay = false;

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
                title = e.Name + " (" + e.Schedules.First().ByPerson.Person.DisplayName + ")";
                start = (from d in schedules select d.StartDate).Min();
                end = (from d in schedules select d.EndDate).Max();
                color = "#3868c8"; // fix color, maybe later a other solution for the event color
            }
        }

        public CalendarItemsModel(string resourceName,string resourceColor, SystemDefinedUnit timeUnit, DateTime startDate, DateTime endDate, long eId, long rQuantity, long sQuantity, string byPerson)
        {
            eventId = eId;
            if (rQuantity > 0)
            {
                title = resourceName + " (" + sQuantity + "/" + rQuantity + ")";
            }
            else
            {
                title = resourceName;
            }

            title = title + " (" + byPerson + ")";
            
            color = resourceColor;
            start = startDate;
            end = endDate;


            if (timeUnit == SystemDefinedUnit.day || timeUnit == SystemDefinedUnit.week || timeUnit == SystemDefinedUnit.year)
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
                //ScheduleManager schManager = new ScheduleManager();
                //List<Schedule> schedules = schManager.GetAllSchedulesByEvent(eEvent.Id);

                Schedules = new List<ScheduleEventModel>();

                foreach (Schedule s in eEvent.Schedules)
                {
                    ScheduleEventModel rEventM = new ScheduleEventModel(s);
                    Schedules.Add(rEventM);
                }

                //Activities = new List<ActivityModel>();
                //eEvent.Activities.ToList().ForEach(r => Activities.Add(new ActivityModel(r)));
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
            public bool IsPreSelected { get; set; }
            public bool ShowMyBookings { get; set; }
            public List<AttributeDomainItemsModel> TreeItems { get; set; }


            public ResourceFilterModel()
            {
                IsPreSelected = false;
                ShowMyBookings = false;
                TreeItems = new List<AttributeDomainItemsModel>();
            }
        }
 }
