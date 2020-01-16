using BExIS.Modules.RBM.UI.Helper;
using BExIS.Rbm.Entities.Booking;
using BExIS.Rbm.Entities.BookingManagementTime;
using BExIS.Rbm.Entities.Resource;
using BExIS.Rbm.Entities.ResourceStructure;
using BExIS.Rbm.Entities.Users;
using BExIS.Rbm.Services.Booking;
using BExIS.Rbm.Services.Resource;
using BExIS.Security.Entities.Subjects;
using BExIS.Security.Services.Subjects;
using BExIS.Web.Shell.Areas.RBM.Helpers;
using BExIS.Web.Shell.Areas.RBM.Models.Booking;
using BExIS.Web.Shell.Areas.RBM.Models.Resource;
using BExIS.Web.Shell.Areas.RBM.Models.ResourceStructure;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
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
            ViewBag.Title = PresentationModel.GetViewTitleForTenant("Schedule Calendar", this.Session.GetTenant());
            Session["Filter"] = null;
            Session["FilterSchedules"] = null;
            return View("Calendar");
        }

        #region Schedule Calendar

        // Get all events filtered by date and optional also by "my bookings"

        [WebMethod]
        public JsonResult GetAllEvents(object st, object ed, bool byUser = false)
        {
            DateTime startDate = DateTime.Parse(st.ToString());
            DateTime endDate = DateTime.Parse(ed.ToString());

            List<BookingEvent> eventList = new List<BookingEvent>();
            List<BookingEvent> eventListTmp = new List<BookingEvent>();
            if (Session["FilterSchedules"] == null)
            {
                BookingEventManager eManager = new BookingEventManager();
                eventList = eManager.GetAllEventByTimePeriod(startDate, endDate);

                if (byUser == true)
                {
                    ScheduleManager schManager = new ScheduleManager();

                    SubjectManager subManager = new SubjectManager();
                    User user = subManager.Subjects.Where(a => a.Name == HttpContext.User.Identity.Name).FirstOrDefault() as User;

                    foreach (BookingEvent e in eventList)
                    {
                        List<Schedule> schedules = schManager.GetAllSchedulesByEvent(e.Id);
                        var s = schedules.Where(a => a.ByPerson.Person.Id == user.Id).ToList();
                        if (s.Count > 0)
                            eventListTmp.Add(e);
                    }

                    eventList = eventListTmp;
                }
            }
            else
            {
                BookingEventManager eManager = new BookingEventManager();
                List<Schedule> scheduleList = new List<Schedule>();
                scheduleList = Session["FilterSchedules"] as List<Schedule>;

                // filter list by user
                if (byUser == true)
                {
                    SubjectManager subManager = new SubjectManager();
                    User user = subManager.Subjects.Where(a => a.Name == HttpContext.User.Identity.Name).FirstOrDefault() as User;

                    scheduleList = scheduleList.Where(a => a.ByPerson.Person.Id == user.Id).ToList();  
                }

                scheduleList = scheduleList.Where(a => ((DateTime)a.StartDate >= startDate && (DateTime)a.EndDate <= endDate) || ((DateTime)a.EndDate >= startDate && (DateTime)a.EndDate <= endDate) || (DateTime)a.StartDate <= startDate && (DateTime)a.EndDate >= endDate).ToList();
                foreach (Schedule s in scheduleList)
                {
                    if (!eventList.Select(a => a.Id).ToList().Contains(s.BookingEvent.Id))
 
                    {
                        eventList.Add(eManager.GetBookingEventById(s.BookingEvent.Id));
                    }

                }

            }

            List<object> eventObjectList = new List<object>();
            foreach (BookingEvent e in eventList)
            {
                eventObjectList.Add(new object[] { JsonHelper.JsonSerializer<CalendarItemsModel>(new CalendarItemsModel(e)) });
            }

            return Json(eventObjectList.ToArray(), JsonRequestBehavior.AllowGet);
        }

        // Get all schedules filtered by date and optional by user
        [WebMethod]
        public JsonResult GetAllSchedules(object st, object ed, bool byUser = false)
        {
            DateTime startDate = DateTime.Parse(st.ToString());
            DateTime endDate = DateTime.Parse(ed.ToString());

            List<Schedule> scheduleList = new List<Schedule>();
            scheduleList = Session["FilterSchedules"] as List<Schedule>;

            if (scheduleList == null)
            {
                ScheduleManager schManager = new ScheduleManager();
                scheduleList = schManager.GetAllSchedules().ToList();
            }

            scheduleList = scheduleList.Where(a => ((DateTime)a.StartDate >= startDate && (DateTime)a.EndDate <= endDate) || ((DateTime)a.EndDate >= startDate && (DateTime)a.EndDate <= endDate) || (DateTime)a.StartDate <= startDate && (DateTime)a.EndDate >= endDate).ToList();

            if (byUser == true)
            {
                SubjectManager subManager = new SubjectManager();
                User user = subManager.Subjects.Where(a => a.Name == HttpContext.User.Identity.Name).FirstOrDefault() as User;

                scheduleList = scheduleList.Where(a => a.ByPerson.Person.Id == user.Id).ToList();
            }

            List<object> scheduleObjectList = new List<object>();
            foreach (Schedule s in scheduleList)
            {
                using (IUnitOfWork uow = this.GetUnitOfWork())
                {
                    if (s.Resource == null) break;

                    var duration = uow.GetReadOnlyRepository<TimeDuration>().Get(s.Resource.Duration.Id);

                    if (duration == null) break;
                    var timeUnit = duration.TimeUnit;

                    scheduleObjectList.Add(new object[] { JsonHelper.JsonSerializer<CalendarItemsModel>(new CalendarItemsModel(s.Resource.Name, s.Resource.Color, timeUnit, s.StartDate, s.EndDate, s.BookingEvent.Id)) });
                }
            }

            return Json(scheduleObjectList.ToArray(), JsonRequestBehavior.AllowGet);
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
                //results after filtering
                List<Schedule> resultScheduleList = new List<Schedule>();

                //result resource list after filtering
                List<long> resultResourceIDList = new List<long>();

                //Filter is this format: AttrID_DomainItem, AttrID_Domain
                List<string> items = selectedItems.Split(',').ToList();

                //get all schedules
                ScheduleManager schManager = new ScheduleManager();
                List<Schedule> allSchedules = schManager.GetAllSchedules().ToList();

                //get all scheduled resources
                ResourceManager srManager = new ResourceManager();
                List<SingleResource> resourcesList = srManager.GetAllResources().ToList();

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

                //Create for each Resource TreeDomainModel witch includes all Attribute Ids and all values
                foreach (SingleResource r in resourcesList)
                {
                    ResourceAttributeValueModel treeDomainModel = new ResourceAttributeValueModel(r);
                    treeDomainList.Add(treeDomainModel);
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

        #endregion

        #region Schedule List

        public ActionResult GetEventsAsList(string myBookings)
        {
            BookingEventManager eManager = new BookingEventManager();
            ScheduleManager schManager = new ScheduleManager();
            SubjectManager subManager = new SubjectManager();

            List<BookingEventModel> model = new List<BookingEventModel>();

            List<BookingEvent> allEvents = new List<BookingEvent>();

            if (Session["FilterSchedules"] == null)
                allEvents = eManager.GetAllBookingEvents().ToList();
            else
            {
                List<Schedule> scheduleList = new List<Schedule>();
                scheduleList = Session["FilterSchedules"] as List<Schedule>;
                foreach (Schedule s in scheduleList)
                {
                    allEvents.Add(eManager.GetBookingEventById(s.BookingEvent.Id));
                }
            }

            if (bool.Parse(myBookings))
            {
                User user = subManager.Subjects.Where(a => a.Name == HttpContext.User.Identity.Name).FirstOrDefault() as User;

                foreach (BookingEvent e in allEvents)
                {
                    List<Schedule> schedules = schManager.GetAllSchedulesByEvent(e.Id);
                    var s = schedules.Where(a => a.ByPerson.Person.Id == user.Id).ToList();
                    if (s.Count > 0)
                        model.Add(new BookingEventModel(e));
                }
            }
            else
                allEvents.ForEach(r => model.Add(new BookingEventModel(r)));

            // Remove dublicate booking events in resources list (without filter not dublicates, but with)
            model = model.GroupBy(x => x.Id).Select(x => x.First()).ToList();

            foreach (BookingEventModel m in model)
            {
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
            }

            //model = model.OrderByDescending(a => a.startDate).ToList();

            return PartialView("_listEvents", model);
        }


        public ActionResult GetSchedulesAsList(string myBookings)
        {

            ScheduleManager sManager = new ScheduleManager();
            SubjectManager subManager = new SubjectManager();
            List<Schedule> allSchedules = new List<Rbm.Entities.Booking.Schedule>();

            if (Session["FilterSchedules"] == null)
                allSchedules = sManager.GetAllSchedules().ToList();
            else
                allSchedules = Session["FilterSchedules"] as List<Schedule>;

            List<ScheduleListModel> model = new List<ScheduleListModel>();

            User user = subManager.Subjects.Where(a => a.Name == HttpContext.User.Identity.Name).FirstOrDefault() as User;


            // filter bookings when only my bookings should be loaded
            List<Schedule> schedules = new List<Schedule>();

                if(bool.Parse(myBookings))
                    schedules = allSchedules.Where(s => s.ByPerson.Person.Id.Equals(user.Id)).ToList(); 
                else
                    schedules = allSchedules;

            using (var uow = this.GetUnitOfWork())
            {
                foreach (Schedule s in schedules)
                {
                    var booking = uow.GetReadOnlyRepository<BookingEvent>().Get(s.BookingEvent.Id);
                    var resource = uow.GetReadOnlyRepository<Resource>().Get(s.Resource.Id);
                    var forperson = uow.GetReadOnlyRepository<Person>().Get(s.ForPerson.Id);
                   

                    model.Add(new ScheduleListModel(
                        booking.Id,
                        booking.Name,
                        booking.Description,
                        resource.Name,
                        s.StartDate,
                        s.EndDate,
                        s.Quantity,
                        forperson,
                        s.Activities
                        ));
                }
            }

            return PartialView("_listSchedules", model);
        }


        #endregion
    }
}