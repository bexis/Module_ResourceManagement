using BExIS.Rbm.Entities.Booking;
using R = BExIS.Rbm.Entities.Resource;
using BExIS.Web.Shell.Areas.RBM.Models.Resource;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Services;
using BExIS.Rbm.Services.Resource;
using BExIS.Security.Entities.Subjects;
using BExIS.Rbm.Entities.Users;
using System.ComponentModel.DataAnnotations;
using BExIS.Rbm.Entities.Resource;
using BExIS.Rbm.Entities.ResourceStructure;
using BExIS.Web.Shell.Areas.RBM.Models.ResourceStructure;
using BExIS.Rbm.Services.ResourceStructure;
using BExIS.Rbm.Entities.BookingManagementTime;
using BExIS.Modules.RBM.UI.Helper;
using BExIS.Dlm.Entities.Party;
using BExIS.Dlm.Services.Party;



namespace BExIS.Web.Shell.Areas.RBM.Models.Booking
{
    public class ScheduleModel
    {
        public  DateTime StartDate { get; set; }
        public  DateTime EndDate { get; set; }

        public ResourceModel Resource { get; set; }
        //public EventModel Event { get; set; }

        public ScheduleModel()
        {
            StartDate = new DateTime();
            EndDate = new DateTime();
            Resource = new ResourceModel();
            //Event = new EventModel();
        }

        public ScheduleModel(Schedule schedule)
        {
            StartDate = schedule.StartDate;
            EndDate = schedule.EndDate;
            Resource = new ResourceModel(schedule.Resource);
            //Event = new EventModel(schedule.Event);
        }

    }

    //For the Start/End Partial View in schedule
    public class ScheduleDurationModel
    {
        public int Index { get; set; }
        [Display(Name = "From")]
        public DateTime StartDate { get; set; }
        [Display(Name = "To")]
        public DateTime EndDate { get; set; }
        public int DurationValue { get; set; }
        public SystemDefinedUnit TimeUnit { get; set; }

        public long EventId { get; set; }

        public bool EditMode { get; set; }

        //define if user has edit access to the time period
        public bool EditAccess { get; set; }

        public ScheduleDurationModel()
        {
            StartDate = new DateTime();
            EndDate = new DateTime();
        }

        //copy hole ScheduleDurationModel with new index
        public ScheduleDurationModel(ScheduleDurationModel scheduleDurationModel, int newIndex)
        {
            Index = newIndex;
            StartDate = scheduleDurationModel.StartDate;
            EndDate = scheduleDurationModel.EndDate;
            EventId = scheduleDurationModel.EventId;
            DurationValue = scheduleDurationModel.DurationValue;
            EditMode = scheduleDurationModel.EditMode;
            EditAccess = scheduleDurationModel.EditAccess;
        }
    }

   
    public class ResourceCart
    {
        public long Id { get; set; }
        public string Name { get; set; }
        public int ResourceQuantity { get; set; }
        //public int AvailableQuantity { get; set; }

        public string ByPersonName { get; set; }
        public long ByPersonUserId { get; set; }

        //Status must be included
        public int Index { get; set; }

        public bool NewInCart { get; set; }

        public ResourceCart()
        {
           
        }

    }

    public class AvailabilityScheduleModel
    {
        public long CurrentResourceId { get; set; }
        public string CurrentResourceName { get; set; }
        public DateTime AlternateStartDate { get; set; }
        public DateTime AlternateEndDate { get; set; }
        public int Index { get; set; }

        public List<AlternateEventResource> AlternateResources { get; set; }

        public AvailabilityScheduleModel()
        {
            AlternateResources = new List<AlternateEventResource>();
        }
    }

    //show alternate resources model
    public class AlternateEventResource
    {
        public long ResourceId { get; set; }
        public string ResourceName { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public bool isChoosen { get; set; }

        public long ContactUserId { get; set; }

        public AlternateEventResource(SingleResource singleResource, DateTime startDate, DateTime enddate, long contactId)
        {
            ResourceId = singleResource.Id;
            ResourceName = singleResource.Name;
            StartDate = startDate;
            EndDate = enddate;
            ContactUserId = contactId;
        }

        public AlternateEventResource()
        {

        }
    }

    public class ScheduleListModel
    {
        public long Id { get; set; }

        public long EventID { get; set; }

        public string EventName { get; set; }

        public string EventDescription { get; set; }

        public string ResourceName { get; set; }

        public string ReservedFor { get; set; }

        public string ContactPerson { get; set; }

        public DateTime StartDate { get; set; }

        public DateTime EndDate { get; set; }

        public string Quantity { get; set; }

        public string Activities { get; set; }

        public ScheduleListModel()
        {

        }

        //public ScheduleListModel(Schedule s)
        public ScheduleListModel(long eventId,
            string bookingEventName, string bookingEventDescription, string resourceName, DateTime start, DateTime end, string quantity, Person forPerson, ICollection<Activity> activities, string contactPerson)
        {

            EventID = eventId;
            EventName = bookingEventName;
            EventDescription = bookingEventDescription;
            ResourceName = resourceName;

            StartDate = start;
            EndDate = end;
            Quantity = quantity;


            ContactPerson = contactPerson;



           
            
            if (forPerson is PersonGroup)
            {
                PersonGroup pg = (PersonGroup)forPerson;
                int count = 1;
                foreach (User u in pg.Users)
                {
                    //string reservedForName = getPartyName(u.Id);
                    //if (reservedForName != "false")
                   // {
                   //     u.Name = reservedForName;
                   // }

                    if (count < pg.Users.Count())
                        ReservedFor += u.DisplayName + ", ";
                    else
                        ReservedFor += u.DisplayName;

                    count++;

                   

                }
            }
            else if (forPerson is IndividualPerson)
            {
                IndividualPerson ip = (IndividualPerson)forPerson;
                ReservedFor = ip.Person.DisplayName;
            }

            Activities = string.Join(", ", activities.Select( a => a.Name ) );

        }


        private string getPartyName(long Id)
        {
            using (var partyManager = new PartyManager())
            {
                Party party = partyManager.GetPartyByUser(Id);
                if (party != null)
                {
                    return party.Name;
                }
                else
                {
                    return "false";
                }
            }

        }
    }
    //Model for a schedule in a event
    public class ScheduleEventModel
    {
        public int Index { get; set; }

        public long EventId { get; set; }

        public long ScheduleId { get; set; }

        public long ResourceId { get; set; }

        public string ResourceName { get; set; }

        public string ResourceDescription { get; set; }

        public ICollection<ResourceAttributeValue> ResourceAttributeValues { get; set; }


        public bool WithActivity { get; set; }

        public bool EditMode { get; set; }

        //define if user has edit access to the schedule
        public bool EditAccess { get; set; }

        //define if user has delete access to the schedule
        public bool DeleteAccess { get; set; }

        //store the start and enddate of a schedule
        public ScheduleDurationModel ScheduleDurationModel { get; set; }

        //Error(s) in the hole Event
        public bool Errors { get; set; }
        //Error at a schedule
        public bool ScheduleError { get; set; }

        public List<PersonInSchedule> ForPersons { get; set; }
        public List<ActivityEventModel> Activities { get; set; }

        public string ByPerson { get; set; }
        public PersonInSchedule Contact { get; set; }

        //def. Quantity for a schedule
        [Display(Name = "Number")]
        public int ScheduleQuantity { get; set; }
        // Quantity of the resource
        public int ResourceQuantity { get; set; }
        // Quantity Available calulated with the resource Quantity and booked schedules for the resource
        public int AvailableQuantity { get; set; }

        public string Status { get; set; }

        [Display(Name = "Contact")]
        public string ContactName { get; set; }

        //Files
        public bool ResourceHasFiles { get; set; }
        public List<FileValueModel> Files { get; set; }
        public bool FileConfirmation { get; set; }


        public ScheduleEventModel()
        {
            //ByPerson = new PersonInSchedule();
            ForPersons = new List<PersonInSchedule>();
            Contact = new PersonInSchedule();
            Activities = new List<ActivityEventModel>();
            ScheduleDurationModel = new ScheduleDurationModel();
            
        }

        // Copy constructor
        public ScheduleEventModel(ScheduleEventModel previousObject, int newIndex)
        {
            Index = newIndex;
            EventId = previousObject.EventId;
            ResourceId = previousObject.ResourceId;
            ResourceName = previousObject.ResourceName;
            ResourceDescription = previousObject.ResourceDescription;
            WithActivity = previousObject.WithActivity;
            ScheduleDurationModel = new ScheduleDurationModel(previousObject.ScheduleDurationModel, newIndex);
            EditMode = previousObject.EditMode;
            EditAccess = previousObject.EditAccess;
            DeleteAccess = previousObject.DeleteAccess;
            ForPersons = new List<PersonInSchedule>(previousObject.ForPersons);
            Activities = new List<ActivityEventModel>(previousObject.Activities);
            ByPerson = previousObject.ByPerson;
            Contact = previousObject.Contact;
            ScheduleQuantity = previousObject.ScheduleQuantity;
            ResourceQuantity = previousObject.ResourceQuantity;
            ContactName = previousObject.ContactName;
            ResourceHasFiles = previousObject.ResourceHasFiles;
            Files = previousObject.Files;
        }

        public ScheduleEventModel(R.SingleResource resource)
        {
            ResourceId = resource.Id;
            ResourceName = resource.Name;
            ResourceDescription = resource.Description;
            WithActivity = resource.WithActivity;
            ResourceQuantity = resource.Quantity;

            ScheduleDurationModel scheduleDurationModel = new ScheduleDurationModel();
            scheduleDurationModel.DurationValue = resource.Duration.Value;
            scheduleDurationModel.TimeUnit = resource.Duration.TimeUnit;
            ScheduleDurationModel = scheduleDurationModel;

            //ByPerson = new PersonInSchedule();
            ForPersons = new List<PersonInSchedule>();
            Contact = new PersonInSchedule();
            Activities = new List<ActivityEventModel>();
            Files = new List<FileValueModel>();

            //Get File to Resource if exsist
            foreach (ResourceAttributeUsage usage in resource.ResourceStructure.ResourceAttributeUsages)
            {
                if (usage.IsFileDataType)
                {
                    using (ResourceStructureAttributeManager valueManager = new ResourceStructureAttributeManager())
                    {
                        ResourceAttributeValue value = valueManager.GetValueByUsageAndResource(usage.Id, resource.Id);
                        if (value is FileValue)
                        {
                            FileValue fileValue = (FileValue)value;
                            FileValueModel fvm = new FileValueModel(fileValue);
                            Files.Add(fvm);
                            //if (!usage.IsValueOptional)
                            if (fvm.Data != null)
                                ResourceHasFiles = true;
                        }
                    }
                }
            }
        }

        public ScheduleEventModel(Schedule schedule)
        {
            ScheduleId = schedule.Id;
            ResourceId = schedule.Resource.Id;
            ResourceName = schedule.Resource.Name;
            ResourceDescription = schedule.Resource.Description;
            ResourceAttributeValues = schedule.Resource.ResourceAttributeValues;
            Files = new List<FileValueModel>();
            WithActivity = schedule.Resource.WithActivity;

            //Get File to Resource if exsist
            foreach (ResourceAttributeUsage usage in schedule.Resource.ResourceStructure.ResourceAttributeUsages)
            {
                if (usage.IsFileDataType)
                {
                    using (ResourceStructureAttributeManager valueManager = new ResourceStructureAttributeManager())
                    {
                        ResourceAttributeValue value = valueManager.GetValueByUsageAndResource(usage.Id, schedule.Resource.Id);
                        if (value is FileValue)
                        {
                            FileValue fileValue = (FileValue)value;
                            FileValueModel fvm = new FileValueModel(fileValue);
                            Files.Add(fvm);
                            if (!usage.IsValueOptional)
                                ResourceHasFiles = true;
                        }
                    }
                        
                }
            }

            ScheduleDurationModel scheduleDurationModel = new ScheduleDurationModel();
            scheduleDurationModel.StartDate = schedule.StartDate;
            scheduleDurationModel.EndDate = schedule.EndDate;
            scheduleDurationModel.DurationValue = schedule.Resource.Duration.Value;
            scheduleDurationModel.TimeUnit = schedule.Resource.Duration.TimeUnit;

            ScheduleDurationModel = scheduleDurationModel;
            ScheduleQuantity = schedule.Quantity;
            ResourceQuantity = schedule.Resource.Quantity;
            Status = schedule.Resource.ResourceStatus.ToString();

            //get Persons
            ForPersons = new List<PersonInSchedule>();

            if (schedule.ForPerson.Self is PersonGroup)
            {
                PersonGroup pGroup = (PersonGroup)schedule.ForPerson.Self;

                Contact = new PersonInSchedule(pGroup.Id, pGroup.Contact, true);
                ContactName = pGroup.Contact.DisplayName;

                foreach (User u in pGroup.Users)
                {
                    if (u.Id == pGroup.Contact.Id)
                    {
                        ForPersons.Add(new PersonInSchedule(pGroup.Id, u, true));
                    }
                    else
                    {
                        ForPersons.Add(new PersonInSchedule(pGroup.Id, u, false));
                    }
                }
            }
            else if (schedule.ForPerson.Self is IndividualPerson)
            {
                IndividualPerson iPerson = (IndividualPerson)schedule.ForPerson.Self;
                Contact = new PersonInSchedule(iPerson.Id, iPerson.Contact, true);
                ContactName = iPerson.Contact.DisplayName;

                ForPersons.Add(new PersonInSchedule(iPerson.Id,iPerson.Person, true));
            }

            if (schedule.ByPerson.Self is IndividualPerson)
            {
                IndividualPerson iPersonBy = (IndividualPerson)schedule.ByPerson.Self;

                ByPerson = iPersonBy.Person.DisplayName; 
            }
        }
    }
}
