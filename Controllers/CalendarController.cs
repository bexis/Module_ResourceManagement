using BExIS.Modules.RBM.UI.Helper;
using BExIS.Rbm.Entities.Booking;
using BExIS.Rbm.Entities.BookingManagementTime;
using BExIS.Rbm.Entities.Resource;
using BExIS.Rbm.Entities.ResourceStructure;
using BExIS.Rbm.Services.ResourceStructure;
using BExIS.Rbm.Entities.Users;
using BExIS.Rbm.Services.Booking;
using BExIS.Rbm.Services.Resource;
using BExIS.Security.Entities.Subjects;
using BExIS.Security.Services.Subjects;
using BExIS.Web.Shell.Areas.RBM.Helpers;
using BExIS.Web.Shell.Areas.RBM.Models.Booking;
using BExIS.Web.Shell.Areas.RBM.Models.Resource;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using System.Web.Services;
using Vaiona.Persistence.Api;
using Vaiona.Web.Extensions;
using Vaiona.Web.Mvc.Models;

namespace BExIS.Modules.RBM.UI.Controllers
{
    public class CalendarController : Controller
    {
        public ActionResult Calendar()
        {
            ViewBag.Title = PresentationModel.GetViewTitleForTenant("Fieldwork Calendar", this.Session.GetTenant());
            Session["Filter"] = null;
            Session["FilterSchedules"] = null;
            return View("Calendar");
        }

        #region  Calendar view

        // Get all events filtered by date and optional by user
        [WebMethod]
        public JsonResult GetAllEvents(string st, string ed, bool byUser = false)
        {
            List<BookingEvent> eventList = new List<BookingEvent>();
            eventList = GetAllEventsFiltered(byUser, st, ed);

            List<object> eventObjectList = new List<object>();
            foreach (BookingEvent e in eventList)
            {
                eventObjectList.Add(new object[] { JsonHelper.JsonSerializer<CalendarItemsModel>(new CalendarItemsModel(e)) });
            }

            return Json(eventObjectList.ToArray(), JsonRequestBehavior.AllowGet);
        }

        // Get all schedules filtered by date and optional by user
        [WebMethod]
        public JsonResult GetAllSchedules(string st, string ed, bool byUser = false)
        {

            List<Schedule> scheduleList = new List<Rbm.Entities.Booking.Schedule>();
            scheduleList = GetAllScheduleFiltered(byUser);

            List<object> scheduleObjectList = new List<object>();
            foreach (Schedule s in scheduleList)
            {
                using (IUnitOfWork uow = this.GetUnitOfWork())
                {
                    if (s.Resource == null) break;

                    var duration = uow.GetReadOnlyRepository<TimeDuration>().Get(s.Resource.Duration.Id);

                    if (duration == null) break;
                    var timeUnit = duration.TimeUnit;

                    scheduleObjectList.Add(new object[] { JsonHelper.JsonSerializer<CalendarItemsModel>(new CalendarItemsModel(s.Resource.Name, s.Resource.Color, timeUnit, s.StartDate, s.EndDate, s.BookingEvent.Id, s.Resource.Quantity, s.Quantity, s.ByPerson.Person.DisplayName)) });
                }
            }

            return Json(scheduleObjectList.ToArray(), JsonRequestBehavior.AllowGet);
        }

        #endregion

        #region List view
        // get all events as list and optional by user
        public ActionResult GetEventsAsList(string myBookings, string history = "false")
        {
            List<BookingEventModel> model = new List<BookingEventModel>();
            List<BookingEvent> eventList = new List<BookingEvent>();

            // Get all events optional filtered by user
            if (bool.Parse(history) == true)
            {
                eventList = GetAllEventsFiltered(bool.Parse(myBookings)); // gut full list with history
                ViewBag.history = "true";
            }
            else
            {
                eventList = GetAllEventsFiltered(bool.Parse(myBookings), DateTime.Now.AddDays(-1).ToString()); // get list without historical events
                ViewBag.history = "false";
            }

            
            
            // Remove dublicate booking events in resources list (without filter not dublicates, but with)
            eventList = eventList.GroupBy(x => x.Id).Select(x => x.First()).ToList();


            foreach (BookingEvent e in eventList)
            {


      
         
                    BookingEventModel m = new BookingEventModel(e);

                    m.startDate = m.Schedules.Select(a => a.ScheduleDurationModel.StartDate).ToList().Min();
                    m.endDate = m.Schedules.Select(a => a.ScheduleDurationModel.EndDate).ToList().Max();
                    m.ResourceName = string.Join<string>(",", (m.Schedules.Select(a => a.ResourceName)).ToList());
                    List<string> attributes = new List<string>();





                    foreach (var c in m.Schedules.Select(a => a.ResourceAttributeValues).ToList())
                    {

                        // Assumption the first attribute (domain value) is of importance -> show it in the list
                        ResourceAttributeValue value = c.First();
                        if (value is TextValue)
                        {
                            TextValue tv = (TextValue)value;
                            attributes.Add(tv.Value.ToString());
                        }
                    }
                    m.ResourceAttributes = string.Join<string>(",", attributes.Distinct());

                    model.Add(m);
                }
            

            //model = model.OrderByDescending(a => a.startDate).ToList();

            return PartialView("_listEvents", model);
        }

        // get all schedules as list and optional by user
        public ActionResult GetSchedulesAsList(string myBookings, string history = "false")
        {

            List<Schedule> scheduleList = new List<Rbm.Entities.Booking.Schedule>();

            if (bool.Parse(history) == true)
            {
                scheduleList = GetAllScheduleFiltered(bool.Parse(myBookings)); // get full list
                ViewBag.history = "true";
            }
            else
            {
                scheduleList = GetAllScheduleFiltered(bool.Parse(myBookings), DateTime.Now.AddDays(-1).ToString()); // get list without historical events
                ViewBag.history = "false";
            }
                        
            List<ScheduleListModel> model = new List<ScheduleListModel>();
            

            using (var uow = this.GetUnitOfWork())
            {
                foreach (Schedule s in scheduleList)
                {
                    var booking = uow.GetReadOnlyRepository<BookingEvent>().Get(s.BookingEvent.Id);
                    var resource = uow.GetReadOnlyRepository<Resource>().Get(s.Resource.Id);
                    var forperson = uow.GetReadOnlyRepository<Person>().Get(s.ForPerson.Id);
                    
                    // Todo: maybe their is a more performant way to get the info for the ContactName
                    var schedule = uow.GetReadOnlyRepository<Schedule>().Get(s.Id);
                    ScheduleEventModel tempSchedule = new ScheduleEventModel(schedule);

                    model.Add(new ScheduleListModel(
                        booking.Id,
                        booking.Name,
                        booking.Description,
                        resource.Name,
                        s.StartDate,
                        s.EndDate,
                        "(" +  s.Quantity + "/" + s.Resource.Quantity + ")",
                        forperson,
                        s.Activities,
                        tempSchedule.ContactName
                        ));
                }
            }

            return PartialView("_listSchedules", model);
        }

        #endregion

        #region Show and set Filter

        public ActionResult TreeFilterSchedules()
        {
            ResourceFilterModel model = new ResourceFilterModel();
            
            using (ResourceStructureManager rsManager = new ResourceStructureManager())
         //   using(ResourceManager rManager = new ResourceManager())
            {

               // List<SingleResource> singleResources = rManager.GetAllReesources().ToList();


               // List<ResourceModel> resources = new List<ResourceModel>();
               // singleResources.ForEach(r => resources.Add(new ResourceModel(r)));
                List<ResourceStructure> resourceStructures = rsManager.GetAllResourceStructures().ToList();

                foreach (ResourceStructure rs in resourceStructures)
                {
                    foreach (ResourceAttributeUsage usage in rs.ResourceAttributeUsages)
                    {
                        ResourceStructureAttribute attr = usage.ResourceStructureAttribute;
                        AttributeDomainItemsModel item = new AttributeDomainItemsModel(attr);
                        if (item.DomainItems.Count != 0)
                        {
                            if (!model.TreeItems.Any(a => a.AttrId == item.AttrId))
                                model.TreeItems.Add(item);
                        }
                    }
                
                }

                /*foreach (ResourceModel r in resources)
                {
                    foreach (ResourceAttributeUsage usage in r.ResourceStructure.ResourceAttributeUsages)
                    {
                        ResourceStructureAttribute attr = usage.ResourceStructureAttribute;
                        AttributeDomainItemsModel item = new AttributeDomainItemsModel(attr);
                        if (item.DomainItems.Count != 0)
                        {
                            if (!model.TreeItems.Any(a => a.AttrId == item.AttrId))
                                model.TreeItems.Add(item);
                        }
                    }
                }*/
            }
            
            return PartialView("_treeFilterSchedules", model);
        }

        public ActionResult OnSelectTreeViewItemFilter(string selectedItems)
        {
            if (selectedItems == null)
            {
                Session["FilterSchedules"] = null;
                return new EmptyResult();
            }
            else
            {
                    using (ScheduleManager schManager = new ScheduleManager())
                    using (ResourceManager srManager = new ResourceManager())
                    {
                        //results after filtering
                        List<Schedule> resultScheduleList = new List<Schedule>();

                        //result resource list after filtering
                        List<long> resultResourceIDList = new List<long>();

                        //Filter is this format: AttrID_DomainItem, AttrID_Domain
                        List<string> items = selectedItems.Split(',').ToList();

                        //get all schedules
                        List<Schedule> allSchedules = schManager.GetAllSchedules().ToList();

                        //get all scheduled resources
                        List<SingleResource> resourcesList;
                        if (Session["resourcesList"] == null)
                        {
                            resourcesList = srManager.GetAllResources().ToList();
                            Session["resourcesList"] = resourcesList;
                        }
                        else
                        {
                            resourcesList = (List<SingleResource>)Session["resourcesList"];
                        }
                       

                        List<ResourceFilterHelper.FilterTreeItem> filterList = new List<ResourceFilterHelper.FilterTreeItem>();
                        //split Id and DomainItem and add it to a FilterItem list
                        foreach (string item in items)
                        {
                            //index 0 = attrbute id, index1 domainvalue
                            List<string> i = item.Split('_').ToList();
                            ResourceFilterHelper.FilterTreeItem filterItem = new ResourceFilterHelper.FilterTreeItem();
                            try
                            {
                                filterItem.Id = Convert.ToInt64(i[0]);
                            }
                            catch (Exception e)
                            {
                                Console.WriteLine(e);
                                throw;
                            }

                            try
                            {
                                filterItem.Value = i[1].ToString();
                            }
                            catch (Exception e)
                            {
                                Console.WriteLine(e);
                                throw;
                            }

                            filterList.Add(filterItem);
                        }

                        List<ResourceAttributeValueModel> treeDomainList = new List<ResourceAttributeValueModel>();

                        if (Session["treeDomainModel"] == null) {
                            //Create for each Resource TreeDomainModel witch includes all Attribute Ids and all values
                            foreach (SingleResource r in resourcesList)
                            {
                                ResourceAttributeValueModel treeDomainModel = new ResourceAttributeValueModel(r);
                                treeDomainList.Add(treeDomainModel);
                            }
                            Session["treeDomainModel"] = treeDomainList;
                        }
                        else
                        {
                            treeDomainList = (List<ResourceAttributeValueModel>)Session["treeDomainModel"];
                        }
                        

                        //Dictionary to save every Filter (domain items) to one attr
                        Dictionary<long, List<string>> filterDic = ResourceFilterHelper.GetFilterDic(filterList);

                        List<ResourceAttributeValueModel> temp = new List<ResourceAttributeValueModel>();

                        //check for every TreeDomainModel (resource) if fits the filter
                        foreach (ResourceAttributeValueModel m in treeDomainList)
                        {
                            if (ResourceFilterHelper.CheckTreeDomainModel(m, filterDic))
                            {
                                resultResourceIDList.Add(m.Resource.Id);
                            }
                        }

                        //create schedule resource list with selected resources
                        foreach (Schedule s in allSchedules)
                        {
                            if (resultResourceIDList.Contains(s.Resource.Id))
                            {
                                resultScheduleList.Add(s);
                            }
                        }

                        Session["FilterSchedules"] = resultScheduleList;

                        //return Redirect(Request.UrlReferrer.ToString());
                        return new EmptyResult();

                    }
        }
        }

        #endregion

        #region Functions

        private List<Schedule> FilterSchedulesByUser(List<Schedule> scheduleList, long userId)
        {
            scheduleList = scheduleList.Where(a => a.ByPerson.Person.Id == userId).ToList();

            return scheduleList;
        }

        private List<Schedule> FilterSchedulesForUser(List<Schedule> scheduleList, long userId)
        {
            List<Schedule> filteredScheduleList = new List<Schedule>();

            using (var uow = this.GetUnitOfWork())
            {
                foreach (Schedule s in scheduleList)
                {

                    var forPerson = uow.GetReadOnlyRepository<Person>().Get(s.ForPerson.Id);

                    if (forPerson is PersonGroup)
                    {
                        PersonGroup pg = (PersonGroup)forPerson;
                        foreach (User u in pg.Users)
                        {
                            if (u.Id == userId)
                            {
                                filteredScheduleList.Add(s);
                                break;
                            }
                        }
                    }
                    else if (forPerson is IndividualPerson)
                    {
                        IndividualPerson ip = (IndividualPerson)forPerson;
                        if (ip.Person.Id == userId)
                        {
                            filteredScheduleList.Add(s);
                            break;
                        }
                    }
                }
            }
            return filteredScheduleList.Distinct().ToList();
        }

        private List<BookingEvent> GetAllEventsFiltered(bool byUser, string start_date = null, string end_date = null)
        {
            List<BookingEvent> eventList = new List<BookingEvent>();
            List<BookingEvent> eventListTmp = new List<BookingEvent>();

            using(BookingEventManager eManager = new BookingEventManager())
            using(ScheduleManager schManager = new ScheduleManager())
            using(SubjectManager subManager = new SubjectManager())
            {
                if (Session["FilterSchedules"] == null)
                {

                    if (start_date != null && end_date != null)
                    {
                        DateTime startDate = DateTime.Parse(start_date.ToString());
                        DateTime endDate = DateTime.Parse(end_date.ToString());
                        eventList = eManager.GetAllEventByTimePeriod(startDate, endDate);
                    }
                    else if (start_date != null && end_date == null)
                    {
                        DateTime startDate = DateTime.Parse(start_date.ToString());

                        eventList = eManager.GetAllEventByTimePeriod(startDate);
                    }
                    else
                    {
                        eventList = eManager.GetAllBookingEvents().ToList();
                    }


                    if (byUser == true)
                    {

                        User user = subManager.Subjects.Where(a => a.Name == HttpContext.User.Identity.Name).FirstOrDefault() as User;

                        foreach (BookingEvent e in eventList)
                        {
                            List<Schedule> schedules = schManager.GetAllSchedulesByEvent(e.Id);
                            var s = FilterSchedulesByUser(schedules, user.Id).Concat(FilterSchedulesForUser(schedules, user.Id)).ToList();
                            if (s.Count > 0)
                                eventListTmp.Add(e);
                        }

                        eventList = eventListTmp;
                    }
                }
                else
                {

                    List<Schedule> scheduleList = new List<Schedule>();
                    scheduleList = Session["FilterSchedules"] as List<Schedule>;
  
                    // filter list by user
                    if (byUser == true)
                    {
                        User user = subManager.Subjects.Where(a => a.Name == HttpContext.User.Identity.Name).FirstOrDefault() as User;

                        var scheduleListNew = FilterSchedulesByUser(scheduleList, user.Id).Concat(FilterSchedulesForUser(scheduleList, user.Id)).ToList();
                        scheduleList = scheduleListNew.Distinct().ToList();
                    }

                    // filter by start and end date
                    if (start_date != null && end_date != null)
                    {
                        DateTime startDate = DateTime.Parse(start_date.ToString());
                        DateTime endDate = DateTime.Parse(end_date.ToString());
                        scheduleList = scheduleList.Where(a => ((DateTime)a.StartDate >= startDate && (DateTime)a.EndDate <= endDate) || ((DateTime)a.EndDate >= startDate && (DateTime)a.EndDate <= endDate) || (DateTime)a.StartDate <= startDate && (DateTime)a.EndDate >= endDate).ToList();
                    }
                    else if (start_date != null && end_date == null)
                    {
                        DateTime startDate = DateTime.Parse(start_date.ToString());

                        scheduleList = scheduleList.Where(a => ((DateTime)a.EndDate >= startDate)).ToList();
                    }

                    foreach (Schedule s in scheduleList)
                    {
                        if (!eventList.Select(a => a.Id).ToList().Contains(s.BookingEvent.Id))
                        {
                            eventList.Add(eManager.GetBookingEventById(s.BookingEvent.Id));
                        }
                    }

                }
                return eventList;
            }
        }

        private List<Schedule> GetAllScheduleFiltered(bool byUser, string start_date = null, string end_date = null)
        {

            List<Schedule> scheduleList = new List<Schedule>();
            scheduleList = Session["FilterSchedules"] as List<Schedule>;
            Session["FilterSchedules"] = scheduleList; // Set again to renew Session 

            using (ScheduleManager schManager = new ScheduleManager())
            using (SubjectManager subManager = new SubjectManager())
            {
                if (scheduleList == null)
                {
                    scheduleList = schManager.GetAllSchedules().ToList();
                    // Set filtered list to full list
                    Session["FilterSchedules"] = scheduleList;
                }

                // filter by start and end date
                if (start_date != null && end_date != null)
                {
                    DateTime startDate = DateTime.Parse(start_date.ToString());
                    DateTime endDate = DateTime.Parse(end_date.ToString());

                    scheduleList = scheduleList.Where(a => ((DateTime)a.StartDate >= startDate && (DateTime)a.EndDate <= endDate) || ((DateTime)a.EndDate >= startDate && (DateTime)a.EndDate <= endDate) || (DateTime)a.StartDate <= startDate && (DateTime)a.EndDate >= endDate).ToList();
                }
                else if (start_date != null && end_date == null)
                {
                    DateTime startDate = DateTime.Parse(start_date.ToString());

                    scheduleList = scheduleList.Where(a => ((DateTime)a.EndDate >= startDate)).ToList();
                }

                if (byUser == true)
                {
                    User user = subManager.Subjects.Where(a => a.Name == HttpContext.User.Identity.Name).FirstOrDefault() as User;

                    var scheduleListNew = FilterSchedulesByUser(scheduleList, user.Id).Concat(FilterSchedulesForUser(scheduleList, user.Id)).ToList();
                    scheduleList = scheduleListNew.Distinct().ToList();
                }

                return scheduleList;
            }
            
        }

        #endregion


    }
}