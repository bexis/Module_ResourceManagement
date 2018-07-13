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

namespace BExIS.Modules.RBM.UI.Controllers
{
    public class ScheduleController : Controller
    {
        public ActionResult Schedule()
        {
            ViewBag.Title = PresentationModel.GetViewTitleForTenant("Schedule Calendar", this.Session.GetTenant());
            Session["Filter"] = null;
            Session["FilterSchedules"] = null;
            return View("Schedule");
        }
      
        #region Create Event -- Filter Resources

        //internal useage for resóurce filtering

        private struct FilterTreeItem
        {
            public long Id;
            public string Value;
        }

        //internal usage to store a filter query
        private struct Filter
        {
            public DateTime StartDate;
            public DateTime EndDate;
            public int Quantity;
            //true if filter is active
            public bool IsSet;
            public List<FilterTreeItem> TreeFilterItems;
        }

        //internal usage to store seleced resources schedules in a session
        private struct TempSchedule
        {
            public long Id;
            public DateTime StartDate;
            public DateTime EndDate;
        }

        public ActionResult OnChangeFilter(string selectedItems)
        {
            Session["TreeFilterResults"] = null;
            //set result set for tree Filter null
            Session["TreeFilterResults"] = null;

            //get all selected resources from session
            List<TempSchedule> selectedResources = new List<TempSchedule>();

            //if (Session["SelectedResources"] != null)
            //    selectedResources = (List<TempSchedule>)Session["SelectedResources"];

            //Filter is this format: AttrID_DomainItem, AttrID_Domain
            List<string> items = selectedItems.Split(',').ToList();

            List<ResourceAttributeValueModel> treeDomainList = new List<ResourceAttributeValueModel>();
            List<SelectResourceForEventModel> resultFromTreeFilter = new List<SelectResourceForEventModel>();

                SingleResourceManager rManager = new SingleResourceManager();

                ResourceStructureAttributeManager rsaManager = new ResourceStructureAttributeManager();
                List<SingleResource> resources = rManager.GetAllResources().ToList();

                //Create for each Resource TreeDomainModel witch includes all Attribute Ids and all values
                foreach (SingleResource r in resources)
                {
                    ResourceAttributeValueModel treeDomainModel = new ResourceAttributeValueModel(r);
                    treeDomainList.Add(treeDomainModel);
                }
            
            //else
            //{
            //    List<SelectResourceForEventModel> tempResults = new List<SelectResourceForEventModel>();
            //    tempResults = (List<SelectResourceForEventModel>)Session["FilterResults"];
            //    foreach (SelectResourceForEventModel tempResult in tempResults)
            //    {
            //        treeDomainList.Add(new ResourceAttributeValueModel(rManager.GetResourceById(tempResult.Id)));
            //    }

            //}

            List<FilterTreeItem> filterList = new List<FilterTreeItem>();
           
            //split Id and DomainItem and add it to a FilterItem list
            foreach (string item in items)
            {
                //index 0 = attrbute id, index1 domainvalue
                List<string> i = item.Split('_').ToList();
                FilterTreeItem filterItem = new FilterTreeItem();
                filterItem.Id = Convert.ToInt64(i[0]);
                filterItem.Value = i[1].ToString();
                filterList.Add(filterItem);
            }


            //Dictionary to save every Filter (domain items) to one attr
            Dictionary<long, List<string>> filterDic = GetFilterDic(filterList);

            List<ResourceAttributeValueModel> temp = new List<ResourceAttributeValueModel>();

            //check for every TreeDomainModel (resource) if fits the filter
            foreach (ResourceAttributeValueModel m in treeDomainList)
            {
                if (CheckTreeDomainModel(m, filterDic))
                {
                        resultFromTreeFilter.Add(new SelectResourceForEventModel(m.Resource));
                }
            }

            List<SelectResourceForEventModel> endResultList = new List<SelectResourceForEventModel>();
            Filter filter = new Filter();
            if (Session["Filter"] != null)
                filter = (Filter)Session["Filter"];

            //if filter is set apply it on the results
            if (filter.IsSet == true)
            {
                endResultList = ApplyFilter(resultFromTreeFilter, filter);
            }
            else
                endResultList = resultFromTreeFilter;

            Session["TreeFilterResults"] = endResultList;
            
            return PartialView("_gridResources", endResultList);
        }

        public ActionResult ApplyFilter(string startdate, string enddate, string quantity)
        {
            Session["Filter"] = null;

            Filter filter = new Filter();
            filter.StartDate = DateTime.Parse(startdate);
            filter.EndDate = DateTime.Parse(enddate);
            filter.Quantity = int.Parse(quantity);
            filter.IsSet = true;

            Session["Filter"] = filter;
            List<SelectResourceForEventModel> resourcesEventList = new List<SelectResourceForEventModel>();

            //if the user choose already a treefilter then the results in the session and apply filter on this results
            if (Session["TreeFilterResults"] != null)
            {
                resourcesEventList = (List<SelectResourceForEventModel>)Session["TreeFilterResults"];
            }
            else
            {
                SingleResourceManager rManager = new SingleResourceManager();
                List<SingleResource> resources = rManager.GetAllResources().ToList();
                resources.ForEach(r => resourcesEventList.Add(new SelectResourceForEventModel(r)));
            }
            
            List<SelectResourceForEventModel> resultList = new List<SelectResourceForEventModel>();
            resultList = ApplyFilter(resourcesEventList, filter);
            Session["FilterResults"] = resultList;

            return PartialView("_gridResources", resultList);
        }

        private List<SelectResourceForEventModel> ApplyFilter(List<SelectResourceForEventModel> list, Filter filter)
        {
            List<SelectResourceForEventModel> endResultList = new List<SelectResourceForEventModel>();
            ScheduleManager schManager = new ScheduleManager();

            //Filter the result list withe the other criterias if given
            foreach (SelectResourceForEventModel r in list)
            {
                int resourceQuantity = 0;
                if (r.ResourceQuantity == "no limitation")
                    resourceQuantity = 0;
                else
                    resourceQuantity = int.Parse(r.ResourceQuantity);

                
                //get all schedule where resource is booked
                List<Schedule> allSchedules = schManager.GetAllSchedulesByResource(r.Id);
                List<Schedule> inTimeSchedules = new List<Schedule>();
                int schedulesQunatity = 0;
                foreach (Schedule s in allSchedules)
                {
                    //get all schedule in the given time period
                    if ((DateTime.Compare(filter.StartDate, s.StartDate) >= 0 && DateTime.Compare(filter.StartDate, s.EndDate) <= 0) || (DateTime.Compare(filter.EndDate, s.StartDate) >= 0 && DateTime.Compare(filter.EndDate, s.EndDate) <= 0))
                    {
                        //Count all quantities in in time schedules
                        schedulesQunatity = schedulesQunatity + s.Quantity;
                    }
                }
                if (resourceQuantity != 0)
                {
                    //if diff of resourceQuantity and schedulesQunatity minus requested quantity is posive add to resultlist
                    if (((resourceQuantity - schedulesQunatity) - filter.Quantity) >= 0)
                    {
                        r.AvailableQuantity = resourceQuantity - schedulesQunatity;
                        endResultList.Add(r);
                    }
                }
                else
                    endResultList.Add(r);
            }

            return endResultList;
        }

        public ActionResult RemoveFilter(string selectedItems)
        {
            Session["Filter"] = null;
            return RedirectToAction("OnChangeFilter", new RouteValueDictionary(new { controller = "Schedule", action = "Main", selectedItems = selectedItems }));
        }

        private bool CheckTreeDomainModel(ResourceAttributeValueModel model, Dictionary<long, List<string>> filters)
        {
            foreach (KeyValuePair<long, List<string>> kp in filters)
            {
                if (IsResult(model, kp.Key, kp.Value) == false) return false;
            }

            return true;
        }

        //Create dictionary for every attribute filter, e.g. attr id 1 with 3 domain items
        private Dictionary<long, List<string>> GetFilterDic(List<FilterTreeItem> filters)
        {
            Dictionary<long, List<string>> filterDic = new Dictionary<long, List<string>>();

            foreach (FilterTreeItem item in filters)
            {
                if (filterDic.Keys.Contains(item.Id))
                    filterDic[item.Id].Add(item.Value);
                else
                {
                    List<string> values = new List<string>();
                    values.Add(item.Value);
                    filterDic.Add(item.Id, values);
                }
            }

            return filterDic;
        }

        //checks if is a filter result
        private bool IsResult(ResourceAttributeValueModel model, long id, List<string> values)
        {
            bool temp = false;

            foreach (string value in values)
            {
                //int index = model.AttributeIds.IndexOf(id);

                //if (model.Values.ElementAt(index).Equals(value))
                if(model.Values.Contains(value))
                {
                    temp = true;
                }
            }

            return temp;
        }

        #endregion

        #region Create Event -- Select Resource(s) for event and put to resource cart

        //Treeview with Attributes and there Domain Constraints and other filter options
        public ActionResult ResourceFilter()
        {
            ResourceFilterModel model = new ResourceFilterModel();
            //if event creation starting point from ´date(time) in calender set date to clicked date
            if (Session["Filter"] != null)
            {
                Filter filter = (Filter)Session["Filter"];
                model.StartDate = filter.StartDate;
                model.EndDate = filter.EndDate;
                model.IsPreSelected = true;
            }
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

            return PartialView("_filterResources", model);
        }

        //fill grid view in select resources with resources
        public ActionResult LoadResources()
        {
            List<SelectResourceForEventModel> allResourceList = new List<SelectResourceForEventModel>();
            List<SelectResourceForEventModel> results = new List<SelectResourceForEventModel>();

            SingleResourceManager srManager = new SingleResourceManager();
            List<SingleResource> singelResources = new List<SingleResource>();
            singelResources = srManager.GetAllResources().ToList();
            singelResources.ForEach(r => allResourceList.Add(new SelectResourceForEventModel(r)));

            if (Session["Filter"] != null)
            {
                Filter filter = new Filter();
                filter = (Filter)Session["Filter"];
                results = ApplyFilter(allResourceList, filter);
            }
            else
                results = allResourceList;

            return PartialView("_gridResources", results);
        }

        public ActionResult SelectedResources()
        {
            List<ResourceCart> model = (List<ResourceCart>)Session["ResourceCart"];
            if (model == null)
                model = new List<ResourceCart>();

            return PartialView("_cartResources", model);
        }

        //Temp adding resource to event, schedules are not stored in this step
        public ActionResult AddResourceToCart(string resources)
        {
            if (resources == "")
            {
                ModelState.AddModelError("ResourceList", "You must select an resource to add it to the event.");
            }

            if (ModelState.IsValid)
            {
                List<ResourceCart> model = (List<ResourceCart>)Session["ResourceCart"];

                if (model == null)
                    model = new List<ResourceCart>();

                //get filter values
                Filter filter = new Filter();
                if (Session["Filter"] != null)
                {
                    filter = (Filter)Session["Filter"];
                }
                else
                {
                    filter.StartDate = new DateTime();
                    filter.EndDate = new DateTime();
                    filter.Quantity = 0;
                }

                //get all selected resource ids
                var rIds = resources.Split(',').Select(Int64.Parse).ToList();

                using (var srManager = new SingleResourceManager())
                using (var pManager = new PersonManager())
                {
                    UserManager userManager = new UserManager();

                    for (int i = 0; i < rIds.Count(); i++)
                    {
                        //set index for resource in cart
                        int index = 0;
                        if (model.Count() == 0)
                            index = 1;
                        else
                            index = model.Select(e => e.Index).Max() + 1;

                        SingleResource resource = srManager.GetResourceById(rIds[i]);
                        ResourceCart cartItem = new ResourceCart();

                        cartItem.Id = resource.Id;
                        //ToDo: User or Default
                        long createdByUserId = UserHelper.GetUserId(HttpContext.User.Identity.Name);

                        cartItem.ByPersonName = UserHelper.GetPartyByUserId(createdByUserId).Name;
                        cartItem.ByPersonUserId = createdByUserId;

                        cartItem.Name = resource.Name;
                        cartItem.Index = index;
                        cartItem.NewInCart = true;

                        if (filter.IsSet)
                        {
                            cartItem.PreselectdQuantity = filter.Quantity;
                            cartItem.PreselectedStartDate = filter.StartDate;
                            cartItem.PreselectedEndDate = filter.EndDate;
                        }

                        model.Add(cartItem);
                    }
                }
                //model.AddRange(temp);
                Session["ResourceCart"] = model;

                return PartialView("_cartResources", model);
            }
            else
            {
                return RedirectToAction("LoadResources");
            }

        }

        //public ActionResult OnChangeQuantityInCart(string quantity, string index)
        //{
        //    List<ResourceCart> model = (List<ResourceCart>)Session["ResourceCart"];
        //    ResourceCart tempCartItem = model.Where(m => m.Index == int.Parse(index)).FirstOrDefault();

        //    if (Session["Filter"] != null)
        //    {
        //        Filter filter = (Filter)Session["Filter"];
        //        int availableQuantity = GetAvailableQuantity(tempCartItem.Id, tempCartItem.ResourceQuantity, tempCartItem.PreselectedStartDate, tempCartItem.PreselectedEndDate, tempCartItem.PreselectdQuantity, 0);
        //        if ((availableQuantity - int.Parse(quantity)) <= 0)
        //        {
        //            ModelState.AddModelError(index, "Your requested number is not available at the timer period in the filter.");
        //        }
        //    }

        //    if (ModelState.IsValid)
        //    {
        //        tempCartItem.PreselectdQuantity = int.Parse(quantity);
        //        var i = model.FindIndex(p => p.Index == Convert.ToInt64(index));
        //        model[i] = tempCartItem;

        //        Session["ResourceCart"] = model;
        //    }

        //    return PartialView("_cartResources", model);

        //}

        public ActionResult RemoveResourceFromCart(string index)
        {
            List<ResourceCart> model = (List<ResourceCart>)Session["ResourceCart"];
            var i = model.FindIndex(p => p.Index == Convert.ToInt64(index));
            model.RemoveAt(i);
            Session["ResourceCart"] = model;

            return PartialView("_cartResources", model);
        }

        //für eventview später wiederverenden
        //Load schedule to event, if a new event the list is empty
        public ActionResult LoadSchedules()
        {
            EventModel model = (EventModel)Session["Event"];
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

        public ActionResult SetFilter(string startDate)
        {
            Session["Filter"] = null;
            if (startDate != null)
            {
                Filter filter = new Filter();
                filter.StartDate = DateTime.Parse(startDate);
                filter.EndDate = DateTime.Parse(startDate);
                Session["Filter"] = filter;
            }

            return new EmptyResult();
        }

        public ActionResult Create()
        {
            ViewBag.Title = PresentationModel.GetViewTitleForTenant("Create Event", this.Session.GetTenant());
            Session["ResourceCart"] = null;
            Session["Event"] = null;
            Session["FilterResults"] = null;

            //if (Session["Filter"] != null)
            //{
            //    Filter filter = (Filter)Session["Filter"];
            //    if (!filter.IsPredefined)
            //        Session["Filter"] = null;
            //}
          
            return View("SelectResources");
        }

        public ActionResult CreateEvent()
        {
            using (var partyManager = new PartyManager())
            using (var rManager = new SingleResourceManager())
            {
                List<ResourceCart> cart = (List<ResourceCart>)Session["ResourceCart"];
                if (cart == null)
                    ModelState.AddModelError("CartEmpty", "Please add a resource to the cart.");

                if (ModelState.IsValid)
                {
                    EventModel model = (EventModel)Session["Event"];

                    if (model == null)
                    {
                        model = new EventModel(cart);
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

                                s.ScheduleDurationModel.StartDate = rc.PreselectedStartDate;
                                s.ScheduleDurationModel.EndDate = rc.PreselectedEndDate;
                                s.ScheduleDurationModel.Index = rc.Index;
                                s.ScheduleDurationModel.EventId = model.Id;

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

                                //s.Index = rc.Index;
                                s.Index = model.Schedules.Count() + 2;
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

        ////get all activities and load in a grid
        //public ActionResult CreateScheduleActivities()
        //{
        //    ActivityManager aManager = new ActivityManager();
        //    List<Activity> activities = aManager.GetAllActivities().ToList();
        //    List<ActivityEventModel> aEventM = new List<ActivityEventModel>();
        //    activities.ForEach(r => aEventM.Add(new ActivityEventModel(r)));

        //   return PartialView("_enterActivities", aEventM);
        //}


        public ActionResult OnChangeEventItem(string value, string index, string element)
        {
            EventModel model = (EventModel)Session["Event"];
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
                        seModel = model.Schedules.Where(a => a.Index == int.Parse(index)).FirstOrDefault();
                        seModel.ScheduleDurationModel.StartDate = DateTime.Parse(value);
                        seModel.ScheduleDurationModel.EndDate = TimeHelper.GetEndDateOfDuration(seModel.ScheduleDurationModel.DurationValue, seModel.ScheduleDurationModel.TimeUnit, seModel.ScheduleDurationModel.StartDate);
                        //get index of modify schedule and update it in the session list
                        i = model.Schedules.FindIndex(p => p.Index == int.Parse(index));
                        model.Schedules[i] = seModel;
                        Session["Event"] = model;

                        return PartialView("_showTimePeriod", seModel.ScheduleDurationModel);
                    case "End":
                        seModel = model.Schedules.Where(a => a.Index == int.Parse(index)).FirstOrDefault();
                        seModel.ScheduleDurationModel.EndDate = DateTime.Parse(value);
                        //get index of modify schedule and update it in the session list
                        i = model.Schedules.FindIndex(p => p.Index == int.Parse(index));
                        model.Schedules[i] = seModel;
                        Session["Event"] = model;
                    break;
                    case "Quantity":
                        seModel = model.Schedules.Where(a => a.Index == int.Parse(index)).FirstOrDefault();
                        seModel.ScheduleQuantity = int.Parse(value);
                        //get index of modify schedule and update it in the session list
                        i = model.Schedules.FindIndex(p => p.Index == int.Parse(index));
                        model.Schedules[i] = seModel;
                        Session["Event"] = model;
                    break;

                }
            }

            return View("EditEvent", model);
        }

        public ActionResult SaveEvent()
        {
            EventModel model = (EventModel)Session["Event"];

            int countSchedulesCurrent = model.Schedules.Count();
            int countSchedulesBefor = 0;

            SingleResourceManager rManager = new SingleResourceManager();

            //Validate schedules ----------------------------------------------------------
            List<ScheduleEventModel> tempScheduleList = new List<ScheduleEventModel>();
            tempScheduleList.AddRange(model.Schedules);

            //set if one or more errors in event schedules
            bool isError = false;

            //check name
            if(model.Name == "" || model.Name == null)
            {
                ModelState.AddModelError("Name", "Name is required.");
                isError = true;
            }

            foreach (ScheduleEventModel s in tempScheduleList)
            {
                //set if error at this schedule
                bool sError = false;

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
                    int availableQuantity = GetAvailableQuantity(s.ResourceId, s.ResourceQuantity, s.ScheduleDurationModel.StartDate, s.ScheduleDurationModel.EndDate, s.ScheduleQuantity, s.ScheduleId);

                    if ((availableQuantity - s.ScheduleQuantity) < 0)
                    {
                        isError = true;
                        sError = true;
                        ModelState.AddModelError("Quantity_" + s.Index, "Selected number not available.");
                    }
                    if (s.ScheduleQuantity == 0)
                    {
                        isError = true;
                        sError = true;
                        ModelState.AddModelError("Quantity_" + s.Index, "The Quantity must be greater than 0");
                    }
                }

                //check if file which need to confirm attached to confirmation it set
                if (s.ResourceHasFiles)
                {
                    foreach(FileValueModel file in s.Files)
                    {
                        if (file.NeedConfirmation && !s.FileConfirmation)
                        {
                            ModelState.AddModelError("FileConfirmation_" + s.Index, "You must confirm that you read the file(s).");
                        }
                    }
                }

                //check Resource Constraints
                SingleResource r = rManager.GetResourceById(s.ResourceId);
                foreach (ResourceConstraint c in r.ResourceConstraints)
                {
                    if (c is DependencyConstraint)
                    {
                        DependencyConstraint dc = (DependencyConstraint)c;
                        ResourceConstraintData data = new ResourceConstraintData();

                        List<Schedule> temp = new List<Schedule>();
                        foreach(ScheduleEventModel t in model.Schedules)
                        {
                            Schedule tempS = new Rbm.Entities.Booking.Schedule();
                            tempS.Resource = rManager.GetResourceById(t.ResourceId);
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
                        data.PersonsInSchedule = s.ForPersons.Select(a=>a.UserId).ToList();
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
                UserManager userManager = new UserManager();

                using (var scheduleManager = new ScheduleManager())
                using (var singleResourceManager = new SingleResourceManager())
                using (var personManager = new PersonManager())
                using (var eventManager = new EventManager())
                using (var permissionManager = new EntityPermissionManager())
                using (var entityTypeManager = new EntityManager())
                {
                    Schedule newSchedule = new Schedule();

                    try
                    {
                        // get event min und max date from schedules

                        DateTime minDate = model.Schedules.Select(a => a.ScheduleDurationModel.StartDate).ToList().Min();
                        DateTime maxDate = model.Schedules.Select(a => a.ScheduleDurationModel.EndDate).ToList().Max();

                        //if event id = 0, create new event with note and description
                        BookingEvent eEvent = new BookingEvent();
                        if (model.Id == 0)
                            eEvent = eventManager.CreateEvent(model.Name, model.Description, null, minDate, maxDate);
                        else
                        {
                            eEvent = eventManager.GetEventById(model.Id);
                            countSchedulesBefor = scheduleManager.GetAllSchedulesByEvent(model.Id).Count();
                            eEvent.Name = model.Name;
                            eEvent.Description = model.Description;
                            eEvent.MinDate = minDate;
                            eEvent.MaxDate = maxDate;
                            eventManager.UpdateEvent(eEvent);
                        }

                        IndividualPerson createdBy = new IndividualPerson();
                        List<Notification> notifications = new List<Notification>();

                        //Create or update all schedules for this event
                        foreach (ScheduleEventModel schedule in model.Schedules)
                        {
                            //Delete deleted schedules in DB
                            foreach (long id in model.DeletedSchedules)
                            {
                                scheduleManager.DeleteSchedule(scheduleManager.GetScheduleById(id));
                            }

                            int index = 0;
                            //if(schedule.Index.HasValue)
                            index = schedule.Index;

                            SingleResource tempResource = singleResourceManager.GetResourceById(schedule.ResourceId);

                            //Users
                            List<User> users = new List<User>();

                            //Activities
                            List<Activity> activityList = new List<Activity>();
                            if (schedule.Activities.Count > 0)
                            {
                                ActivityManager aManager = new ActivityManager();
                                foreach (ActivityEventModel ae in schedule.Activities)
                                {
                                    activityList.Add(aManager.GetActivityById(ae.Id));
                                }
                            }

                            //reserved for
                            if (schedule.ForPersons.Count() > 1)
                            {
                                User contact = null;

                                foreach (PersonInSchedule user in schedule.ForPersons)
                                {
                                    User u = userManager.FindByIdAsync(user.Id).Result;
                                        
                                    if (user.IsContactPerson == true)
                                    {
                                        contact = u;
                                    }
                                    users.Add(u);
                                }

                                //Create or Update schedule
                                if (model.Id == 0 || (countSchedulesCurrent > countSchedulesBefor && schedule.ScheduleId == 0))
                                {
                                    User created = userManager.FindByNameAsync(HttpContext.User.Identity.Name).Result;
                                    createdBy = personManager.CreateIndividualPerson(created);
                                    PersonGroup pGroup = personManager.CreatePersonGroup(users, contact);
                                    newSchedule = scheduleManager.CreateSchedule(schedule.ScheduleDurationModel.StartDate, schedule.ScheduleDurationModel.EndDate, eEvent, tempResource, pGroup, createdBy, activityList, schedule.ScheduleQuantity, index);
                                }
                                else
                                {
                                    UpdateSchedule(schedule, activityList, users, contact);
                                }
                            }
                            else
                            {
                                User u = userManager.FindByIdAsync(schedule.ForPersons[0].UserId).Result;
                                IndividualPerson iPerson = personManager.CreateIndividualPerson(u);
                                if (model.Id == 0)
                                {
                                    User created = userManager.FindByNameAsync(HttpContext.User.Identity.Name).Result;
                                    createdBy = personManager.CreateIndividualPerson(created);
                                    newSchedule = scheduleManager.CreateSchedule(schedule.ScheduleDurationModel.StartDate, schedule.ScheduleDurationModel.EndDate, eEvent, tempResource, iPerson, createdBy, activityList, schedule.ScheduleQuantity, index);
                                }
                                else
                                {
                                    users.Add(u);
                                    newSchedule = UpdateSchedule(schedule, activityList, users, u);
                                }
                            }

                            //Get affected notificationen
                            List<Notification> temp = CheckNotificationForSchedule(schedule);
                            notifications.AddRange(temp);

                            //Add rights to the schedule for all user reserved for
                            if (model.Id == 0)
                            {
                                //get entities types
                                var entityTypeSchedule = entityTypeManager.FindByName("Schedule");
                                var entityTypeEvent = entityTypeManager.FindByName("Event");

                                //add rights to logged in user
                                var userId = UserHelper.GetUserId(HttpContext.User.Identity.Name);
                              
                                //rights on schedule 31 is the sum from all rights:  Read = 1, Download = 2, Write = 4, Delete = 8, Grant = 16
                                permissionManager.Create(userId, entityTypeSchedule.Id, newSchedule.Id, 31);

                                //rights on event
                                permissionManager.Create(userId, entityTypeSchedule.Id, eEvent.Id, 31);

                                foreach (PersonInSchedule user in schedule.ForPersons)
                                {
                                    User us = userManager.FindByIdAsync(user.UserId).Result;
                                    //rights on schedule 15 is the sum from this rights:  Read = 1, Download = 2, Write = 4, Delete = 8
                                    permissionManager.Create(us.Id, entityTypeSchedule.Id, newSchedule.Id, 15);
                                    //rights on event, Read = 1, Write = 4
                                    permissionManager.Create(us.Id, entityTypeSchedule.Id, newSchedule.Id, 5);
                                }
                            }

                            if (notifications.Count > 0)
                                SendNotification(notifications, model.Schedules);
                        }
                    }
                    catch (Exception e)
                    {
                        ModelState.AddModelError("error", e.Message);
                    }
                }
            }
            else
            {
                return View("EditEvent", model);
            }

                return View("Schedule");
        }

        private Schedule UpdateSchedule(ScheduleEventModel schedule, List<Activity> activityList, List<User> users, User contact)
        {
            PersonManager pManager = new PersonManager();
            ScheduleManager schManager = new ScheduleManager();

            Schedule s = new Schedule();
            s = schManager.GetScheduleById(schedule.ScheduleId);
            s.StartDate = schedule.ScheduleDurationModel.StartDate;
            s.EndDate = schedule.ScheduleDurationModel.EndDate;
            s.Quantity = schedule.ScheduleQuantity;

            //Update ForPerson
            if (users.Count() > 1)
            {
                PersonGroup pGroup = pManager.GetPersonGroupById(schedule.ForPersons.Select(a => a.Id).FirstOrDefault());
                pGroup.Users = users;
                pGroup.Contact = contact;
                pManager.UpdatePersonGroup(pGroup);
                s.ForPerson = pGroup;
            }
            else
            {
                IndividualPerson iPerson = pManager.GetIndividualPersonById(users[0].Id);
                pManager.UpdateIndividualPerson(iPerson);
                s.ForPerson = iPerson;
            }

            //Person pBy = pManager.GetIndividualPersonById(schedule.ByPerson.Id);
            //s.ByPerson = pBy;
            s.Activities = activityList;

            schManager.UpdateSchedule(s);

            return s;
        }

        #endregion

        #region Edit Event

        public ActionResult OpenEdit(string id)
        {
            EntityPermissionManager permissionManager = new EntityPermissionManager();
            SubjectManager subjectManager = new SubjectManager();

            EventModel model = (EventModel)Session["Event"];
            if (model != null)
            {
                model.EditMode = true;
                for (int i = 0; i < model.Schedules.Count();i++)
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
            EventManager eManager = new EventManager();
            BookingEvent e = eManager.GetEventById(id);
            List<ActivityEventModel> model = new List<ActivityEventModel>();
            e.Activities.ToList().ForEach(r => model.Add(new ActivityEventModel(r)));

            return PartialView("_enterActivities", model);
        }

        #endregion

        #region Edit Event -- TimePeriod

        public ActionResult ShowTimePeriodInSchedule(string index)
        {
            EventModel sEventM = (EventModel)Session["Event"];
            ScheduleEventModel tempSchedule = sEventM.Schedules.Where(a => a.Index == int.Parse(index)).FirstOrDefault();

            if (tempSchedule.ScheduleDurationModel != null)
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
            EventModel sEventM = (EventModel)Session["Event"];
            ScheduleEventModel tempSchedule = sEventM.Schedules.Where(a => a.Index == int.Parse(index)).FirstOrDefault();

            if (tempSchedule.ForPersons != null)
            {
                tempSchedule.ForPersons.ForEach(a =>
                {
                  a.EditAccess = tempSchedule.EditAccess;
                  a.EditMode = tempSchedule.EditMode;
                  Party partyPerson = UserHelper.GetPartyByUserId(a.UserId);
                  a.UserFullName = partyPerson.Name;
                    //get party type attribute value
                  a.MobileNumber = partyPerson.CustomAttributeValues.Where(b => b.CustomAttribute.Name == "Mobile").Select(v => v.Value).FirstOrDefault();
                 
                });
                return PartialView("_scheduleUsers", tempSchedule.ForPersons);
            }
            else
                return new EmptyResult();
        }

        //Load all user to select one or more for the event
        public ActionResult LoadUsers(string index)
        {
            //Session["ScheduleUsers"] = null;

            EventModel sEventM = (EventModel)Session["Event"];
            ScheduleEventModel tempSchedule = sEventM.Schedules.Where(a => a.Index == int.Parse(index)).FirstOrDefault();

            using (var partyManager = new PartyManager())
            using (var partyTypeManager = new PartyTypeManager())
            {

                UserManager userManager = new UserManager();

                //get party type person
                var partyType = partyTypeManager.PartyTypes.Where(p => p.Title == "person").FirstOrDefault();
                //get all parties with person party type
                List<Party> partyPersons = partyManager.PartyRepository.Query(p => p.PartyType == partyType).ToList();

                List<PersonInSchedule> personList = new List<PersonInSchedule>();

                List<long> tempUserIds = tempSchedule.ForPersons.Select(c => c.UserId).ToList();

                foreach (var partyPerson in partyPersons)
                {
                    var userTask = userManager.FindByIdAsync(partyManager.GetUserIdByParty(partyPerson.Id));
                    userTask.Wait();
                    var user = userTask.Result;

                    //check if user is contect person
                    bool isContact = false;
                    if (tempSchedule.Contact.UserId == user.Id)
                        isContact = true;

                    PersonInSchedule pu = new PersonInSchedule(0, user, isContact);
                    pu.Index = int.Parse(index);
                    pu.UserFullName = partyPerson.Name;

                    if (tempUserIds.Contains(user.Id))
                        pu.IsSelected = true;

                    personList.Add(pu);
                }

                //users.ForEach(r => personList.Add(new PersonUsersList(r)));

                return PartialView("_chooseUsers", personList);
            }
        }

        public ActionResult ChangeSelectedUser(string userId, string selected, string index)
        {
            //List<PersonInSchedule> sEventUser = (List<PersonInSchedule>)Session["ScheduleUsers"];
            EventModel model = (EventModel)Session["Event"];
            ScheduleEventModel tempSchedule = model.Schedules.Where(a => a.Index == int.Parse(index)).FirstOrDefault();

            //if (sEventUser == null)
            //    sEventUser = new List<PersonInSchedule>();

            using (var partyManager = new PartyManager())
            {
                UserManager userManager = new UserManager();
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

        public ActionResult ChangeContactPerson(string userId, string index)
        {
            //List<PersonInSchedule> sEventUser = (List<PersonInSchedule>)Session["ScheduleUsers"];

            EventModel model = (EventModel)Session["Event"];
            ScheduleEventModel tempSchedule = model.Schedules.Where(a => a.Index == int.Parse(index)).FirstOrDefault();

            if (tempSchedule.ForPersons.Where(a => a.IsContactPerson == true).Count() > 0)
            {
                //Message to user they allready exsist a contact person 
            }
            else
            {
                tempSchedule.ForPersons.First(d => d.UserId == Convert.ToInt64(userId)).IsContactPerson = true;
            }

            Session["Event"] = model;
            //Session["ScheduleUsers"] = sEventUser;

            return new EmptyResult();
        }

        //Temp adding user to schedule
        public ActionResult AddUsersToSchedule(string scheduleIndex)
        {
            //var users = userIds.Split(',').Select(Int64.Parse).ToList();

            //List<PersonInSchedule> sEventUser = (List<PersonInSchedule>)Session["ScheduleUsers"];

            EventModel eventM = (EventModel)Session["Event"];
            ScheduleEventModel tempSchedule = eventM.Schedules.Where(a => a.Index == int.Parse(scheduleIndex)).FirstOrDefault();

            SubjectManager subManager = new SubjectManager();

            //List<PersonUser> personGroup = new List<PersonUser>();
            var userIdsBefore = tempSchedule.ForPersons.Select(a => a.UserId).ToList();

            if (tempSchedule.ForPersons != null && tempSchedule.ForPersons.Count() > 1)
            {
                foreach (PersonInSchedule pUser in tempSchedule.ForPersons)
                {
                    //PersonUser pu = tempSchedule.ForPersons.Select(a=>a.UserId == userId).FirstOrDefault();
                    if (!userIdsBefore.Contains(pUser.UserId))
                    {
                        if (pUser.IsContactPerson == true)
                        {
                            tempSchedule.Contact = pUser;
                            tempSchedule.ContactName = UserHelper.GetPartyByUserId(pUser.Id).Name;
                        }
                        tempSchedule.ForPersons.Add(pUser);
                    }
                }
            }
            //else
            //{
            //    if (!userIdsBefore.Contains(sEventUser[0].UserId))
            //    {
            //        if (sEventUser[0].IsContactPerson == true)
            //        {
            //            tempSchedule.Contact = sEventUser[0];
            //            tempSchedule.ContactName = sEventUser[0].UserFullName;
            //        }
            //        tempSchedule.ForPersons.Add(sEventUser[0]);
            //    }
            //}

            //get index of modify schedule and update it in the session list
            var i = eventM.Schedules.FindIndex(p => p.Index == tempSchedule.Index);
            eventM.Schedules[i] = tempSchedule;
            Session["Event"] = eventM;

            return PartialView("_scheduleUsers", tempSchedule.ForPersons);
        }

        //open widow to show all users wich are related to one schedule
        public ActionResult ShowAllUsers(string scheduleIndex)
        {
            EventModel sEventM = (EventModel)Session["Event"];
            ScheduleEventModel tempSchedule = sEventM.Schedules.Where(a => a.Index == int.Parse(scheduleIndex)).FirstOrDefault();

            return PartialView("_showUsers", tempSchedule.ForPersons);
        }

        //Remove user from a schedule
        public ActionResult RemoveUserFromSchedule(string userId, string scheduleIndex)
        {
            EventModel sEventM = (EventModel)Session["Event"];
            ScheduleEventModel tempSchedule = sEventM.Schedules.Where(a => a.Index == int.Parse(scheduleIndex)).FirstOrDefault();
            tempSchedule.ForPersons.RemoveAll(a => a.UserId == int.Parse(userId));

            //get index of modify schedule and update it in the session list
            var i = sEventM.Schedules.FindIndex(p => p.Index == tempSchedule.Index);
            sEventM.Schedules[i] = tempSchedule;
            Session["Event"] = sEventM;

            return PartialView("_scheduleUsers", tempSchedule.ForPersons);
        }


        #endregion

        #region Edit Event -- Activities

        public ActionResult ShowActivityInSchedule(string index)
        {
            EventModel sEventM = (EventModel)Session["Event"];
            ScheduleEventModel tempSchedule = sEventM.Schedules.Where(a => a.Index == int.Parse(index)).FirstOrDefault();


            if (tempSchedule.Activities != null)
            {
                tempSchedule.Activities.ForEach(a => a.EditAccess = tempSchedule.EditAccess);
                return PartialView("_showActivities", tempSchedule.Activities);
            }
            else
                return new EmptyResult();
        }

        public ActionResult LoadActivities(string index)
        {
            Session["ScheduleActivities"] = null;

            List<ActivityEventModel> model = new List<ActivityEventModel>();
            ActivityManager aManager = new ActivityManager();
            List<Activity> activities = aManager.GetAllAvailableActivityById().ToList();

            EventModel sEventM = (EventModel)Session["Event"];

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

        public ActionResult ChangeSelectedActivity(string activityId, string selected, string index)
        {
            List<ActivityEventModel> model = (List<ActivityEventModel>)Session["ScheduleActivities"];
            if (model == null)
                model = new List<ActivityEventModel>();

            ActivityManager aManager = new ActivityManager();
            ActivityEventModel activity = new ActivityEventModel(aManager.GetActivityById(long.Parse(activityId)));

            if (selected == "true")
            {
                activity.Index = int.Parse(index);
                model.Add(activity);
            }
            else
            {
                int i = model.FindIndex(a => a.Id == long.Parse(activityId));
                model.RemoveAt(i);
            }

            Session["ScheduleActivities"] = model;

            return new EmptyResult();
        }

        //Temp adding user to schedule
        public ActionResult AddActivitiesToSchedule(string scheduleIndex)
        {

            List<ActivityEventModel> sScheduleActivities = (List<ActivityEventModel>)Session["ScheduleActivities"];
            if (sScheduleActivities == null)
                sScheduleActivities = new List<ActivityEventModel>();

            EventModel eventM = (EventModel)Session["Event"];
            ScheduleEventModel tempSchedule = eventM.Schedules.Where(a => a.Index == int.Parse(scheduleIndex)).FirstOrDefault();

            ActivityManager aManager = new ActivityManager();

            //List<PersonUser> personGroup = new List<PersonUser>();
            var activitiesIdsBefore = tempSchedule.Activities.Select(a => a.Id).ToList();

            if (sScheduleActivities != null && sScheduleActivities.Count() > 1)
            {
                foreach (ActivityEventModel ae in sScheduleActivities)
                {
                    //PersonUser pu = tempSchedule.ForPersons.Select(a=>a.UserId == userId).FirstOrDefault();
                    if (!activitiesIdsBefore.Contains(ae.Id))
                    {
                        tempSchedule.Activities.Add(ae);
                    }
                }
            }
            else
            {
                if (!activitiesIdsBefore.Contains(sScheduleActivities[0].Id))
                {
                    tempSchedule.Activities.Add(sScheduleActivities[0]);
                }
            }

            //get index of modify schedule and update it in the session list
            var i = eventM.Schedules.FindIndex(p => p.Index == tempSchedule.Index);
            eventM.Schedules[i] = tempSchedule;
            Session["Event"] = eventM;

            return PartialView("_scheduleActivities", tempSchedule.Activities);
        }

        //open window to show all activities wich are related to one schedule
        public ActionResult ShowAllActivities(string scheduleIndex)
        {
            EventModel sEventM = (EventModel)Session["Event"];
            ScheduleEventModel tempSchedule = sEventM.Schedules.Where(a => a.Index == int.Parse(scheduleIndex)).FirstOrDefault();

            return PartialView("_showActivities", tempSchedule.Activities);
        }

        //Remove user from a schedule
        public ActionResult RemoveActivityFromSchedule(string activityId, string scheduleIndex)
        {
            EventModel sEventM = (EventModel)Session["Event"];
            ScheduleEventModel tempSchedule = sEventM.Schedules.Where(a => a.Index == int.Parse(scheduleIndex)).FirstOrDefault();
            tempSchedule.Activities.RemoveAll(a => a.Id == long.Parse(activityId));

            //get index of modify schedule and update it in the session list
            var i = sEventM.Schedules.FindIndex(p => p.Index == tempSchedule.Index);
            sEventM.Schedules[i] = tempSchedule;
            Session["EventResources"] = sEventM;

            return View("_scheduleActivities", tempSchedule.Activities);
        }

        #endregion

        #region Edit Event -- Add Resources

        public ActionResult AddResource()
        {
            ViewBag.Title = PresentationModel.GetViewTitleForTenant("Add Resources", this.Session.GetTenant());

            EventModel model = (EventModel)Session["Event"];
            List<ResourceCart> resourceCart = new List<ResourceCart>();

            foreach (ScheduleEventModel s in model.Schedules)
            {
                ResourceCart cartItem = new ResourceCart();
                cartItem.Name = s.ResourceName;
                cartItem.PreselectedStartDate = s.ScheduleDurationModel.StartDate;
                cartItem.PreselectedEndDate = s.ScheduleDurationModel.EndDate;
                cartItem.PreselectdQuantity = s.ScheduleQuantity;
                cartItem.NewInCart = false;
                cartItem.Index = s.Index;
                resourceCart.Add(cartItem);
            }

            Session["ResourceCart"] = resourceCart;

            return View("SelectResources");
        }

        #endregion

        #region Edit Event -- Functions

        public ActionResult UseValuesForAllSchedules(string index)
        {
            EventModel model = (EventModel)Session["Event"];
            ScheduleEventModel tempSchedule = model.Schedules.Where(a => a.Index == int.Parse(index)).FirstOrDefault();

            foreach(ScheduleEventModel s in model.Schedules)
            {
                if(s.Index != int.Parse(index))
                {
                    s.ForPersons = tempSchedule.ForPersons;
                    s.Activities = tempSchedule.Activities;
                    s.ScheduleQuantity = tempSchedule.ScheduleQuantity;
                    s.ScheduleDurationModel = tempSchedule.ScheduleDurationModel;
                }
            }

            Session["Event"] = model;

            return PartialView("EditEvent", model);

        }

        public ActionResult RemoveScheduleFromEvent(string index)
        {
            EventModel model = (EventModel)Session["Event"];
            if (model.Schedules.Count() > 1)
            {
                var i = model.Schedules.FindIndex(p => p.Index == Convert.ToInt64(index));

                if(model.Schedules[i].ScheduleId != 0)
                    model.DeletedSchedules.Add(model.Schedules[i].ScheduleId);

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
            EventModel sEventM = (EventModel)Session["Event"];
            ScheduleEventModel tempSchedule = sEventM.Schedules.Where(a => a.Index == int.Parse(index)).FirstOrDefault();
            ScheduleEventModel newSchedule = new ScheduleEventModel(tempSchedule);
            newSchedule.Index = sEventM.Schedules.Count() + 1;
            sEventM.Schedules.Add(newSchedule);
            Session["Event"] = sEventM;

            return PartialView("EditEvent", sEventM);
        }

        private int GetQuantityLeftOnTime(long resourceId, DateTime startDate, DateTime endDate)
        {
            SingleResourceManager rManger = new SingleResourceManager();
            ScheduleManager schManager = new ScheduleManager();

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

        public bool CheckResourceAvailability(long id, string startDate, string endDate)
        {
            DateTime inputStart;
            DateTime.TryParse(startDate, out inputStart);
            DateTime inputEnd;
            DateTime.TryParse(endDate, out inputEnd);

            bool available = false;
            SingleResourceManager rManger = new SingleResourceManager();
            ScheduleManager schManager = new ScheduleManager();

            SingleResource resource = rManger.GetResourceById(id);
            List<Schedule> resourceSchedules = schManager.GetAllSchedulesByResource(id);
            List<Schedule> temp = new List<Rbm.Entities.Booking.Schedule>();
                foreach (Schedule sch in resourceSchedules)
                {
                    //if ((inputStart >= sch.StartDate && inputStart <= sch.EndDate) || (inputEnd >= sch.StartDate && inputEnd <= sch.EndDate))
                    if ((DateTime.Compare(inputStart.Date, sch.StartDate.Date) >= 0 && DateTime.Compare(inputStart.Date, sch.EndDate.Date) <= 0) || (DateTime.Compare(inputEnd.Date, sch.StartDate.Date) >= 0 && DateTime.Compare(inputEnd.Date, sch.EndDate.Date) <= 0))
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

            if(temp.Count()>0)
            {
                int bookedQuantity = 0;
                foreach(Schedule s in temp)
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

        //if resource not avaiable then open widow with alternatives
        public ActionResult LoadResourceAlternatives(string scheduleIndex)
        {
            EventModel sEventM = (EventModel)Session["Event"];
            ScheduleEventModel tempSchedule = sEventM.Schedules.Where(a => a.Index == int.Parse(scheduleIndex)).FirstOrDefault();

            AvailabilityScheduleModel model = new AvailabilityScheduleModel();
            model.CurrentResourceId = tempSchedule.ResourceId;
            model.CurrentResourceName = tempSchedule.ResourceName;
            model.Index = int.Parse(scheduleIndex);

            FindSimilarResources(tempSchedule.ResourceId).ToList().ForEach(r => model.AlternateResources.Add(new AlternateEventResource(r)));
            //remove this resource from the alternative list
            model.AlternateResources.RemoveAll(a => a.ResourceId == tempSchedule.ResourceId);
            return PartialView("_resourceAvailability", model);
        }

        //find similar resources for one resource based on there domain items
        private List<SingleResource> FindSimilarResources(long resourceId)
        {
            List<SingleResource> similarResources = new List<SingleResource>();
            List<string> DomainItems = new List<string>();

            SingleResourceManager srManager = new SingleResourceManager();
            ResourceStructureAttributeManager rsaManager = new ResourceStructureAttributeManager();
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
                            DomainItems.Add(value.Value);
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

        public ActionResult LoadSimilarResources(string index, string id)
        {
            EventModel sEventM = (EventModel)Session["Event"];

            //find similar resource to the sourceresource based on there domain constrains
            List<SingleResource> similarResources = FindSimilarResources(long.Parse(id));

            List<AlternateEventResource> model = new List<AlternateEventResource>();
            similarResources.ForEach(r => model.Add(new AlternateEventResource(r)));

            //remove the source resource from the result list
            ScheduleEventModel tempSchedule = sEventM.Schedules.Where(a => a.Index == int.Parse(index)).FirstOrDefault();
            model.RemoveAll(a => a.ResourceId == tempSchedule.ResourceId);

            return PartialView("_similarResources", model);
        }

        public ActionResult UseSimilarResources(List<AlternateEventResource> model)
        {
            EventModel sEventM = (EventModel)Session["Event"];
            SingleResourceManager srManager = new SingleResourceManager();

            foreach (AlternateEventResource r in model)
            {
                if (r.isChoosen == true)
                {
                    SingleResource sr = srManager.GetResourceById(r.ResourceId);
                    ScheduleEventModel tempSchedule = new ScheduleEventModel(sr);
                    tempSchedule.ScheduleDurationModel.StartDate = r.StartDate;
                    tempSchedule.ScheduleDurationModel.EndDate = r.EndDate;
                    tempSchedule.Index = sEventM.Schedules.Count() + 1;
                    sEventM.Schedules.Add(tempSchedule);
                }
            }

           
            Session["Event"] = sEventM;
            return Json(new { success = true });
        }


        public ActionResult UseAlternateResource(AvailabilityScheduleModel model, string chooseAlternateResource)
        {
            //List<ScheduleEventModel> sEventM = (List<ScheduleEventModel>)Session["EventResources"];

            EventModel sEventM = (EventModel)Session["Event"];

            try
            {
                AlternateEventResource a = model.AlternateResources.Where(d => d.ResourceId == long.Parse(chooseAlternateResource)).FirstOrDefault();
                SingleResourceManager srManager = new SingleResourceManager();
                SingleResource sr = srManager.GetResourceById(long.Parse(chooseAlternateResource));

                ScheduleEventModel tempSchedule = new ScheduleEventModel(sr);
                tempSchedule.ScheduleDurationModel.StartDate = a.StartDate;
                tempSchedule.ScheduleDurationModel.EndDate = a.EndDate;

                //Remove currentResource
                var i = sEventM.Schedules.FindIndex(p => p.Index == Convert.ToInt64(model.Index));
                sEventM.Schedules.RemoveAt(i);

                //give new event schedule a index
                tempSchedule.Index = sEventM.Schedules.Count() + 1;

                //Add alternative to event schedules
                sEventM.Schedules.Add(tempSchedule);
            }
            catch (Exception e)
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
            NotificationManager nManager = new NotificationManager();
            SingleResourceManager rManager = new SingleResourceManager();
            ResourceStructureAttributeManager rsaManager = new ResourceStructureAttributeManager();

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

        Expression<Func<NotificationDependency, bool>> MakePredicate(int id)
        {
            return u => u.Id == id;
        }

        private void SendNotification(List<Notification> notifications, List<ScheduleEventModel> schedules)
        {
            SubjectManager subManager = new SubjectManager();
            List<string> userToNotify = new List<string>();

            foreach (ScheduleEventModel s in schedules)
            {
                foreach (PersonInSchedule p in s.ForPersons)
                {
                    User u = subManager.Subjects.Where(a => a.Id == p.UserId).FirstOrDefault() as User;
                    if(!userToNotify.Contains(u.Email))
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
           
            SendNotificationHelper.SendNotification(userToNotify, "bexis@listserv.uni-jena.de", message, subject, true);

        }


        #endregion

        #region Event -- File Resource Structure Attribute Value

        public ActionResult DownloadFile(string id)
        {
            ResourceStructureAttributeManager valueManager = new ResourceStructureAttributeManager();
            FileValue value = valueManager.GetFileValueById(long.Parse(id));

            FileValue fv = (FileValue)value;
            byte[] contents = fv.Data;


            return File(contents, fv.Minmetype);
        }

        public ActionResult OnChangeFileConfirmation(string resourceId)
        {
            EventModel model = (EventModel)Session["Event"];
            long rId = long.Parse(resourceId);

            for(int i = 0; i<model.Schedules.Count(); i++)
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
            Session["ScheduleActivities"] = null;
            Session["Event"] = null;

            EventManager eManager = new EventManager();
            BookingEvent e = eManager.GetEventById(id);
            EventModel model = new EventModel(e);
            model.EditMode = false;

            EntityPermissionManager permissionManager = new EntityPermissionManager();
            //get permission from logged in user
            //model.EditAccess = permissionManager.HasUserDataAccess(HttpContext.User.Identity.Name, 6, id, RightType.Write);
            //model.DeleteAccess = permissionManager.HasUserDataAccess(HttpContext.User.Identity.Name, 6, id, RightType.Delete);

            model.Schedules.ForEach(a => a.EditAccess = model.EditAccess);
            model.Schedules.ForEach(a => a.DeleteAccess = model.DeleteAccess);


            Session["Event"] = model;

            return View("EditEvent", model);
        }

        //in show event view
        public ActionResult ShowActivities(long Id)
        {
            EventManager eManager = new EventManager();
            BookingEvent e = eManager.GetEventById(Id);
            return PartialView("_showActivities", new ShowEventModel(e));
        }

        //in show event view
        public ActionResult ShowSchedules(long Id)
        {
            EventManager eManager = new EventManager();
            BookingEvent e = eManager.GetEventById(Id);
            return PartialView("_showSchedules", new ShowEventModel(e));
        }


        #endregion

        #region Delete Event

        public ActionResult Delete(long id)
        {
            EventManager eManager = new EventManager();
            BookingEvent e = eManager.GetEventById(id);
          
            eManager.DeleteEvent(e);

            return RedirectToAction("Schedule");
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
                    a = diff / (Convert.ToDouble(model.DurationValue)*7);
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
                    a = diff / (Convert.ToDouble(model.DurationValue)*365);
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
            EventModel eventM = (EventModel)Session["Event"];

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

        private int GetAvailableQuantity(long resourceId, int resourceQuantity, DateTime startDate, DateTime endDate, int requestQuantity, long scheduleId)
        {
            int availableQuantity = 0;
            ScheduleManager schManager = new ScheduleManager();
            List<Schedule> allSchedules = schManager.GetAllSchedulesByResource(resourceId);
            int schedulesQuantity = 0;

            foreach (Schedule s in allSchedules)
            {
                if (s.Id != scheduleId)
                {
                    //get all schedule in the given time period
                    if ((DateTime.Compare(startDate, s.StartDate) >= 0 && DateTime.Compare(endDate, s.EndDate) <= 0) || (DateTime.Compare(endDate, s.StartDate) >= 0 && DateTime.Compare(startDate, s.EndDate) <= 0))
                    {
                        //Count all quantities in in time schedules to get schedulesQuantity
                        schedulesQuantity = schedulesQuantity + s.Quantity;
                    }
                }
            }

            availableQuantity = resourceQuantity - schedulesQuantity;

            //check schedules which are temporary in den Event
            EventModel eventM = (EventModel)Session["Event"];
            List<ScheduleEventModel> tempSchedules = eventM.Schedules.Where(a => a.ResourceId == resourceId).ToList();
            if (tempSchedules.Count > 1)
            {
                int tempScheduleQuantity = 0;
                foreach (ScheduleEventModel t in tempSchedules)
                {
                    //if (t.ResourceId != resourceId)
                    //{
                        if ((DateTime.Compare(startDate, t.ScheduleDurationModel.StartDate) >= 0 && DateTime.Compare(endDate, t.ScheduleDurationModel.EndDate) <= 0) || (DateTime.Compare(endDate, t.ScheduleDurationModel.StartDate) >= 0 && DateTime.Compare(startDate, t.ScheduleDurationModel.EndDate) <= 0))
                        {
                            tempScheduleQuantity = tempScheduleQuantity + t.ScheduleQuantity;
                        }
                    //}
                }
                availableQuantity = availableQuantity - tempScheduleQuantity;
            }

            return availableQuantity;
        }

        #endregion

        #region Validation - Constraints



        #endregion

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
                    if(!eventList.Select(a=>a.Id).ToList().Contains(s.Event.Id)) 
                        eventList.Add(eManager.GetEventById(s.Event.Id));
                }

            }
        
            List<object> eventObjectList = new List<object>();
            foreach (BookingEvent e in eventList)
            {
                eventObjectList.Add(new object[] { JsonHelper.JsonSerializer<CalendarItemsModel>(new CalendarItemsModel(e))});
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
                foreach(BookingEvent e in temp)
                {
                    List<Schedule> schedules = schManager.GetAllSchedulesByEvent(e.Id);
                    var s = schedules.Where(a=>a.ByPerson.Person.Id == user.Id).ToList();
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
                   if(s.ByPerson.Person.Id == user.Id)
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

                List<FilterTreeItem> filterList = new List<FilterTreeItem>();
                //split Id and DomainItem and add it to a FilterItem list
                foreach (string item in items)
                {
                    //index 0 = attrbute id, index1 domainvalue
                    List<string> i = item.Split('_').ToList();
                    FilterTreeItem filterItem = new FilterTreeItem();
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
                Dictionary<long, List<string>> filterDic = GetFilterDic(filterList);

                List<ResourceAttributeValueModel> temp = new List<ResourceAttributeValueModel>();

                //check for every TreeDomainModel (resource) if fits the filter
                foreach (ResourceAttributeValueModel m in treeDomainList)
                {
                    if (CheckTreeDomainModel(m, filterDic))
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

            if(bool.Parse(myBookings))
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
