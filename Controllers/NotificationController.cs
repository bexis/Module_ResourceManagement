using BExIS.Rbm.Entities.Booking;
using BExIS.Rbm.Entities.Resource;
using BExIS.Rbm.Services.Booking;
using BExIS.Rbm.Services.Resource;
using BExIS.Security.Services.Authorization;
using BExIS.Security.Services.Subjects;
using BExIS.Web.Shell.Areas.RBM.Models.Booking;
using BExIS.Web.Shell.Areas.RBM.Models.Resource;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using Telerik.Web.Mvc;
using BExIS.Security.Entities.Subjects;
using BExIS.Security.Entities.Objects;
using Vaiona.Web.Mvc.Models;
using BExIS.Rbm.Entities.Users;
using BExIS.Web.Shell.Areas.RBM.Helpers;
using Vaiona.Web.Extensions;
using BExIS.Security.Entities.Authorization;
using BExIS.Modules.RBM.UI.Helper;
using BExIS.Security.Services.Objects;
using Vaiona.Web.Mvc.Modularity;

namespace BExIS.Modules.RBM.UI.Controllers
{
    public class NotificationController : Controller
    {
        #region Notification Management

        public ActionResult Notification()
        {
            ViewBag.Title = PresentationModel.GetViewTitleForTenant("Notification Manager", this.Session.GetTenant());
            List<NotificationModel> model = new List<NotificationModel>();

            using (var nManager = new NotificationManager())
            using (var permissionManager = new EntityPermissionManager())
            using (var entityTypeManager = new EntityManager())
            {
                List<Notification> data = nManager.GetAllNotifications().ToList();

                //get id from loged in user
                long userId = UserHelper.GetUserId(HttpContext.User.Identity.Name);
                //get entity type id
                long entityTypeId = entityTypeManager.FindByName("Notification").Id;

                foreach (Notification n in data)
                {
                    NotificationModel temp = new NotificationModel(n);

                    //get permission from logged in user
                    temp.EditAccess = permissionManager.HasEffectiveRight(userId, new List<long>() { entityTypeId }, n.Id, RightType.Write);
                    temp.DeleteAccess = permissionManager.HasEffectiveRight(userId, new List<long>() { entityTypeId }, n.Id, RightType.Delete);

                    model.Add(temp);
                }
            }

                return View("NotificationManager", model);
        }

        public ActionResult CreateNotification()
        {
            using (ResourceManager rManager = new ResourceManager())
            {
                List<SingleResource> resources = rManager.GetAllResources().ToList();
                List<ResourceModel> rModelList = new List<ResourceModel>();
                resources.ToList().ForEach(r => rModelList.Add(new ResourceModel(r)));
                EditNotificationModel model = new EditNotificationModel(rModelList);
                Session["FilterOptions"] = model.AttributeDomainItems;
                Session["ResourceFilter"] = null;
                return PartialView("_editNotification", model);
            } 
        }

        [HttpPost]
        public ActionResult Save(EditNotificationModel model)
        {
            using (NotificationManager nManager = new NotificationManager())
            using (EntityPermissionManager pManager = new EntityPermissionManager())
            using (EntityManager entityTypeManager = new EntityManager())
            using (UserManager userManager = new UserManager())
            {
                Dictionary<long, List<string>> dictionary = (Dictionary<long, List<string>>)Session["ResourceFilter"];
                if (ModelState.IsValid && dictionary != null)
                {

                    //if edit need comarison between stored dependensies and set in session
                    //get resource filter from the notification
                    //Dictionary<long, List<string>> dictionary = (Dictionary<long, List<string>>)Session["ResourceFilter"];
                    Session["ResourceFilter"] = null;

                    List<Schedule> affectedSchedules = GetAffectedSchedules(dictionary, model.StartDate, model.EndDate);

                    Notification notification = new Rbm.Entities.Booking.Notification();
                    if (model.Id == 0)
                    {
                        notification = nManager.CreateNotification(model.Subject, model.StartDate, model.EndDate, model.Message);
                    }
                    else
                    {
                        notification = nManager.GetNotificationById(model.Id);
                        notification.Subject = model.Subject;
                        notification.StartDate = model.StartDate;
                        notification.EndDate = model.EndDate;
                        notification.Message = model.Message;
                        nManager.UpdateNotification(notification);
                    }

                    //save or update dependencies 
                    if (model.Id == 0)
                    {
                        foreach (KeyValuePair<long, List<string>> kp in dictionary)
                        {
                            foreach (string value in kp.Value)
                            {
                                nManager.CreateNotificationDependency(notification, Convert.ToInt64(kp.Key), value);
                            }
                        }
                    }
                    else
                    {
                        //remove dep.
                        for(int i = 0; i < notification.NotificationDependency.Count(); i++)
                        //foreach (NotificationDependency d in notification.NotificationDependency)
                        {
                            if (dictionary.ContainsKey(notification.NotificationDependency.ToList()[i].AttributeId))
                            {
                                if (dictionary[notification.NotificationDependency.ToList()[i].AttributeId].Contains(notification.NotificationDependency.ToList()[i].DomainItem))
                                {
                                    //:)
                                }
                                else
                                {
                                    notification.NotificationDependency.Remove(notification.NotificationDependency.ToList()[i]);
                                    nManager.UpdateNotification(notification);
                                    //nManager.DeleteNotificationDependency(d);
                                }

                            }
                            else
                            {
                                //delete all dependencies
                                notification.NotificationDependency.Clear();
                                nManager.UpdateNotification(notification);
                            }

                        }

                        //add dep
                        foreach (KeyValuePair<long, List<string>> kvp in dictionary)
                        {
                            long id = kvp.Key;
                            foreach (string value in kvp.Value)
                            {
                                if (!notification.NotificationDependency.Any(a => a.AttributeId == id && a.DomainItem == value))
                                    nManager.CreateNotificationDependency(notification, Convert.ToInt64(id), value);
                            }
                        }
                    }

                    if (notification.Id != 0 && model.Id == 0)
                    {
                        //Start -> add security ----------------------------------------

                        //31 is the sum from all rights:  Read = 1,  Write = 4, Delete = 8, Grant = 16
                        int fullRights = (int)RightType.Read + (int)RightType.Write + (int)RightType.Delete + (int)RightType.Grant;
                        Entity entityType = entityTypeManager.FindByName("Notification");

                        //get admin groups: format= "groupname:resource structure attribute value"
                        // give rights to group if fetch to notification
                        var settings = ModuleManager.GetModuleSettings("rbm");
                        string[] eventAdminGroups = settings.GetValueByKey("EventAdminGroups").ToString().Split(',');
                        Dictionary<string, string> adminGroupsDictionary = new Dictionary<string, string>();
                        if (eventAdminGroups != null && eventAdminGroups.Length > 0)
                        {
                            foreach (string group in eventAdminGroups)
                            {
                                string[] groupPair = group.Split(':');
                                adminGroupsDictionary.Add(groupPair[0], groupPair[1]);
                            }
                        }

                        //get resource structrue attribute values to compare with admin group settings
                        if (adminGroupsDictionary.Count > 0)
                        {
                            //get admin groups for notification
                            var values = dictionary.SelectMany(pair => pair.Value).ToList();
                            var adminGroups = adminGroupsDictionary
                                                .Where(pair => values.Contains(pair.Value))
                                                .Select(pair => pair.Key)
                                                .ToList();
                            using (var groupManager = new GroupManager())
                            {
                                foreach (var g in adminGroups)
                                {
                                    var group = groupManager.FindByNameAsync(g).Result;
                                    if (group != null)
                                    {
                                        if (pManager.GetRights(group.Id, entityType.Id, notification.Id) == 0)
                                            pManager.Create(group.Id, entityType.Id, notification.Id, fullRights);
                                    }
                                }
                            }
                        }

                        //rights to bexcis admin group
                        using (var groupManager = new GroupManager())
                        {
                            var adminGroup = groupManager.FindByNameAsync("administrator").Result;
                            if (pManager.GetRights(adminGroup.Id, entityType.Id, notification.Id) == 0)
                                pManager.Create(adminGroup.Id, entityType.Id, notification.Id, fullRights);

                        }

                         //rights to user that has create the notification
                         var userTask = userManager.FindByNameAsync(HttpContext.User.Identity.Name);
                         userTask.Wait();
                         var user = userTask.Result;
                         pManager.Create(user, entityType, notification.Id, fullRights);
                        

                        //End -> add security ------------------------------------------
                    }

                    //ToDo: Send email with notification to all in a affected schedule involved people
                    if (affectedSchedules.Count > 0)
                    {
                        SendNotification(notification, affectedSchedules);
                    }

                    //return View("NotificationManager");
                    return Json(new { success = true });
                }
                else
                {
                    List<AttributeDomainItemsModel> attributeDomainItems = (List<AttributeDomainItemsModel>)Session["FilterOptions"];
                    //Dictionary<long, List<string>> dictionary = (Dictionary<long, List<string>>)Session["ResourceFilter"];
                    if (dictionary != null)
                    {
                        foreach (AttributeDomainItemsModel m in attributeDomainItems)
                        {
                            for (int i = 0; i < m.DomainItems.Count(); i++)
                            {
                                var list = dictionary.SelectMany(x => x.Value);
                                if (list.Contains(m.DomainItems[i].Key))
                                {
                                    m.DomainItems[i].Selected = true;
                                }
                            }
                        }
                    }

                    model.AttributeDomainItems = attributeDomainItems;
                    return PartialView("_editNotification", model);
                }
            }
        }

        private List<Schedule> GetAffectedSchedules(Dictionary<long, List<string>> dictionary, DateTime startDate, DateTime endDate)
        {
            using (ScheduleManager sManager = new ScheduleManager())
            using (ResourceManager srManager = new ResourceManager())
            {

                List<ResourceAttributeValueModel> resourceAttributeValueModels = new List<ResourceAttributeValueModel>();

                //get all resources
                List<SingleResource> resources = srManager.GetAllResources().ToList();

                //Create for each Resource ResourceAttributeValueModel witch includes all Attribute Ids and all values
                foreach (SingleResource r in resources)
                {
                    ResourceAttributeValueModel treeDomainModel = new ResourceAttributeValueModel(r);
                    resourceAttributeValueModels.Add(treeDomainModel);
                }

                List<ResourceAttributeValueModel> temp = new List<ResourceAttributeValueModel>();
                List<ResourceModel> resultResourceList = new List<ResourceModel>();

                //check for every TreeDomainModel (resource) if fits the filter
                foreach (ResourceAttributeValueModel m in resourceAttributeValueModels)
                {
                    if (CheckTreeDomainModel(m, dictionary))
                        resultResourceList.Add(new ResourceModel(m.Resource));
                }

                //get all schedules in the selected time period
                List<Schedule> tempSchedules = sManager.GetSchedulesBetweenStartAndEndDate(startDate, endDate);
                List<Schedule> affectedSchedules = new List<Schedule>();

                //go through all resource which have the selected filter a check if there are schedules
                foreach (ResourceModel resource in resultResourceList)
                {
                    List<Schedule> s = tempSchedules.Where(a => a.Resource.Id == resource.Id).ToList();
                    if (s.Count() > 0)
                        affectedSchedules.AddRange(s);
                }

                return affectedSchedules;
            }
        }

        private void SendNotification(Notification notification, List<Schedule> affectedSchedules)
        {
            List<string> userToNotify = new List<string>();
            foreach (Schedule s in affectedSchedules)
            {
                if (s.ForPerson.Self is PersonGroup)
                {
                    PersonGroup pGroup = (PersonGroup)s.ForPerson.Self;
                    foreach(User u in pGroup.Users)
                    {
                        if (!userToNotify.Contains(u.Email))
                            userToNotify.Add(u.Email);
                    }
                }

                if(s.ForPerson.Self is IndividualPerson)
                {
                    IndividualPerson iPerson = (IndividualPerson)s.ForPerson.Self;
                    if(!userToNotify.Contains(iPerson.Person.Email))
                        userToNotify.Add(iPerson.Person.Email);
                }
            }

            string subject = "Resource Notification - " + notification.Subject;
            string message = "";
            message += "<b>" + notification.Subject + "</b><br/><br/>";
            message += "Startdate: " + notification.StartDate.ToString("yyyy-MM-dd") + "<br/>";
            message += "EndDate: " + notification.EndDate.ToString("yyyy-MM-dd") + "<br/><br/>";
            message += "<p>" + notification.Message + "</p>" + "<br/>";

            SendNotificationHelper.SendNotification(userToNotify, message, subject);
        }

        public ActionResult Edit(long id)
        {
            using (NotificationManager nManager = new NotificationManager())
            using (ResourceManager rManager = new ResourceManager())
            {
                Notification notification = nManager.GetNotificationById(id);

                List<SingleResource> resources = rManager.GetAllResources().ToList();
                List<ResourceModel> rModelList = new List<ResourceModel>();
                resources.ToList().ForEach(r => rModelList.Add(new ResourceModel(r)));

                EditNotificationModel model = new EditNotificationModel(rModelList, notification);

                //add dependensies to Session
                Dictionary<long, List<string>> newDictionary = new Dictionary<long, List<string>>();
                foreach (NotificationDependency d in notification.NotificationDependency)
                {
                    if (newDictionary.ContainsKey(d.AttributeId))
                    {
                        List<string> tmp = (List<string>)newDictionary[d.AttributeId];
                        if (!tmp.Contains(d.DomainItem)) tmp.Add(d.DomainItem);

                        //newDictionary[d.AttributeId] = tmp;
                    }
                    else
                    {
                        newDictionary.Add(d.AttributeId, new List<string>() { d.DomainItem });
                    }
                }

                Session["ResourceFilter"] = newDictionary;

                return PartialView("_editNotification", model);
            }
             
        }

        public ActionResult Delete(long id)
        {
            using (var nManager = new NotificationManager())
            using (var permissionManager = new EntityPermissionManager())
            using (var entityTypeManager = new EntityManager())
            {
                Notification notification = nManager.GetNotificationById(id);

                bool deleted = nManager.DeleteNotification(notification);

                if (deleted)
                {
                    Type entityType = entityTypeManager.FindByName("Notification").EntityType;
                    //delete security 
                    permissionManager.Delete(entityType, id);
                }
            }

            return RedirectToAction("Notification");
        }

        private bool CheckTreeDomainModel(ResourceAttributeValueModel model, Dictionary<long, List<string>> filters)
        {
            
            if (filters != null){
                foreach (KeyValuePair<long, List<string>> kp in filters)
                {
                    if (IsResult(model, kp.Key, kp.Value) == false) return false;
                }
            }
            return true;
        }

        //checks if is a filter result
        private bool IsResult(ResourceAttributeValueModel model, long id, List<string> values)
        {
            bool temp = false;

            foreach (string value in values)
            {
                //int index = model.AttributeIds.IndexOf(id);

                //if (model.Values.ElementAt(index).Equals(value))
                if (model.Values.Contains(value))
                {
                    temp = true;
                }
            }

            return temp;
        }


        [GridAction]
        public ActionResult Notification_Select()
        {
            using (var nManager = new NotificationManager())
            using (var permissionManager = new EntityPermissionManager())
            using (var entityTypeManager = new EntityManager())
            {
                List<Notification> data = nManager.GetAllNotifications().ToList();
                List<NotificationModel> notifications = new List<NotificationModel>();

                //get id from loged in user
                long userId = UserHelper.GetUserId(HttpContext.User.Identity.Name);
                //get entity type id
                long entityTypeId = entityTypeManager.FindByName("Notification").Id;

                foreach (Notification n in data)
                {
                    NotificationModel temp = new NotificationModel(n);

                    //get permission from logged in user
                    temp.EditAccess = permissionManager.HasEffectiveRight(userId, new List<long>() { entityTypeId }, n.Id, RightType.Write);
                    temp.DeleteAccess = permissionManager.HasEffectiveRight(userId, new List<long>() { entityTypeId }, n.Id, RightType.Delete);

                    notifications.Add(temp);
                }

                //data.ToList().ForEach(r => notifications.Add(new NotificationModel(r)));

                return View("NotificationManager", new GridModel<NotificationModel> { Data = notifications });
            }
        }

        //Add all for notification affected resource filter (by domain constraints on resource attributes) to a Dictionary in a session
        public JsonResult ChangeResourceFilter(string stringKey, string value, bool isChecked)
        {
            long key = Convert.ToInt64(stringKey);

            if (Session["ResourceFilter"] != null)
            {
                Dictionary<long, List<string>> dictionary = (Dictionary<long, List<string>>)Session["ResourceFilter"];

                if (dictionary.ContainsKey(key))
                {
                    if (isChecked)
                        dictionary[key].Add(value);
                    else
                    {
                        List<string> temp = (List<string>)dictionary[key];
                        if (temp.Contains(value))
                        {
                            temp.Remove(value);
                            if (temp.Count() == 0)
                                dictionary.Remove(key);
                        }
                    }
                }
                else
                {
                    List<string> values = new List<string>();
                    values.Add(value);
                    dictionary.Add(key, values);
                }

                Session["ResourceFilter"] = dictionary;
            }
            else
            {
                Dictionary<long, List<string>> newDictionary = new Dictionary<long, List<string>>();
                List<string> values = new List<string>();
                values.Add(value);
                newDictionary.Add(key, values);
                Session["ResourceFilter"] = newDictionary;
            }

            return Json(true, JsonRequestBehavior.AllowGet);
        }

        #endregion
  
    }
}
