using BExIS.Dlm.Entities.DataStructure;
using BExIS.Rbm.Entities.Booking;
using BExIS.Rbm.Entities.Resource;
using BExIS.Rbm.Entities.ResourceStructure;
using BExIS.Rbm.Entities.Users;
using BExIS.Rbm.Services.Booking;
using BExIS.Rbm.Services.Resource;
using BExIS.Rbm.Services.ResourceStructure;
using BExIS.Rbm.Services.Users;
using BExIS.Security.Entities.Objects;
using BExIS.Security.Entities.Subjects;
using BExIS.Security.Services.Authorization;
using BExIS.Security.Services.Subjects;
using BExIS.Web.Shell.Areas.RBM.Helpers;
using BExIS.Web.Shell.Areas.RBM.Models.Booking;
using BExIS.Web.Shell.Areas.RBM.Models.Resource;
using BExIS.Web.Shell.Areas.RBM.Models.ResourceStructure;
using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.Mvc;
//using System.Web.Script.Serialization;
using Telerik.Web.Mvc;
using Vaiona.Web.Mvc.Models;
using System.Text;
using BExIS.Rbm.Entities.ResourceConstraint;
using BExIS.Rbm.Entities.BookingManagementTime;
using System.Linq.Expressions;
using System.Web.Routing;
using Vaiona.Web.Extensions;
using System.Web.Services;
using System.Data;
using BExIS.Security.Entities.Authorization;
using BExIS.Dlm.Entities.Party;
using BExIS.Dlm.Services.Party;
using BExIS.Modules.RBM.UI.Helper;
using BExIS.Security.Services.Objects;
using System.Configuration;
using System.Web.Configuration;
using Vaiona.Utils.Cfg;
using Vaiona.Persistence.Api;

namespace BExIS.Modules.RBM.UI.Controllers
{
    public class ScheduleController : Controller
    {
        #region Create Event -- Filter Resources

        //internal usage to store seleced resources schedules in a session
        private struct TempSchedule
        {

            public long Id;

            public DateTime StartDate;

            public DateTime EndDate;
        }

        public ActionResult OnChangeFilter(string selectedItems)
        {
            //set result set for tree Filter null
            Session["TreeFilterResults"] = null;

            //Filter is this format: AttrID_DomainItem, AttrID_Domain
            List<string> items = selectedItems.Split(',').ToList();

            List<ResourceAttributeValueModel> treeDomainList = new List<ResourceAttributeValueModel>();
            List<SelectResourceForEventModel> resultFromTreeFilter = new List<SelectResourceForEventModel>();

            using (ResourceManager rManager = new ResourceManager())
            using (ResourceStructureAttributeManager rsaManager = new ResourceStructureAttributeManager())
            {

                //List<SingleResource> resources = rManager.GetAllResources().ToList();
                //get all scheduled resources
                List<SingleResource> resources;
                if (Session["resourcesList"] == null)
                {
                    resources = rManager.GetAllResources().ToList();
                    Session["resourcesList"] = resources;
                }
                else
                {
                    resources = (List<SingleResource>)Session["resourcesList"];
                }

                //Create for each Resource TreeDomainModel witch includes all Attribute Ids and all values
                if (Session["treeDomainModel"] == null)
                {
                    //Create for each Resource TreeDomainModel witch includes all Attribute Ids and all values
                    foreach (SingleResource r in resources)
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

                //Session["Filter"] = filterList;

                //Dictionary to save every Filter (domain items) to one attr
                Dictionary<long, List<string>> filterDic = ResourceFilterHelper.GetFilterDic(filterList);

                List<ResourceAttributeValueModel> temp = new List<ResourceAttributeValueModel>();

                //check for every TreeDomainModel (resource) if fits the filter
                foreach (ResourceAttributeValueModel m in treeDomainList)
                {
                    if (ResourceFilterHelper.CheckTreeDomainModel(m, filterDic))
                    {
                        resultFromTreeFilter.Add(new SelectResourceForEventModel(m.Resource));
                    }
                }

                Session["TreeFilterResults"] = resultFromTreeFilter;

                return PartialView("_gridResources", resultFromTreeFilter);
            }
        }

        #endregion

        #region Create Event -- Select Resource(s) for event and put to resource cart

        //Treeview with Attributes and there Domain Constraints and other filter options
        public ActionResult ResourceFilter()
        {
            ResourceFilterModel model = new ResourceFilterModel();
            //if event creation starting point from ´date(time) in calender set date to clicked date

            using (ResourceStructureManager rsManager = new ResourceStructureManager())
            {
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

                return PartialView("_filterResources", model);
            }
        }

        //fill grid view in select resources with resources
        public ActionResult LoadResources()
        {
            List<SelectResourceForEventModel> allResourceList = new List<SelectResourceForEventModel>();
            List<SelectResourceForEventModel> results = new List<SelectResourceForEventModel>();

            using (ResourceManager srManager = new ResourceManager())
            {
                List<SingleResource> singelResources = new List<SingleResource>();
                singelResources = srManager.GetAllResources().ToList();
                singelResources.ForEach(r => allResourceList.Add(new SelectResourceForEventModel(r)));

               results = allResourceList;
               Session["TreeFilterResults"] = results;
                 
                return PartialView("_gridResources", results);
            }

        }

        public ActionResult SelectedResources()
        {
            List<ResourceCart> model = (List<ResourceCart>)Session["ResourceCart"];
            if (model == null)
                model = new List<ResourceCart>();

            return PartialView("_cartResources", model);
        }

        //Temp adding resource to event, schedules are not stored in this step


        public ActionResult AddResourceToCart(string id)
        {
            if (id == "")
            {
                ModelState.AddModelError("ResourceList", "You must select an resource to add it to the event.");
            }

            if (ModelState.IsValid)
            {
                List<ResourceCart> model = (List<ResourceCart>)Session["ResourceCart"];

                if (model == null)
                    model = new List<ResourceCart>();

                using (var srManager = new ResourceManager())
                using (var pManager = new PersonManager())
                using (UserManager userManager = new UserManager())
                {
                    int index = 0;
                    if (model.Count() == 0)
                        index = 1;
                    else
                    {
                        BookingEventModel eventModel = (BookingEventModel)Session["Event"];
                        if (eventModel != null)
                        {
                            if (eventModel.Schedules.Count > 0 && model.Count > 0)
                            {
                                index = (eventModel.Schedules.Select(e => e.Index).Max() + 1) + model.Select(e => e.Index).Max() + 1;
                            }
                            else if (eventModel.Schedules.Count > 0)
                                index = eventModel.Schedules.Select(e => e.Index).Max() + 1;
                            else
                                index = model.Select(e => e.Index).Max() + 1;
                        }
                        else
                            index = model.Select(e => e.Index).Max() + 1;
                    }

                    SingleResource resource = srManager.GetResourceById(long.Parse(id));
                    ResourceCart cartItem = new ResourceCart();

                    cartItem.Id = resource.Id;

                    //ToDo: User or Default
                    var user = UserHelper.GetUser(HttpContext.User.Identity.Name);
                    cartItem.ByPersonName = user.DisplayName;
                    cartItem.ByPersonUserId = user.Id;

                    cartItem.Name = resource.Name;
                    cartItem.Index = index;
                    cartItem.NewInCart = true;

                    model.Add(cartItem);
                }

                Session["ResourceCart"] = model;

                return PartialView("_cartResources", model);
            }
            else
            {
                return RedirectToAction("LoadResources");
            }

        }

        public ActionResult RemoveResourceFromCart(string index)
        {
            List<ResourceCart> model = (List<ResourceCart>)Session["ResourceCart"];
            BookingEventModel eventModel = (BookingEventModel)Session["Event"];
            var i = model.Where(a=>a.Index == Convert.ToInt32(index)).FirstOrDefault().Index;
           
            //if event != null remov resource here, too
            if (eventModel != null)
            {
                //var ie = eventModel.Schedules.FindIndex(p => p.Index == Convert.ToInt64(index));
                if (eventModel.Schedules.Where(a=>a.Index == i).Count() == 1)
                {
                    using (ScheduleManager scheduleManager = new ScheduleManager())
                    {
                        var scheduleModel = eventModel.Schedules.Where(a => a.Index == i).FirstOrDefault();
                        if (scheduleModel != null)
                        {
                            var scheduleDB = scheduleManager.GetScheduleById(scheduleModel.ScheduleId);
                            if(scheduleDB != null)
                                scheduleManager.DeleteSchedule(scheduleDB);
                        }
                    }

                    eventModel.Schedules.Remove(eventModel.Schedules.Where(a => a.Index == i).FirstOrDefault());
                    Session["Event"] = eventModel;
                }
            }
           
            model.Remove(model.Where(a => a.Index == i).FirstOrDefault());
            Session["ResourceCart"] = model;

            return PartialView("_cartResources", model);
        }

        public ActionResult RemoveAll()
        {
            Session["ResourceCart"] = new List<ResourceCart>();

            BookingEventModel eventModel = (BookingEventModel)Session["Event"];
            if (eventModel.Schedules.Count() > 0)
            {
                using (ScheduleManager scheduleManager = new ScheduleManager())
                {
                    foreach (var schedule in eventModel.Schedules)
                    {
                        var scheduleDB = scheduleManager.GetScheduleById(schedule.ScheduleId);
                        if(scheduleDB != null)
                            scheduleManager.DeleteSchedule(scheduleDB);
                    }
                }
                eventModel.Schedules.Clear();
                Session["Event"] = eventModel;
            }

            return PartialView("_cartResources", new List<ResourceCart>());
        }

        //für eventview später wiederverenden
        //Load schedule to event, if a new event the list is empty
        public ActionResult LoadSchedules()
        {
            BookingEventModel model = (BookingEventModel)Session["Event"];
            //int eventId;
            //bool parsed = Int32.TryParse(id, out eventId);
            //if (parsed)
            //{
            //    int index = 0;
            //    ScheduleManager sManager = new ScheduleManager();
            //    List<Schedule> schedules = new List<Schedule>();
            //    schedules = sManager.GetAllSchedulesByEvent(Convert.ToInt64(id));
            //    foreach (Schedule s in schedules)
            //    {
            //        ScheduleEventModel seModel = new ScheduleEventModel(s);
            //        index++;
            //        seModel.Index = index;
            //        model.Schedules.Add(seModel);
            //    }

            //    //schedules.ForEach(r => model.Add(new ScheduleEventModel(r)));
            //    Session["EventResources"] = model;
            //}
            //else if (id == "reload")
            //{
            //    EventModel eModel = (EventModel)Session["Event"];
            //   // model = eModel.Schedules;
            //}
            //else
            //{
            //    Session["EventResources"] = null;
            //}


            return PartialView("EditEvent", model);
        }



        #endregion

        #region Create/Edit Event
        public ActionResult Create()
        {
            ViewBag.Title = PresentationModel.GetViewTitleForTenant("Create Event", this.Session.GetTenant());
            Session["ResourceCart"] = null;
            Session["Event"] = null;
            Session["FilterResults"] = null;
            
            ResourceFilterModel model = new ResourceFilterModel();
           
            return View("SelectResources");
        }

        public ActionResult CreateEvent()
        {
            using (var partyManager = new PartyManager())
            using (var rManager = new ResourceManager())
            using (UserManager userManager = new UserManager())
            {
                List<ResourceCart> cart = (List<ResourceCart>)Session["ResourceCart"];
                if (cart == null)
                    ModelState.AddModelError("CartEmpty", "Please add a resource to the cart.");

                if (ModelState.IsValid)
                {
                    BookingEventModel model = (BookingEventModel)Session["Event"];

                    if (model == null)
                    {
                        model = new BookingEventModel(cart);
                        cart.ForEach(x => x.NewInCart = false);
                    }
                    else
                    {
                        List<ScheduleEventModel> tempList = new List<ScheduleEventModel>();
                        tempList.AddRange(model.Schedules);
                        foreach (ResourceCart rc in cart)
                        {
                            if (rc.NewInCart)
                            {
                                rc.NewInCart = false;
                                SingleResource resource = rManager.GetResourceById(rc.Id);
                                ScheduleEventModel s = new ScheduleEventModel(resource);

                                s.ScheduleDurationModel.StartDate = DateTime.Now;
                                s.ScheduleDurationModel.EndDate = DateTime.Now;

                                s.EditMode = true;
                                s.EditAccess = true;

                                if (resource.Quantity > 0)
                                    s.ScheduleQuantity = 1;

                                s.ScheduleDurationModel.Index = rc.Index;
                                s.ScheduleDurationModel.EventId = model.Id;

                                s.ResourceQuantity = resource.Quantity;
                                s.ByPerson = rc.ByPersonName;

                                //add as default resvered by user as reserved for user

                                var userTask = userManager.FindByIdAsync(rc.ByPersonUserId);
                                userTask.Wait();
                                var user = userTask.Result;

                                PersonInSchedule byPerson = new PersonInSchedule(0, user, true);

                                //set contact                       
                                s.Contact = byPerson;
                                s.ContactName = byPerson.UserFullName;
                                s.ForPersons.Add(byPerson);

                                s.Index = rc.Index;
                                //s.Index = model.Schedules.Count() + 2;
                                model.Schedules.Add(s);
                            }
                        }

                        //check for deleted resource in cart
                        var diff = tempList.Select(a => a.Index).ToList().Except(cart.Select(c => c.Index).ToList());
                        foreach (var i in diff)
                        {
                            ScheduleEventModel temp = tempList.Where(a => a.Index == i).FirstOrDefault();
                            model.Schedules.Remove(temp);
                        }

                    }

                    model.EditMode = true;
                    Session["Event"] = model;
                    Session["ResourceCart"] = cart;

                    return View("EditEvent", model);
                }
                else
                {
                    return View("SelectResources");
                }
            }
        }

        public ActionResult OnChangeEventItem(string value, string index, string element)
        {
            BookingEventModel model = (BookingEventModel)Session["Event"];
            var i = 0;
            if (model != null)
            {
                ScheduleEventModel seModel = new ScheduleEventModel();
                switch (element)
                {
                    case "Name":
                        //name validierung rein
                        model.Name = value;
                        Session["Event"] = model;
                        break;
                    case "Description":
                        model.Description = value;
                        Session["Event"] = model;
                        break;
                    case "Start":
                        i = model.Schedules.FindIndex(p => p.Index == int.Parse(index));
                        if (!String.IsNullOrEmpty(value))
                        {
                            model.Schedules[i].ScheduleDurationModel.StartDate = DateTime.ParseExact(value, "dd.MM.yyyy", null);
                            model.Schedules[i].ScheduleDurationModel.EndDate = TimeHelper.GetEndDateOfDuration(model.Schedules[i].ScheduleDurationModel.DurationValue, model.Schedules[i].ScheduleDurationModel.TimeUnit, model.Schedules[i].ScheduleDurationModel.StartDate);
                        }
                        else
                            model.Schedules[i].ScheduleDurationModel.EndDate = DateTime.MinValue;

                        Session["Event"] = model;
                        return PartialView("_showTimePeriod", model.Schedules[i].ScheduleDurationModel);

                    case "End":
                        i = model.Schedules.FindIndex(p => p.Index == int.Parse(index));
                        if (!String.IsNullOrEmpty(value))
                            model.Schedules[i].ScheduleDurationModel.EndDate = DateTime.ParseExact(value, "dd.MM.yyyy", null);
                        else
                            model.Schedules[i].ScheduleDurationModel.EndDate = DateTime.MinValue;

                        Session["Event"] = model;
                        break;

                    case "Quantity":
                        seModel = model.Schedules.Where(a => a.Index == int.Parse(index)).FirstOrDefault();

                        int quantity;
                        if (Int32.TryParse(value, out quantity))
                        {
                            seModel.ScheduleQuantity = quantity;
                            // get index of modify schedule and update it in the session list
                            i = model.Schedules.FindIndex(p => p.Index == int.Parse(index));
                            model.Schedules[i] = seModel;
                            Session["Event"] = model;
                        }
                        break;

                }
            }

            return View("EditEvent", model);
        }

        public ActionResult SaveEvent()
        {
            BookingEventModel model = (BookingEventModel)Session["Event"];

            int countSchedulesCurrent = model.Schedules.Count();
            int countSchedulesBefor = 0;
            using (var singleResourceManager = new ResourceManager())
            {

                //Validate schedules ----------------------------------------------------------
                List<ScheduleEventModel> tempScheduleList = new List<ScheduleEventModel>();
                tempScheduleList.AddRange(model.Schedules);

                //set if one or more errors in event schedules
                bool isError = false;

                //check name
                if (model.Name == "" || model.Name == null)
                {
                    ModelState.AddModelError("Name", "A name for your reservation is required.");
                    isError = true;
                }

                //check description
                if (model.Description == "" || model.Description == null)
                {
                    ModelState.AddModelError("Description", "A description for your reservation is required.");
                    isError = true;
                }

               

                foreach (ScheduleEventModel s in tempScheduleList)
                {
                    //set if error at this schedule
                    bool sError = false;

                    //check date not empty
                    if(s.ScheduleDurationModel.StartDate == DateTime.MinValue)
                    {
                        ModelState.AddModelError("DateMismatch_" + s.Index, "Start date is missing.");
                        isError = true;
                        sError = true;
                    }
                    if (s.ScheduleDurationModel.EndDate == DateTime.MinValue)
                    {
                        ModelState.AddModelError("DateMismatch_" + s.Index, "End date is missing.");
                        isError = true;
                        sError = true;
                    }

                    //check date
                    string dateMismatch = CheckDateInconsistency(s.ScheduleDurationModel.StartDate, s.ScheduleDurationModel.EndDate);
                    if (dateMismatch != null)
                    {
                        ModelState.AddModelError("DateMismatch_" + s.Index, dateMismatch);
                        isError = true;
                        sError = true;
                    }

                    //check time period against duration
                    string duration = CheckTimePeriodAgainstDuration(s.ScheduleDurationModel);
                    if (duration != null)
                    {
                        ModelState.AddModelError("DurationError_" + s.Index, duration);
                        isError = true;
                        sError = true;
                    }

                    //check users
                    bool hasUser = HasScheduleUser(s);
                    if (!hasUser)
                    {
                        ModelState.AddModelError("UserMissing_" + s.Index, "Reserved for user(s) are missing.");
                        isError = true;
                        sError = true;
                    }

                    //check contact person
                    var contactSet = s.ForPersons.Where(a => a.IsContactPerson == true).Count()==1;
                    if (!contactSet)
                    {
                        ModelState.AddModelError("ContactMissing_" + s.Index, "Contact person is missing.");
                        isError = true;
                        sError = true;
                    }

                    //check achtivities
                    if (s.WithActivity)
                    {
                        bool hasActivity = HasScheduleActivity(s);
                        if (!hasActivity)
                        {
                            isError = true;
                            sError = true;
                            ModelState.AddModelError("ActivityMissing_" + s.Index, "Activities are missing.");
                        }
                    }
                    //check if selected number is available to the timeperiod
                    if (s.ResourceQuantity != 0)
                    {
                        bool availableQuantity = GetAvailableQuantity(s.ResourceId, s.ResourceQuantity, s.ScheduleDurationModel.StartDate, s.ScheduleDurationModel.EndDate, s.ScheduleQuantity, s.ScheduleId);

                        if (!availableQuantity)
                        {
                            isError = true;
                            sError = true;
                            ModelState.AddModelError("Quantity_" + s.Index, "Selected number not available.");
                        }
                        if (s.ScheduleQuantity == 0)
                        {
                            isError = true;
                            sError = true;
                            ModelState.AddModelError("Quantity_" + s.Index, "The Number must be greater than 0.");
                        }
                    }

                    //check if file which need to confirm attached to confirmation it set
                    if (s.ResourceHasFiles)
                    {
                        foreach (FileValueModel file in s.Files)
                        {
                            if (file.NeedConfirmation && !s.FileConfirmation)
                            {
                                isError = true;
                                sError = true;
                                ModelState.AddModelError("FileConfirmation_" + s.Index, "You must confirm that you read the file(s).");
                            }
                        }
                    }

                    //check Resource Constraints
                    SingleResource r = singleResourceManager.GetResourceById(s.ResourceId);
                    foreach (ResourceConstraint c in r.ResourceConstraints)
                    {
                        if (c is DependencyConstraint)
                        {
                            DependencyConstraint dc = (DependencyConstraint)c;
                            ResourceConstraintData data = new ResourceConstraintData();

                            List<Schedule> temp = new List<Schedule>();
                            foreach (ScheduleEventModel t in model.Schedules)
                            {
                                Schedule tempS = new Rbm.Entities.Booking.Schedule();
                                tempS.Resource = singleResourceManager.GetResourceById(t.ResourceId);
                                tempS.Quantity = t.ScheduleQuantity;
                                tempS.StartDate = t.ScheduleDurationModel.StartDate;
                                tempS.EndDate = t.ScheduleDurationModel.EndDate;
                                temp.Add(tempS);
                            }
                            data.SchedulesInEvent = temp;
                            data.TimePeriodInSchedule = new TimeInterval(s.ScheduleDurationModel.StartDate, s.ScheduleDurationModel.EndDate);

                            data.QuantityInSchedule = s.ScheduleQuantity;
                            if (!dc.IsSatisfied(data))
                            {
                                isError = true;
                                sError = true;
                                ModelState.AddModelError("DependencyConstraint_" + s.Index, dc.ErrorMessage);
                            }
                        }
                        if (c is QuantityConstraint)
                        {
                            QuantityConstraint qc = (QuantityConstraint)c;
                            ResourceConstraintData data = new ResourceConstraintData();
                            data.PersonsInSchedule = s.ForPersons.Select(a => a.UserId).ToList();
                            data.TimePeriodInSchedule = new TimeInterval(s.ScheduleDurationModel.StartDate, s.ScheduleDurationModel.EndDate);
                            data.QuantityInSchedule = s.ScheduleQuantity;
                            if (!qc.IsSatisfied(data))
                            {
                                isError = true;
                                sError = true;
                                ModelState.AddModelError("QuantityConstraint_" + s.Index, qc.ErrorMessage);
                            }
                        }
                        if (c is BlockingConstraint)
                        {
                            BlockingConstraint bc = (BlockingConstraint)c;
                            ResourceConstraintData data = new ResourceConstraintData();
                            data.PersonsInSchedule = s.ForPersons.Select(a => a.UserId).ToList();
                            data.TimePeriodInSchedule = new TimeInterval(s.ScheduleDurationModel.StartDate, s.ScheduleDurationModel.EndDate);
                            if (!bc.IsSatisfied(data))
                            {
                                isError = true;
                                sError = true;
                                ModelState.AddModelError("BlockingConstraint_" + s.Index, bc.ErrorMessage);
                            }
                        }
                    }

                    if (sError)
                        s.ScheduleError = true;
                    else
                        s.ScheduleError = false;

                    var i = model.Schedules.FindIndex(p => p.Index == s.Index);
                    model.Schedules[i] = s;
                }

                if (isError)
                {
                    model.Schedules.ToList().ForEach(c => c.Errors = true);
                }

                //End Validation ----------------------------------------------------------

                if (ModelState.IsValid)
                {
                    using (UserManager userManager = new UserManager())
                    using (var scheduleManager = new ScheduleManager())
                    using (var personManager = new PersonManager())
                    using (var eventManager = new BookingEventManager())
                    using (var permissionManager = new EntityPermissionManager())
                    using (var entityTypeManager = new EntityManager())
                    {
                        try
                        {
                            // get event min und max date from schedules
                            DateTime minDate = model.Schedules.Select(a => a.ScheduleDurationModel.StartDate).ToList().Min();
                            DateTime maxDate = model.Schedules.Select(a => a.ScheduleDurationModel.EndDate).ToList().Max();

                            SendNotificationHelper.BookingAction bookingAction = new SendNotificationHelper.BookingAction();

                            //if event id = 0, create new event with note and description
                            BookingEvent eEvent = new BookingEvent();
                            if (model.Id == 0)
                            {
                                eEvent = eventManager.CreateBookingEvent(model.Name, model.Description, null, minDate, maxDate);
                                bookingAction = SendNotificationHelper.BookingAction.created;
                            }
                            else
                            {
                                eEvent = eventManager.GetBookingEventById(model.Id);
                                countSchedulesBefor = scheduleManager.GetAllSchedulesByEvent(model.Id).Count();
                                eEvent.Name = model.Name;
                                eEvent.Description = model.Description;
                                eEvent.MinDate = minDate;
                                eEvent.MaxDate = maxDate;
                                eventManager.UpdateBookingEvent(eEvent);
                                bookingAction = SendNotificationHelper.BookingAction.edited;
                            }

                            //Delete deleted schedules in DB
                            //foreach (long id in model.DeletedSchedules)
                            //{
                            //    scheduleManager.DeleteSchedule(scheduleManager.GetScheduleById(id));
                            //}



                            List<Notification> notifications = new List<Notification>();

                            //Create or update all schedules for this event
                            foreach (ScheduleEventModel schedule in model.Schedules)
                            {

                                //get user who has created the event/schedule
                                IndividualPerson createdBy = new IndividualPerson();
                                User created = userManager.FindByNameAsync(HttpContext.User.Identity.Name).Result;
                                createdBy = personManager.CreateIndividualPerson(created);

                                Schedule newSchedule = new Schedule();

                                int index = 0;
                                index = schedule.Index;

                                //get scheduled resource
                                SingleResource tempResource = singleResourceManager.GetResourceById(schedule.ResourceId);

                                //Users
                                List<User> users = new List<User>();

                                User contact = new User();
                                Person person = new Person();

                                //get event admin group
                                string[] eventAdminGroups = Helper.Settings.get("EventAdminGroup").ToString().Split(',');

                                //add all persons reserved for the user list
                                if (schedule.ForPersons.Count() > 1)
                                {
                                    foreach (PersonInSchedule user in schedule.ForPersons)
                                    {
                                        User u = userManager.FindByIdAsync(user.UserId).Result;

                                        if (user.IsContactPerson == true)
                                        {
                                            contact = u;
                                            schedule.ContactName = u.DisplayName;
                                            schedule.Contact = user;
                                        }
                                        users.Add(u);
                                    }

                                    person = personManager.CreatePersonGroup(users, contact);
                                }
                                else
                                {
                                    User u = userManager.FindByIdAsync(schedule.ForPersons[0].UserId).Result;
                                    person = personManager.CreateIndividualPerson(u);
                                }

                                //Create or Update schedule
                                if (model.Id == 0 || (countSchedulesCurrent > countSchedulesBefor && schedule.ScheduleId == 0))
                                    newSchedule = scheduleManager.CreateSchedule(schedule.ScheduleDurationModel.StartDate, schedule.ScheduleDurationModel.EndDate, eEvent, tempResource, person, createdBy, schedule.Activities.Select(a => a.Id), schedule.ScheduleQuantity, index);
                                else
                                    UpdateSchedule(schedule, schedule.Activities.Select(a => a.Id), person);


                                //Get affected notificationen for the schedule
                                List<Notification> temp = CheckNotificationForSchedule(schedule);
                                foreach(var n in temp)
                                {
                                    if(!notifications.Contains(n))
                                        notifications.Add(n);
                                }
                                

                                //Add rights to the schedule and event for all user reserved for

                                //get entities types
                                var entityTypeSchedule = entityTypeManager.FindByName("Schedule");
                                var entityTypeEvent = entityTypeManager.FindByName("BookingEvent");

                                //full rights as int
                                int fullRights = (int)RightType.Read + (int)RightType.Write + (int)RightType.Delete + (int)RightType.Grant;

                                //give rights to group if group exsits
                                using (var groupManager = new GroupManager())
                                {
                                    foreach (string g in eventAdminGroups)
                                    {
                                        var group = groupManager.FindByNameAsync(g).Result;
                                        if (group != null)
                                        {
                                            //rights on schedule
                                            if (permissionManager.GetRights(group.Id, entityTypeSchedule.Id, newSchedule.Id) == 0)
                                                permissionManager.Create(group.Id, entityTypeSchedule.Id, newSchedule.Id, fullRights);

                                            //rights on event
                                            if (permissionManager.GetRights(group.Id, entityTypeEvent.Id, eEvent.Id) == 0)
                                                permissionManager.Create(group.Id, entityTypeEvent.Id, eEvent.Id, fullRights);
                                        }
                                    }
                                }

                                //add rights to logged in user if not exsit
                                //rights on schedule 31 is the sum from all rights:  Read = 1, Write = 4, Delete = 8, Grant = 16

                                var userIdLoggedIn = UserHelper.GetUserId(HttpContext.User.Identity.Name);
                                if (permissionManager.GetRights(userIdLoggedIn, entityTypeSchedule.Id, newSchedule.Id) == 0)
                                    permissionManager.Create(userIdLoggedIn, entityTypeSchedule.Id, newSchedule.Id, fullRights);

                                //rights on event
                                if (permissionManager.GetRights(userIdLoggedIn, entityTypeEvent.Id, eEvent.Id) == 0)
                                    permissionManager.Create(userIdLoggedIn, entityTypeEvent.Id, eEvent.Id, fullRights);

                                foreach (PersonInSchedule user in schedule.ForPersons)
                                {
                                    User us = userManager.FindByIdAsync(user.UserId).Result;
                                    if (us.Id != userIdLoggedIn)
                                    {
                                        //rights on schedule 15 is the sum from this rights:  Read = 1, Download = 2, Write = 4, Delete = 8
                                        int schedulesRights = (int)RightType.Read + (int)RightType.Write + (int)RightType.Delete;
                                        if (permissionManager.GetRights(us.Id, entityTypeSchedule.Id, newSchedule.Id) == 0)
                                            permissionManager.Create(us.Id, entityTypeSchedule.Id, newSchedule.Id, schedulesRights);
                                        //rights on event, Read = 1, Write = 4
                                        int eventRights = (int)RightType.Read + (int)RightType.Write;
                                        if (permissionManager.GetRights(us.Id, entityTypeEvent.Id, eEvent.Id) == 0)
                                            permissionManager.Create(us.Id, entityTypeEvent.Id, eEvent.Id, eventRights);
                                    }
                                }
                            }

                            //send field notifications
                            if (notifications.Count > 0)
                                SendNotification(notifications, model.Schedules);

                            SendNotificationHelper.SendBookingNotification(bookingAction, model);

                        }
                        catch (Exception e)
                        {
                            ModelState.AddModelError("", e.Message);
                            return View("EditEvent", model);
                        }
                    }
                }

                else
                {
                    return View("EditEvent", model);
                }
            }

            return RedirectToAction("Calendar", "Calendar");
        }
        /// <summary>
        /// Update schedule in database
        /// </summary>
        /// <param name="schedule"> Filled event schdule model with updates</param>
        /// <param name="activityList">List of selected activities</param>
        /// <param name="person">User(s) where schedule is reserved for. Can be IndividualPerson or PersonGroup </param>
        /// <returns>Updated schedule</returns>
        private Schedule UpdateSchedule(ScheduleEventModel schedule, IEnumerable<long> activityList, Person person)
        {
            using (var uow = this.GetUnitOfWork())
            using (var pManager = new PersonManager())
            using (var schManager = new ScheduleManager())
            {
                Schedule s = schManager.GetScheduleById(schedule.ScheduleId);
                s.StartDate = schedule.ScheduleDurationModel.StartDate;
                s.EndDate = schedule.ScheduleDurationModel.EndDate;
                s.Quantity = schedule.ScheduleQuantity;

                if (person is PersonGroup)
                {
                    PersonGroup pGroup = (PersonGroup)person;
                    pManager.UpdatePersonGroup(pGroup);
                    s.ForPerson = pGroup;
                }
                else if (person is IndividualPerson)
                {
                    IndividualPerson iPerson = (IndividualPerson)person;
                    pManager.UpdateIndividualPerson(iPerson);
                    s.ForPerson = iPerson;
                }

                //add or remove activites

                var scheduleActivitiesIds = s.Activities.Select(c => c.Id).ToList();
                //find new activity ids to add to schedule
                var listTempUpdate = activityList.Except(scheduleActivitiesIds).ToList();
                //find deleted ids to remove from schedule
                var listTempDelete = scheduleActivitiesIds.Except(activityList).ToList();

                //remove
                foreach (var id in listTempDelete)
                {
                    var i = s.Activities.ToList().Find(e => e.Id == id);
                    s.Activities.Remove(i);
                }
                //add
                listTempUpdate.ForEach(a => s.Activities.Add(uow.GetReadOnlyRepository<Activity>().Get(a)));

                schManager.UpdateSchedule(s);

                return s;
            }
        }

        #endregion

        #region Edit Event

        public ActionResult OpenEdit(string id)
        {
            using (var permissionManager = new EntityPermissionManager())
            using (var subjectManager = new SubjectManager())
            {
                BookingEventModel model = (BookingEventModel)Session["Event"];
                if (model != null)
                {
                    model.EditMode = true;
                    for (int i = 0; i < model.Schedules.Count(); i++)
                    {
                        //Has user rights on schedule
                        User user = subjectManager.Subjects.Where(a => a.Name == HttpContext.User.Identity.Name).FirstOrDefault() as User;
                        //model.Schedules[i].EditAccess = permissionManager.HasEffectiveRight(HttpContext.User.Identity.Name, model.Schedules[i].,8, RightType.Write);
                        //model.Schedules[i].DeleteAccess = permissionManager.HasEffectiveRight(HttpContext.User.Identity.Name, 8, model.Schedules[i].ScheduleId, RightType.Delete);

                        model.Schedules[i].EditMode = true;
                        model.Schedules[i].FileConfirmation = true;
                        for (int j = 0; j < model.Schedules[i].Activities.Count(); j++)
                        {
                            model.Schedules[i].Activities[j].EditMode = true;
                        }
                        for (int j = 0; j < model.Schedules[i].ForPersons.Count(); j++)
                        {
                            model.Schedules[i].ForPersons[j].EditMode = true;
                        }
                    }

                    return View("EditEvent", model);
                }
                else
                {
                    //need errors message here
                    return new EmptyResult();
                }
            }
        }

        public ActionResult Edit(long id)
        {
            ViewBag.Title = PresentationModel.GetViewTitleForTenant("Edit Event", this.Session.GetTenant());
            return View("EditEvent", id);
        }


        public ActionResult EditSchedules(long id)
        {
            return PartialView("_enterSchedules", id.ToString());
        }

        public ActionResult EditScheduleActivities(long id)
        {
            using (BookingEventManager eManager = new BookingEventManager())
            {
                BookingEvent e = eManager.GetBookingEventById(id);
                List<ActivityEventModel> model = new List<ActivityEventModel>();
                //e.Activities.ToList().ForEach(r => model.Add(new ActivityEventModel(r)));

                return PartialView("_enterActivities", model);
            }
        }

        #endregion

        #region Edit Event -- TimePeriod

        public ActionResult ShowTimePeriodInSchedule(string index)
        {
            BookingEventModel sEventM = (BookingEventModel)Session["Event"];
            ScheduleEventModel tempSchedule = sEventM.Schedules.Where(a => a.Index == int.Parse(index)).FirstOrDefault();

            if (tempSchedule != null && tempSchedule.ScheduleDurationModel != null)
            {
                tempSchedule.ScheduleDurationModel.EditAccess = tempSchedule.EditAccess;
                tempSchedule.ScheduleDurationModel.EditMode = tempSchedule.EditMode;
                tempSchedule.ScheduleDurationModel.EventId = tempSchedule.EventId;
                tempSchedule.ScheduleDurationModel.Index = tempSchedule.Index;
                return PartialView("_showTimePeriod", tempSchedule.ScheduleDurationModel);
            }
            else
                return new EmptyResult();
        }

        #endregion

        #region Edit Event -- Users

        public ActionResult ShowUsersInSchedule(string index)
        {
            BookingEventModel sEventM = (BookingEventModel)Session["Event"];
            ScheduleEventModel tempSchedule = sEventM.Schedules.Where(a => a.Index == int.Parse(index)).FirstOrDefault();

            if (tempSchedule != null && tempSchedule.ForPersons != null)
            {
                tempSchedule.ForPersons.ForEach(a =>
                {
                    a.EditAccess = tempSchedule.EditAccess;
                    a.EditMode = tempSchedule.EditMode;

                    Party partyPerson = UserHelper.GetPartyByUserId(a.UserId);
                    if (partyPerson != null)
                    {
                        a.MobileNumber = partyPerson.CustomAttributeValues.Where(b => b.CustomAttribute.Name == "Mobile").Select(v => v.Value).FirstOrDefault();
                        a.UserFullName = partyPerson.Name;
                    }

                  
                    a.Index = int.Parse(index);

                });
                return PartialView("_scheduleUsers", tempSchedule.ForPersons);
            }
            else
                return new EmptyResult();
        }

        //Load all user to select one or more for the event
        public ActionResult LoadUsers(string index)
        {
            // retreive list of all users 
            DataTable result = null;
            string groupName = Helper.Settings.get("AlumniGroup").ToString();
            var query = "SELECT partyid, userid FROM public.partyusers Left join users on partyusers.userid = users.id where users.id not in (select userref from users_groups where groupref = (SELECT id FROM groups WHERE name = '"+ groupName +"'))";

            result = retrieve(query);
            List<object> personsIsList = new List<object>();

            for (int i = 0; i < result.Rows.Count; i++)
            {
                personsIsList.Add(new { partyid = result.Rows[i].ItemArray[0], userid = result.Rows[i].ItemArray[1] });
            }

            using (var partyManager = new PartyManager())
            using (var partyTypeManager = new PartyTypeManager())
            using (UserManager userManager = new UserManager())
            {
                //get party type where you store the first and-lastname of the persons
                var accountPartyTypesStr = Helper.Settings.get("AccountPartyTypes");
                var partyType = partyTypeManager.PartyTypes.Where(p => p.Title == accountPartyTypesStr.ToString()).FirstOrDefault();

                //get all parties with person party type
                List<Party> partyPersons = partyManager.PartyRepository.Query(p => p.PartyType == partyType).ToList();

                List<PersonInSchedule> personList = new List<PersonInSchedule>();

                BookingEventModel sEventM = (BookingEventModel)Session["Event"];
                ScheduleEventModel tempSchedule = sEventM.Schedules.Where(a => a.Index == int.Parse(index)).FirstOrDefault();

                List<long> tempUserIds = tempSchedule.ForPersons.Select(c => c.UserId).ToList();

                var found = false; var userId = 0;
                foreach (var partyPerson in partyPersons)
                {
                   //    var userTask = userManager.FindByIdAsync(partyManager.GetUserIdByParty(partyPerson.Id));
                   //    userTask.Wait();
                   //    var user = userTask.Result;
   
                    found = false;

                    // Search if user is in the list -> if not it is a alumni and will be not shown
                    for (int i = 0; i < personsIsList.Count; i++)
                    {
                        var partyid = personsIsList[i].GetType().GetProperty("partyid").GetValue(personsIsList[i], null);

                        if (long.Parse(partyid.ToString()) == partyPerson.Id)
                        {
                            found = true;
                            userId = int.Parse(personsIsList[i].GetType().GetProperty("userid").GetValue(personsIsList[i], null).ToString());
                            break;
                        }
                    }
               
                    bool isContact = false;
                    if (found == true)
                    {
                        //check if user is contect person
                        if (tempSchedule.Contact.UserId == userId)
                            isContact = true;

                        PersonInSchedule pu = new PersonInSchedule();
                        pu.Index = int.Parse(index);
                        pu.UserFullName = partyPerson.Name;
                        pu.UserId = userId;
                        if (tempUserIds.Contains(userId))
                            pu.IsSelected = true;

                        pu.IsContactPerson = isContact;
                        personList.Add(pu);
                    }
            }
            return PartialView("_chooseUsers", personList);
        }
    } 

        public ActionResult ChangeSelectedUser(string userId, string selected, string index)
        {
            //List<PersonInSchedule> sEventUser = (List<PersonInSchedule>)Session["ScheduleUsers"];
            BookingEventModel model = (BookingEventModel)Session["Event"];
            ScheduleEventModel tempSchedule = model.Schedules.Where(a => a.Index == int.Parse(index)).FirstOrDefault();

            //if (sEventUser == null)
            //    sEventUser = new List<PersonInSchedule>();

            using (var partyManager = new PartyManager())
            using (UserManager userManager = new UserManager())
            {
                var userTask = userManager.FindByIdAsync(Convert.ToInt64(userId));
                userTask.Wait();
                var user = userTask.Result;

                PersonInSchedule pUser = new PersonInSchedule(0, user, false);
                pUser.Index = int.Parse(index);

                if (selected == "true")
                {
                    tempSchedule.ForPersons.Add(pUser);
                }
                else
                {
                    int i = tempSchedule.ForPersons.FindIndex(a => a.UserId == Convert.ToInt64(userId));
                    tempSchedule.ForPersons.RemoveAt(i);
                }

                Session["Event"] = model;
            }

            return new EmptyResult();
        }
        // change contact person // remove old set new
        public ActionResult ChangeContactPerson(string userId, string index)
        {
            BookingEventModel model = (BookingEventModel)Session["Event"];
            ScheduleEventModel tempSchedule = model.Schedules.Where(a => a.Index == int.Parse(index)).FirstOrDefault();

            // read current contact user and unset "is contact"
            var currentContact = tempSchedule.ForPersons.FirstOrDefault(a => a.IsContactPerson == true);
            if(currentContact!=null) currentContact.IsContactPerson = false;

            // find new contact
            tempSchedule.ForPersons.First(d => d.UserId == Convert.ToInt64(userId)).IsContactPerson = true;
            tempSchedule.Contact = tempSchedule.ForPersons.First(d => d.UserId == Convert.ToInt64(userId));
            tempSchedule.ContactName = getPartyName(Convert.ToInt64(userId));



            Session["Event"] = model;

            return new EmptyResult();
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

        //Temp adding user to schedule
        public ActionResult AddUsersToSchedule(string scheduleIndex)
        {
            BookingEventModel eventM = (BookingEventModel)Session["Event"];
            ScheduleEventModel tempSchedule = eventM.Schedules.Where(a => a.Index == int.Parse(scheduleIndex)).FirstOrDefault();

                using (SubjectManager subManager = new SubjectManager())
                {
                    if (tempSchedule.ForPersons != null)
                    {
                        tempSchedule.ForPersons.ForEach(a =>
                        {
                            a.EditAccess = tempSchedule.EditAccess;
                            a.EditMode = tempSchedule.EditMode;
                            
                            Party partyPerson = UserHelper.GetPartyByUserId(a.UserId);
                            if (partyPerson != null)
                            {
                                a.MobileNumber = partyPerson.CustomAttributeValues.Where(b => b.CustomAttribute.Name == "Mobile").Select(v => v.Value).FirstOrDefault();
                                a.UserFullName = partyPerson.Name;
                            }
                        });
                    }

                    //get index of modify schedule and update it in the session list
                    var i = eventM.Schedules.FindIndex(p => p.Index == tempSchedule.Index);
                    eventM.Schedules[i] = tempSchedule;
                    Session["Event"] = eventM;

                }
         return PartialView("_scheduleUsers", tempSchedule.ForPersons);

        }

        //open widow to show all users wich are related to one schedule
        public ActionResult ShowAllUsers(string scheduleIndex)
        {
            BookingEventModel sEventM = (BookingEventModel)Session["Event"];
            ScheduleEventModel tempSchedule = sEventM.Schedules.Where(a => a.Index == int.Parse(scheduleIndex)).FirstOrDefault();

            return PartialView("_showUsers", tempSchedule.ForPersons);
        }

        //Remove user from a schedule
        public ActionResult RemoveUserFromSchedule(string userId, string scheduleIndex)
        {
            BookingEventModel sEventM = (BookingEventModel)Session["Event"];
            ScheduleEventModel tempSchedule = sEventM.Schedules.Where(a => a.Index == int.Parse(scheduleIndex)).FirstOrDefault();
            tempSchedule.ForPersons.RemoveAll(a => a.UserId == int.Parse(userId));

            //get index of modify schedule and update it in the session list
            var i = sEventM.Schedules.FindIndex(p => p.Index == tempSchedule.Index);
            sEventM.Schedules[i] = tempSchedule;
            Session["Event"] = sEventM;

            return PartialView("_scheduleUsers", tempSchedule.ForPersons);
        }

        private DataTable retrieve(string queryStr)
        {
            try
            {
                using (IUnitOfWork uow = this.GetUnitOfWork())
                {
                    DataTable table = uow.ExecuteQuery(queryStr);
                    return table;
                }
            }
            catch (Exception ex)
            {
                throw new Exception(string.Format("Could not retrieve alumni information"), ex);
            }
        }


        #endregion

        #region Edit Event -- Activities

        public ActionResult ShowActivityInSchedule(string index)
        {
            BookingEventModel sEventM = (BookingEventModel)Session["Event"];
            ScheduleEventModel tempSchedule = sEventM.Schedules.Where(a => a.Index == int.Parse(index)).FirstOrDefault();

            var index_int = Convert.ToInt32(index);
     
            if (tempSchedule != null && tempSchedule.Activities != null)
            {
                tempSchedule.Activities.ForEach(a => a.EditAccess = tempSchedule.EditAccess);
                tempSchedule.Activities.ForEach(a => a.Index = index_int);
                return PartialView("_showActivities", tempSchedule.Activities);
            }
            else
                return new EmptyResult();
        }

        public ActionResult LoadActivities(string index)
        {
            List<ActivityEventModel> model = new List<ActivityEventModel>();
            using (ActivityManager aManager = new ActivityManager())
            {
                List<Activity> activities = aManager.GetAllAvailableActivityById().ToList();
                BookingEventModel sEventM = (BookingEventModel)Session["Event"];

                ScheduleEventModel tempSchedule = sEventM.Schedules.Where(a => a.Index == int.Parse(index)).FirstOrDefault();
                List<long> tempActivityIds = tempSchedule.Activities.Select(c => c.Id).ToList();
                foreach (Activity a in activities)
                {
                    ActivityEventModel ae = new ActivityEventModel(a);
                    ae.Index = int.Parse(index);
                    if (tempActivityIds.Contains(a.Id))
                    {
                        ae.IsSelected = true;
                    }
                    model.Add(ae);
                }
                return PartialView("_chooseActivities", model);
            }
        }

        public ActionResult ChangeSelectedActivity(string activityId, string selected, string index)
        {
            BookingEventModel sEventM = (BookingEventModel)Session["Event"];
            ScheduleEventModel tempSchedule = sEventM.Schedules.Where(a => a.Index == int.Parse(index)).FirstOrDefault();

            using (ActivityManager aManager = new ActivityManager())
            {
                ActivityEventModel activity = new ActivityEventModel(aManager.GetActivityById(long.Parse(activityId)));

                if (selected == "true")
                {
                    activity.Index = int.Parse(index);
                    tempSchedule.Activities.Add(activity);
                }
                else
                {
                    int i = tempSchedule.Activities.FindIndex(a => a.Id == long.Parse(activityId));
                    tempSchedule.Activities.RemoveAt(i);
                }

                Session["Event"] = sEventM;

                return new EmptyResult();
            }
        }

        //Temp adding activity to schedule
        public ActionResult AddActivitiesToSchedule(string scheduleIndex)
        {
            BookingEventModel eventM = (BookingEventModel)Session["Event"];
            ScheduleEventModel tempSchedule = eventM.Schedules.Where(a => a.Index == int.Parse(scheduleIndex)).FirstOrDefault();

            if (tempSchedule.Activities != null && tempSchedule.Activities.Any())
            {
                tempSchedule.Activities.ForEach(a => { a.EditAccess = true; a.EditMode = true; });
            }
            var i = eventM.Schedules.FindIndex(p => p.Index == tempSchedule.Index);
            eventM.Schedules[i] = tempSchedule;
            Session["Event"] = eventM;

            return PartialView("_showActivities", tempSchedule.Activities);
        }

        //open window to show all activities wich are related to one schedule
        public ActionResult ShowAllActivities(string scheduleIndex)
        {
            BookingEventModel sEventM = (BookingEventModel)Session["Event"];
            ScheduleEventModel tempSchedule = sEventM.Schedules.Where(a => a.Index == int.Parse(scheduleIndex)).FirstOrDefault();

            return PartialView("_showActivities", tempSchedule.Activities);
        }

        //Remove user from a schedule
        public ActionResult RemoveActivityFromSchedule(string activityId, string scheduleIndex)
        {
            BookingEventModel sEventM = (BookingEventModel)Session["Event"];
            ScheduleEventModel tempSchedule = sEventM.Schedules.Where(a => a.Index == int.Parse(scheduleIndex)).FirstOrDefault();
            tempSchedule.Activities.RemoveAll(a => a.Id == long.Parse(activityId));

            //get index of modify schedule and update it in the session list
            var i = sEventM.Schedules.FindIndex(p => p.Index == tempSchedule.Index);
            sEventM.Schedules[i] = tempSchedule;
            Session["EventResources"] = sEventM;

            return PartialView("_showActivities", tempSchedule.Activities);
        }

        #endregion

        #region Edit Event -- Add Resources

        public ActionResult AddResource()
        {
            ViewBag.Title = PresentationModel.GetViewTitleForTenant("Add Resources", this.Session.GetTenant());

            BookingEventModel model = (BookingEventModel)Session["Event"];
            List<ResourceCart> resourceCart = new List<ResourceCart>();

            foreach (ScheduleEventModel s in model.Schedules)
            {
                ResourceCart cartItem = new ResourceCart();
                cartItem.Name = s.ResourceName;
                cartItem.NewInCart = false;
                cartItem.Index = s.Index;
                resourceCart.Add(cartItem);
            }

            Session["ResourceCart"] = resourceCart;

            return View("SelectResources");
        }

        #endregion

        #region Edit Event -- Functions

        
        /// <summary>
        /// Fill value from source schedule to all other schedules
        /// </summary>
        /// <param name="index">Index of source schedule </param>
        /// <returns>View with copied schedule values in event</returns>
        public ActionResult UseValuesForAllSchedules(string index)
        {
            BookingEventModel model = (BookingEventModel)Session["Event"];
            ScheduleEventModel tempSchedule = model.Schedules.Where(a => a.Index == int.Parse(index)).FirstOrDefault();

            foreach (ScheduleEventModel s in model.Schedules)
            {
                if (s.Index != int.Parse(index))
                {
                    //create for every person object in list a new object
                    List<PersonInSchedule> copyListPersons = new List<PersonInSchedule>();
                    tempSchedule.ForPersons.ForEach(r => copyListPersons.Add(new PersonInSchedule(r, s.Index)));
                    s.ForPersons = copyListPersons;

                    //create for every activity object in list a new object
                    List<ActivityEventModel> copyListActivities = new List<ActivityEventModel>();
                    tempSchedule.Activities.ForEach(r => copyListActivities.Add(new ActivityEventModel(r, s.Index)));
                    s.Activities = copyListActivities;
                    
                    s.ScheduleQuantity = tempSchedule.ScheduleQuantity;
                    s.ScheduleDurationModel = new ScheduleDurationModel(tempSchedule.ScheduleDurationModel, s.Index); ;
                }
            }

            Session["Event"] = model;

            return PartialView("EditEvent", model);

        }

        public ActionResult RemoveScheduleFromEvent(string index)
        {
            BookingEventModel model = (BookingEventModel)Session["Event"];
            if (model.Schedules.Count() > 1)
            {
                var i = model.Schedules.FindIndex(p => p.Index == Convert.ToInt64(index));

                //remove resource schedule from cart, too
                List<ResourceCart> cartModel = (List<ResourceCart>)Session["ResourceCart"];
                var inCart = cartModel.Where(a => a.Index == Convert.ToInt32(index)).Count() == 1;
                if (inCart)
                {
                    cartModel.RemoveAt(i);
                    Session["ResourceCart"] = cartModel;
                }

                if (model.Schedules[i].ScheduleId != 0)
                {
                    using (ScheduleManager scheduleManager = new ScheduleManager())
                    {
                        scheduleManager.DeleteSchedule(scheduleManager.GetScheduleById(model.Schedules[i].ScheduleId));
                        //model.DeletedSchedules.Add(model.Schedules[i].ScheduleId);
                    }
                }

                model.Schedules.RemoveAt(i);
                Session["Event"] = model;
            }
            else
            {
                //inform User that they can not delete the last
            }

            return PartialView("EditEvent", model);
        }


        public ActionResult CopySchedule(string index)
        {
            BookingEventModel sEventM = (BookingEventModel)Session["Event"];
            ScheduleEventModel tempSchedule = sEventM.Schedules.Where(a => a.Index == int.Parse(index)).FirstOrDefault();
            int newIndex = sEventM.Schedules.Select(a => a.Index).ToList().Max() + 1;
            ScheduleEventModel newSchedule = new ScheduleEventModel(tempSchedule, newIndex);

            sEventM.Schedules.Add(newSchedule);
            Session["Event"] = sEventM;

            return PartialView("EditEvent", sEventM);
        }

        private int GetQuantityLeftOnTime(long resourceId, DateTime startDate, DateTime endDate)
        {
            using (ResourceManager rManger = new ResourceManager())
            using (ScheduleManager schManager = new ScheduleManager())
            {
                int quantityLeft = 0;
                SingleResource sr = rManger.GetResourceById(resourceId);
                List<Schedule> schedules = schManager.GetAllSchedulesByResource(resourceId);

                //count quantity which is used in schedules
                int countQinSchedules = 0;
                foreach (Schedule s in schedules)
                {
                    countQinSchedules = countQinSchedules + s.Quantity;
                }

                quantityLeft = sr.Quantity - countQinSchedules;

                return quantityLeft;
            }
        }

        public bool CheckResourceAvailability(long id, string startDate, string endDate)
        {
            DateTime inputStart = DateTime.ParseExact(startDate, "dd.MM.yyyy", null);
            DateTime inputEnd = DateTime.ParseExact(endDate, "dd.MM.yyyy", null);

            bool available = false;
            using (ResourceManager rManger = new ResourceManager())
            using (ScheduleManager schManager = new ScheduleManager())
            {

                SingleResource resource = rManger.GetResourceById(id);
                List<Schedule> resourceSchedules = schManager.GetAllSchedulesByResource(id);
                List<Schedule> temp = new List<Rbm.Entities.Booking.Schedule>();
                foreach (Schedule sch in resourceSchedules)
                {
                    //if ((inputStart >= sch.StartDate && inputStart <= sch.EndDate) || (inputEnd >= sch.StartDate && inputEnd <= sch.EndDate))
       
                    if ((DateTime.Compare(inputStart.Date, sch.StartDate.Date) >= 0 && DateTime.Compare(inputEnd.Date, sch.EndDate.Date) <= 0) || (DateTime.Compare(inputEnd.Date, sch.StartDate.Date) >= 0 && DateTime.Compare(inputStart.Date, sch.EndDate.Date) <= 0))
                    {
                        temp.Add(sch);
                        //if (resourceSchedules.Count() >= resource.Quantity)
                        //{
                        //    available = false;
                        //    break;
                        //}
                        //else
                        //    available = true;
                    }
                    //else
                    //    available = true;
                }

                if (temp.Count() > 0)
                {
                    int bookedQuantity = 0;
                    foreach (Schedule s in temp)
                    {
                        bookedQuantity = bookedQuantity + s.Quantity;
                    }

                    if (bookedQuantity >= resource.Quantity)
                    {
                        available = false;
                    }
                    else
                        available = true;
                }
                else
                    available = true;

                return available;
            }
        }

        //if resource not avaiable then open widow with alternatives
        public ActionResult LoadResourceAlternatives(string scheduleIndex)
        {
            BookingEventModel sEventM = (BookingEventModel)Session["Event"];
            ScheduleEventModel tempSchedule = sEventM.Schedules.Where(a => a.Index == int.Parse(scheduleIndex)).FirstOrDefault();

            AvailabilityScheduleModel model = new AvailabilityScheduleModel();
            model.CurrentResourceId = tempSchedule.ResourceId;
            model.CurrentResourceName = tempSchedule.ResourceName;
            model.Index = int.Parse(scheduleIndex);

            FindSimilarResources(tempSchedule.ResourceId).ToList().ForEach(r => model.AlternateResources.Add(new AlternateEventResource(r, tempSchedule.ScheduleDurationModel.StartDate, tempSchedule.ScheduleDurationModel.EndDate, tempSchedule.Contact.UserId)));
            //remove this resource from the alternative list
            model.AlternateResources.RemoveAll(a => a.ResourceId == tempSchedule.ResourceId);
            return PartialView("_resourceAvailability", model);
        }

        //find similar resources for one resource based on there domain items
        private List<SingleResource> FindSimilarResources(long resourceId)
        {
            List<SingleResource> similarResources = new List<SingleResource>();
            List<string> DomainItems = new List<string>();

            using (ResourceManager srManager = new ResourceManager())
            using (ResourceStructureAttributeManager rsaManager = new ResourceStructureAttributeManager())
            {
                SingleResource sr = srManager.GetResourceById(resourceId);

                //first get all DomainItem values from this resource
                foreach (ResourceAttributeUsage usage in sr.ResourceStructure.ResourceAttributeUsages)
                {
                    ResourceStructureAttribute a = usage.ResourceStructureAttribute;
                    if (a.Constraints.Count() > 0)
                    {
                       foreach (Dlm.Entities.DataStructure.Constraint c in a.Constraints)
                         {
                            if (c is DomainConstraint)
                            {
                                TextValue value = rsaManager.GetTextValueByUsageAndResource(usage.Id, resourceId);
                                if (value != null) DomainItems.Add(value.Value);
                            }  
                        }
                    }
                }

                List<long> resources = rsaManager.GetResourcesByValues(DomainItems);
                foreach (long r in resources)
                {
                    similarResources.Add(srManager.GetResourceById(r));
                }

                return similarResources;
            }
        }

        public ActionResult LoadSimilarResources(string index, string id)
        {
            BookingEventModel sEventM = (BookingEventModel)Session["Event"];

            ScheduleEventModel tempSchedule = sEventM.Schedules.Where(a => a.Index == int.Parse(index)).FirstOrDefault();

            //find similar resource to the sourceresource based on there domain constrains
            List<SingleResource> similarResources = FindSimilarResources(long.Parse(id));

            List<AlternateEventResource> model = new List<AlternateEventResource>();
            similarResources.ForEach(r => model.Add(new AlternateEventResource(r, tempSchedule.ScheduleDurationModel.StartDate, tempSchedule.ScheduleDurationModel.EndDate, tempSchedule.Contact.UserId)));

            //remove the source resource from the result list
            model.RemoveAll(a => a.ResourceId == tempSchedule.ResourceId);

            return PartialView("_similarResources", model);
        }

        public ActionResult UseSimilarResources(List<AlternateEventResource> model)
        {
            BookingEventModel sEventM = (BookingEventModel)Session["Event"];
            using (ResourceManager srManager = new ResourceManager())
            using (UserManager userManager = new UserManager())
            {
                foreach (AlternateEventResource r in model)
                {
                    if (r.isChoosen == true)
                    {
                        SingleResource sr = srManager.GetResourceById(r.ResourceId);
                        ScheduleEventModel tempSchedule = new ScheduleEventModel(sr);
                        tempSchedule.ScheduleDurationModel.StartDate = r.StartDate;
                        tempSchedule.ScheduleDurationModel.EndDate = r.EndDate;
                        var userTask = userManager.FindByIdAsync(Convert.ToInt64(r.ContactUserId));
                        userTask.Wait();
                        var user = userTask.Result;
                        var person = new PersonInSchedule(0, user, true);
                        tempSchedule.Contact = person;
                        tempSchedule.ForPersons.Add(person);
                        int index = sEventM.Schedules.Select(a => a.Index).ToList().Max() +1;
                        tempSchedule.Index = index;
                        tempSchedule.ScheduleQuantity = 1;
                        sEventM.Schedules.Add(tempSchedule);
                    }
                }

                Session["Event"] = sEventM;
                return Json(new { success = true });
            }
        }


        public ActionResult UseAlternateResource(AvailabilityScheduleModel model, string chooseAlternateResource)
        {
            //List<ScheduleEventModel> sEventM = (List<ScheduleEventModel>)Session["EventResources"];

            BookingEventModel sEventM = (BookingEventModel)Session["Event"];

            try
            {
                AlternateEventResource a = model.AlternateResources.Where(d => d.ResourceId == long.Parse(chooseAlternateResource)).FirstOrDefault();

                using (ResourceManager srManager = new ResourceManager())
                {
                    SingleResource sr = srManager.GetResourceById(long.Parse(chooseAlternateResource));
                    ScheduleEventModel tempSchedule = new ScheduleEventModel(sr);
                    tempSchedule.ScheduleDurationModel.StartDate = a.StartDate;
                    tempSchedule.ScheduleDurationModel.EndDate = a.EndDate;

                    //Remove currentResource
                    var i = sEventM.Schedules.FindIndex(p => p.Index == Convert.ToInt64(model.Index));
                    sEventM.Schedules.RemoveAt(i);

                    //give new event schedule a index
                    int index = sEventM.Schedules.Select(s => s.Index).ToList().Max() + 1;
                    tempSchedule.Index = index;

                    //Add alternative to event schedules
                    sEventM.Schedules.Add(tempSchedule);
                }
            }
#pragma warning disable CS0168 // The variable 'e' is declared but never used
            catch (Exception e)
#pragma warning restore CS0168 // The variable 'e' is declared but never used
            {
            }

            Session["Event"] = sEventM;

            return Json(new { success = true });
        }

        public ActionResult UseAlternateTimePeriod(AvailabilityScheduleModel model)
        {
            List<ScheduleEventModel> sEventM = (List<ScheduleEventModel>)Session["EventResources"];
            ScheduleEventModel tempSchedule = sEventM.Where(a => a.Index == model.Index).FirstOrDefault();

            tempSchedule.ScheduleDurationModel.StartDate = model.AlternateStartDate;
            tempSchedule.ScheduleDurationModel.EndDate = model.AlternateEndDate;

            //get index of modify schedule and update it in the session list
            var i = sEventM.FindIndex(p => p.Index == tempSchedule.Index);
            sEventM[i] = tempSchedule;
            Session["EventResources"] = sEventM;

            //return PartialView("_gridSchedules", sEventM);
            return Json(new { success = true });
        }

        private List<Notification> CheckNotificationForSchedule(ScheduleEventModel schedule)
        {
            using (NotificationManager nManager = new NotificationManager())
            using (ResourceManager rManager = new ResourceManager())
            using (ResourceStructureAttributeManager rsaManager = new ResourceStructureAttributeManager())
            {



                List<Notification> notificationList = nManager.GetNotificationsByTimePeriod(schedule.ScheduleDurationModel.StartDate, schedule.ScheduleDurationModel.EndDate);
                List<Notification> resultList = new List<Notification>();

                if (notificationList.Count() > 0)
                {
                    SingleResource resource = rManager.GetResourceById(schedule.ResourceId);

                    List<long> attributeIds = new List<long>();
                    List<TextValue> values = new List<TextValue>();
                    foreach (ResourceAttributeUsage u in resource.ResourceStructure.ResourceAttributeUsages)
                    {
                        if (u.IsFileDataType == false)
                        {
                            if (u.ResourceStructureAttribute.Constraints.Count > 0)
                            {
                                TextValue v = rsaManager.GetTextValueByUsageAndResource(u.Id, schedule.ResourceId);
                                values.Add(v);
                            }
                        }
                    }
                    List<NotificationDependency> deps = new List<NotificationDependency>();
                    foreach (Notification n in notificationList)
                    {
                        List<NotificationDependency> depList = nManager.GetNotificationDependenciesByNotification(n.Id);


                        var param1 = Expression.Parameter(typeof(NotificationDependency), "p");
                        Expression exp1 = null;

                        foreach (TextValue tv in values)
                        {
                            if (tv == null) break;

                            //query += string.Format("tmp.AttributeId == {0} && tmp.DomainItem == {1}", tv.ResourceAttributeUsage.ResourceStructureAttribute.Id, tv.Value);
                            var exp = Expression.AndAlso(

                                Expression.Equal(
                                  Expression.Property(param1, "AttributeId"),
                                  Expression.Constant(tv.ResourceAttributeUsage.ResourceStructureAttribute.Id)
                                  ),
                                Expression.Equal(
                                  Expression.Property(param1, "DomainItem"),
                                  Expression.Constant(tv.Value)
                                  )

                                );
                            if (exp1 == null)
                                exp1 = exp;
                            else
                                exp1 = Expression.Or(exp1, exp);
                        }

                        var typedExpression = Expression.Lambda<Func<NotificationDependency, bool>>(exp1, new ParameterExpression[] { param1 });
                        deps = depList.AsQueryable().Where(typedExpression).ToList();

                        if (deps.Count() == values.Count())
                            resultList.Add(n);
                    }
                }

                return resultList;
            }
        }

        Expression<Func<NotificationDependency, bool>> MakePredicate(int id)
        {
            return u => u.Id == id;
        }

        private void SendNotification(List<Notification> notifications, List<ScheduleEventModel> schedules)
        {
            using (var subManager = new SubjectManager())
            {
                List<string> userToNotify = new List<string>();

                foreach (ScheduleEventModel s in schedules)
                {
                    foreach (PersonInSchedule p in s.ForPersons)
                    {
                        User u = subManager.Subjects.Where(a => a.Id == p.UserId).FirstOrDefault() as User;
                        if (!userToNotify.Contains(u.Email))
                            userToNotify.Add(u.Email);
                    }
                }

                string message = "";

                List<Notification> distinctNotifications = notifications.Distinct().ToList();

                foreach (Notification n in distinctNotifications)
                {
                    message += "<b>" + n.Subject + "</b><br/>";
                    message += "Startdate:" + n.StartDate + "<br/>";
                    message += "EndDate:" + n.EndDate + "<br/>";
                    message += n.Message + "<br/><hr/>";
                }
                string subject = "NoReply: BExIS Booking Notification to your Event";

                SendNotificationHelper.SendNotification(userToNotify, message, subject);
            }

        }

        #endregion

        #region Event -- File Resource Structure Attribute Value

        public ActionResult DownloadFile(string id)
        {
            using (ResourceStructureAttributeManager valueManager = new ResourceStructureAttributeManager())
            {
                FileValue value = valueManager.GetFileValueById(long.Parse(id));

                FileValue fv = (FileValue)value;
                byte[] contents = fv.Data;


                return File(contents, fv.Minmetype);
            }
        }

        public ActionResult OnChangeFileConfirmation(string resourceId)
        {
            BookingEventModel model = (BookingEventModel)Session["Event"];
            long rId = long.Parse(resourceId);

            for (int i = 0; i < model.Schedules.Count(); i++)
            {
                if (model.Schedules[i].ResourceId == rId)
                {
                    ScheduleEventModel temp = (ScheduleEventModel)model.Schedules[i];
                    temp.FileConfirmation = true;
                    int j = model.Schedules.FindIndex(p => p.Index == model.Schedules[i].Index);
                    model.Schedules[j] = temp;
                }
            }

            Session["Event"] = model;

            return View("EditEvent", model);
        }

        #endregion

        #region Show Event
        //
        public ActionResult Show(long id)
        {
            Session["ScheduleUsers"] = null;
            //Session["ScheduleActivities"] = null;
            Session["Event"] = null;

            using (var permissionManager = new EntityPermissionManager())
            using (var entityTypeManager = new EntityManager())
            using (var eManager = new BookingEventManager())
            {
                BookingEvent e = eManager.GetBookingEventById(id);
                
                BookingEventModel model = new BookingEventModel(e);
                model.EditMode = false;

                //get permission from logged in user
                long userId = UserHelper.GetUserId(HttpContext.User.Identity.Name);

                //Check permission for BookingEvent
                Entity entity = entityTypeManager.FindByName("BookingEvent");
                model.EditAccess = permissionManager.HasEffectiveRight(userId, new List<long>() { entity.Id }, id, RightType.Write);
                model.DeleteAccess = permissionManager.HasEffectiveRight(userId, new List<long>() { entity.Id }, id, RightType.Delete);


                //Check permission for Schedule
                Entity entity2 = entityTypeManager.FindByName("Schedule");
                model.Schedules.ForEach(a => a.EditAccess = permissionManager.HasEffectiveRight(userId, new List<long>() { entity2.Id }, a.ScheduleId, RightType.Write));
                model.Schedules.ForEach(a => a.DeleteAccess = permissionManager.HasEffectiveRight(userId, new List<long>() { entity2.Id }, a.ScheduleId, RightType.Delete));

                //Set Edit access 
                foreach (var s in model.Schedules)
                {
                    if (s.EditAccess == true)
                    {
                        model.EditAccess = true;
                    }
                }

                Session["Event"] = model;


                return View("EditEvent", model);
            }

        }

        //in show event view
        public ActionResult ShowActivities(long Id)
        {
            using (BookingEventManager eManager = new BookingEventManager())
            {
                BookingEvent e = eManager.GetBookingEventById(Id);
                return PartialView("_showActivities", new ShowEventModel(e));
            }
        }

        //in show event view
        public ActionResult ShowSchedules(long Id)
        {
            using (BookingEventManager eManager = new BookingEventManager())
            {
                BookingEvent e = eManager.GetBookingEventById(Id);
                return PartialView("_showSchedules", new ShowEventModel(e));
            }
        }


        #endregion

        #region Delete Event

        public ActionResult Delete(long id)
        {
            using (BookingEventManager eManager = new BookingEventManager())
            {
                BookingEvent e = eManager.GetBookingEventById(id);

                SendNotificationHelper.SendBookingNotification(SendNotificationHelper.BookingAction.deleted, new BookingEventModel(e));
                eManager.DeleteBookingEvent(e);

                return RedirectToAction("Calendar", "Calendar");
            }
        }

        #endregion

        #region Validation - General

        private string CheckDateInconsistency(DateTime startDate, DateTime endDate)
        {
            if (startDate > endDate)
            {
                return "The end date is befor start date.";
            }

            return null;
        }

        private string CheckTimePeriodAgainstDuration(ScheduleDurationModel model)
        {
            string error = null;
            int diff = 0;
            double a = 0;
            switch (model.TimeUnit)
            {
                case SystemDefinedUnit.second:
                    diff = TimeHelper.GetDifferenceInSeconds(model.StartDate, model.EndDate);
                    a = diff / Convert.ToDouble(model.DurationValue);
                    if (a % 1 != 0)
                        error = String.Format("You must book the resource in a duration of {0} second(s).", model.DurationValue);
                    break;
                case SystemDefinedUnit.minute:
                    diff = TimeHelper.GetDifferenceInMinutes(model.StartDate, model.EndDate);
                    a = diff / Convert.ToDouble(model.DurationValue);
                    if (a % 1 != 0)
                        error = String.Format("You must book the resource in a duration of {0} minute(s).", model.DurationValue);
                    break;
                case SystemDefinedUnit.hour:
                    diff = TimeHelper.GetDifferenceInMinutes(model.StartDate, model.EndDate);
                    a = diff / Convert.ToDouble(model.DurationValue);
                    if (a % 1 != 0)
                        error = String.Format("You must book the resource in a duration of {0} hour(s).", model.DurationValue);
                    break;
                case SystemDefinedUnit.day:
                    diff = TimeHelper.GetDifferenceInDays(model.StartDate, model.EndDate);
                    a = diff / Convert.ToDouble(model.DurationValue);
                    if (a % 1 != 0)
                        error = String.Format("You must book the resource in a duration of {0} day(s).", model.DurationValue);
                    break;
                case SystemDefinedUnit.week:
                    diff = TimeHelper.GetDifferenceInDays(model.StartDate, model.EndDate);
                    a = diff / (Convert.ToDouble(model.DurationValue) * 7);
                    if (a % 1 != 0)
                        error = String.Format("You must book the resource in a duration of {0} week(s).", model.DurationValue);
                    break;
                case SystemDefinedUnit.month:
                    diff = TimeHelper.GetDifferenceInMonths(model.StartDate, model.EndDate);
                    a = diff / (Convert.ToDouble(model.DurationValue));
                    if (a % 1 != 0)
                        error = String.Format("You must book the resource in a duration of {0} month(s).", model.DurationValue);
                    break;
                case SystemDefinedUnit.year:
                    diff = TimeHelper.GetDifferenceInDays(model.StartDate, model.EndDate);
                    a = diff / (Convert.ToDouble(model.DurationValue) * 365);
                    if (a % 1 != 0)
                        error = String.Format("You must book the resource in a duration of {0} month(s).", model.DurationValue);
                    break;
            }

            return error;
        }

        private bool HasScheduleUser(ScheduleEventModel se)
        {
            bool hasUser = false;

            if (se.ForPersons.Count() == 0)
            {
                hasUser = false;
                return hasUser;
            }
            else
                hasUser = true;

            return hasUser;
        }

        //get back how many user to munch in the schedule, validate against numOfShare and avaiable quantity
        private int CheckNumberofUsersInSchedule()
        {

            return 0;
        }

        private bool HasScheduleActivity(ScheduleEventModel se)
        {
            bool hasActivity = false;
            BookingEventModel eventM = (BookingEventModel)Session["Event"];

            if (se.Activities.Count() == 0)
            {
                hasActivity = false;
                return hasActivity;
            }
            else
            {
                hasActivity = true;
            }

            return hasActivity;
        }

        private bool GetAvailableQuantity(long resourceId, int resourceQuantity, DateTime startDate, DateTime endDate, int requestQuantity, long scheduleId)
        {
            int availableQuantity = 0;
            using (ScheduleManager schManager = new ScheduleManager())
            {

                var allDays = Enumerable.Range(0, 1 + endDate.Date.Subtract(startDate.Date).Duration().Days).Select(offset => startDate.Date.AddDays(offset));
                List<int> schedulesQuantitiesPDay = new List<int>();

                foreach (DateTime date in allDays)
                {
                    List<Schedule> allSchedules = schManager.GetSchedulesBetweenStartAndEndDateByResource(date.Date, date.Date, resourceId);
                    int schedulesQuantity = 0;
                    foreach (Schedule s in allSchedules)
                    {
                        if(s.Id != scheduleId)
                            schedulesQuantity += s.Quantity;
                        else
                            schedulesQuantity += requestQuantity - s.Quantity;
                    }

                    schedulesQuantitiesPDay.Add(schedulesQuantity);
                }

                availableQuantity = resourceQuantity - schedulesQuantitiesPDay.Max();

                //check schedules which are temporary in den Event
                BookingEventModel eventM = (BookingEventModel)Session["Event"];
                List<ScheduleEventModel> tempSchedules = eventM.Schedules.Where(a => a.ResourceId == resourceId).ToList();
                if (tempSchedules.Count > 1)
                {
                    int tempScheduleQuantity = 0;
                    foreach (ScheduleEventModel t in tempSchedules)
                    {
                        if ((DateTime.Compare(startDate, t.ScheduleDurationModel.StartDate) >= 0 && DateTime.Compare(endDate, t.ScheduleDurationModel.EndDate) <= 0) || (DateTime.Compare(endDate, t.ScheduleDurationModel.StartDate) >= 0 && DateTime.Compare(startDate, t.ScheduleDurationModel.EndDate) <= 0))
                        {
                            tempScheduleQuantity = tempScheduleQuantity + t.ScheduleQuantity;
                        }
                    }
                    availableQuantity = availableQuantity - (tempScheduleQuantity- requestQuantity);
                }

                if (availableQuantity > 0 && availableQuantity >= requestQuantity)
                    return true;
                else
                    return false;

            }
        }

        #endregion

        #region Validation - Constraints



        #endregion

    }
}
