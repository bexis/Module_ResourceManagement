using BExIS.Modules.RBM.UI.Helper;
using BExIS.Rbm.Entities.Booking;
using BExIS.Rbm.Entities.Resource;
using BExIS.Rbm.Entities.ResourceStructure;
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
using System.Web;
using System.Web.Mvc;
using System.Web.Services;
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

        [WebMethod]
        public JsonResult GetAllEvents(object st, object ed)
        {
            DateTime startDate = DateTime.Parse(st.ToString());
            DateTime endDate = DateTime.Parse(ed.ToString());

            List<BookingEvent> eventList = new List<BookingEvent>();
            if (Session["FilterSchedules"] == null)
            {
                EventManager eManager = new EventManager();
                eventList = eManager.GetAllEventByTimePeriod(startDate, endDate);
            }
            else
            {
                EventManager eManager = new EventManager();
                List<Schedule> scheduleList = new List<Schedule>();
                scheduleList = Session["FilterSchedules"] as List<Schedule>;
                foreach (Schedule s in scheduleList)
                {
                    if (!eventList.Select(a => a.Id).ToList().Contains(s.Event.Id))
                        eventList.Add(eManager.GetEventById(s.Event.Id));
                }

            }

            List<object> eventObjectList = new List<object>();
            foreach (BookingEvent e in eventList)
            {
                eventObjectList.Add(new object[] { JsonHelper.JsonSerializer<CalendarItemsModel>(new CalendarItemsModel(e)) });
            }

            return Json(eventObjectList.ToArray(), JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public JsonResult GetAllSchedules()
        {
            List<Schedule> scheduleList = new List<Schedule>();
            scheduleList = Session["FilterSchedules"] as List<Schedule>;

            if (scheduleList == null)
            {
                ScheduleManager schManager = new ScheduleManager();
                scheduleList = schManager.GetAllSchedules().ToList();
            }

            List<object> scheduleObjectList = new List<object>();
            foreach (Schedule s in scheduleList)
            {
                scheduleObjectList.Add(new object[] { JsonHelper.JsonSerializer<CalendarItemsModel>(new CalendarItemsModel(s.Resource, s.StartDate, s.EndDate, s.Event.Id)) });
            }

            Session["FilterSchedules"] = null;
            return Json(scheduleObjectList.ToArray(), JsonRequestBehavior.AllowGet);
        }


        public JsonResult GetEventsByUser()
        {
            List<BookingEvent> eventList = new List<BookingEvent>();
            SubjectManager subManager = new SubjectManager();

            User user = subManager.Subjects.Where(a => a.Name == HttpContext.User.Identity.Name).FirstOrDefault() as User;

            if (Session["FilterSchedules"] == null)
            {
                EventManager eManager = new EventManager();
                ScheduleManager schManager = new ScheduleManager();

                List<BookingEvent> temp = eManager.GetAllEvents().ToList();
                foreach (BookingEvent e in temp)
                {
                    List<Schedule> schedules = schManager.GetAllSchedulesByEvent(e.Id);
                    var s = schedules.Where(a => a.ByPerson.Person.Id == user.Id).ToList();
                    if (s.Count > 0)
                        eventList.Add(e);
                }
            }

            List<object> eventObjectList = new List<object>();
            foreach (BookingEvent e in eventList)
            {
                eventObjectList.Add(new object[] { JsonHelper.JsonSerializer<CalendarItemsModel>(new CalendarItemsModel(e)) });
            }

            return Json(eventObjectList.ToArray(), JsonRequestBehavior.AllowGet);

        }

        public JsonResult GetSchedulesByUser()
        {
            List<Schedule> scheduleList = new List<Schedule>();
            SubjectManager subManager = new SubjectManager();

            User user = subManager.Subjects.Where(a => a.Name == HttpContext.User.Identity.Name).FirstOrDefault() as User;

            if (Session["FilterSchedules"] == null)
            {
                ScheduleManager schManager = new ScheduleManager();
                List<Schedule> temp = schManager.GetAllSchedules().ToList();

                foreach (Schedule s in temp)
                {
                    if (s.ByPerson.Person.Id == user.Id)
                        scheduleList.Add(s);
                }
            }

            List<object> scheduleObjectList = new List<object>();
            foreach (Schedule s in scheduleList)
            {
                scheduleObjectList.Add(new object[] { JsonHelper.JsonSerializer<CalendarItemsModel>(new CalendarItemsModel(s.Resource, s.StartDate, s.EndDate, s.Event.Id)) });
            }

            Session["FilterSchedules"] = null;
            return Json(scheduleObjectList.ToArray(), JsonRequestBehavior.AllowGet);

        }

        //use if resource not avalible in event creation
        public JsonResult GetAllSchedulesFromResource(string resourceId)
        {
            ScheduleManager schManager = new ScheduleManager();
            List<Schedule> allSchedules = schManager.GetAllSchedulesByResource(Convert.ToInt64(resourceId));

            List<object> scheduleList = new List<object>();
            foreach (Schedule s in allSchedules)
            {
                scheduleList.Add(new object[] { JsonHelper.JsonSerializer<CalendarItemsModel>(new CalendarItemsModel(s.Resource, s.StartDate, s.EndDate, s.Event.Id)) });
            }

            return Json(scheduleList.ToArray(), JsonRequestBehavior.AllowGet);
        }

        public ActionResult TreeFilterSchedules()
        {
            ResourceFilterModel model = new ResourceFilterModel();
            SingleResourceManager rManager = new SingleResourceManager();
            List<SingleResource> singleResources = rManager.GetAllResources().ToList();

            List<ResourceModel> resources = new List<ResourceModel>();
            singleResources.ForEach(r => resources.Add(new ResourceModel(r)));

            foreach (ResourceModel r in resources)
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
                //results after filtering
                List<Schedule> resultScheduleList = new List<Schedule>();

                //result resource list after filtering
                List<SingleResource> resultResourceList = new List<SingleResource>();

                //Filter is this format: AttrID_DomainItem, AttrID_Domain
                List<string> items = selectedItems.Split(',').ToList();

                //get all schedules
                ScheduleManager schManager = new ScheduleManager();
                List<Schedule> allSchedules = schManager.GetAllSchedules().ToList();

                //get all scheduled resources 
                List<SingleResource> scheduledResources = allSchedules.Select(r => r.Resource).ToList();

                List<ResourceFilterHelper.FilterTreeItem> filterList = new List<ResourceFilterHelper.FilterTreeItem>();
                //split Id and DomainItem and add it to a FilterItem list
                foreach (string item in items)
                {
                    //index 0 = attrbute id, index1 domainvalue
                    List<string> i = item.Split('_').ToList();
                    ResourceFilterHelper.FilterTreeItem filterItem = new ResourceFilterHelper.FilterTreeItem();
                    filterItem.Id = Convert.ToInt64(i[0]);
                    filterItem.Value = i[1].ToString();
                    filterList.Add(filterItem);
                }

                List<ResourceAttributeValueModel> treeDomainList = new List<ResourceAttributeValueModel>();

                //Create for each Resource TreeDomainModel witch includes all Attribute Ids and all values
                foreach (SingleResource r in scheduledResources)
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
                        //if (selectedResources.Count > 0)
                        //{
                        //    if (selectedResources.Where(a => a.Id == m.Resource.Id).Count() > 0)
                        //    {
                        //        ResourceManagerModel rem = new ResourceManagerModel(m.Resource);
                        //        //rem.InEvent = true;
                        //        //TempSchedule tempS = selectedResources.Where(a => a.Id == m.Resource.Id).FirstOrDefault();
                        //        //rem.StartDate = tempS.StartDate;
                        //        //rem.EndDate = tempS.EndDate;
                        //        resultList.Add(rem);
                        //    }
                        //}
                        //else
                        resultResourceList.Add(m.Resource);
                    }
                }

                //create schedule resource list with selected resources
                foreach (SingleResource sr in resultResourceList)
                {
                    if (allSchedules.Any(s => s.Resource == sr))
                    {
                        resultScheduleList.Add(allSchedules.Where(r => r.Resource == sr).FirstOrDefault());
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
            EventManager eManager = new EventManager();
            ScheduleManager schManager = new ScheduleManager();
            SubjectManager subManager = new SubjectManager();

            List<EventModel> model = new List<EventModel>();

            List<BookingEvent> allEvents = new List<BookingEvent>();

            if (Session["FilterSchedules"] == null)
                allEvents = eManager.GetAllEvents().ToList();
            else
            {
                List<Schedule> scheduleList = new List<Schedule>();
                scheduleList = Session["FilterSchedules"] as List<Schedule>;
                foreach (Schedule s in scheduleList)
                {
                    allEvents.Add(eManager.GetEventById(s.Event.Id));
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
                        model.Add(new EventModel(e));
                }
            }
            else
                allEvents.ForEach(r => model.Add(new EventModel(r)));

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

            if (bool.Parse(myBookings))
            {
                User user = subManager.Subjects.Where(a => a.Name == HttpContext.User.Identity.Name).FirstOrDefault() as User;
                foreach (Schedule s in allSchedules)
                {
                    //var s = schedules.Where(a => a.ByPerson.Person.Id == user.Id).ToList();
                    if (s.ByPerson.Person.Id == user.Id)
                        model.Add(new ScheduleListModel(s));
                }

            }
            else
                allSchedules.ForEach(r => model.Add(new ScheduleListModel(r)));


            return PartialView("_listSchedules", model);
        }

        #endregion
    }
}