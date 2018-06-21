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
using System.Web;
using System.Web.Mvc;
using Telerik.Web.Mvc;
using BExIS.Security.Entities.Subjects;
using BExIS.Security.Entities.Objects;
using Vaiona.Web.Mvc.Models;
using BExIS.Rbm.Entities.Users;
using BExIS.Web.Shell.Areas.RBM.Helpers;
using Vaiona.Web.Extensions;
using BExIS.Security.Entities.Authorization;

namespace BExIS.Web.Shell.Areas.RBM.Controllers
{
    public class NotificationController : Controller
    {
        #region Notification Management

        public ActionResult Notification()
        {
            ViewBag.Title = PresentationModel.GetViewTitleForTenant("Notification Manager", this.Session.GetTenant());
            return View("NotificationManager");
        }

        public ActionResult CreateN()
        {
            SingleResourceManager rManager = new SingleResourceManager();
            List<SingleResource> resources = rManager.GetAllResources().ToList();
            List<ResourceModel> rModelList = new List<ResourceModel>();
            resources.ToList().ForEach(r => rModelList.Add(new ResourceModel(r)));
            EditNotificationModel model = new EditNotificationModel(rModelList);
            Session["FilterOptions"] = model.AttributeDomainItems;
            return PartialView("_editNotification", model);
        }

        [HttpPost]
        public ActionResult Save(EditNotificationModel model)
        {
            if (ModelState.IsValid)
            {
                NotificationManager nManager = new NotificationManager();

                //if edit need comarison between stored dependensies and set in session
                //get resource filter from the notification
                Dictionary<long, List<string>> dictionary = (Dictionary<long, List<string>>)Session["ResourceFilter"];
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
                    foreach(NotificationDependency d in notification.NotificationDependency)
                    {
                        if (dictionary.ContainsKey(d.AttributeId))
                        {
                            if(dictionary[d.AttributeId].Contains(d.DomainItem))
                            {
                                //:)
                            }
                            else
                            {
                                nManager.DeleteNotificationDependency(d);
                            }

                        }
                        else
                        { 
                            //delete all dep. by id
                        }

                    }

                    //add dep
                    foreach(KeyValuePair<long, List<string>> kvp in dictionary)
                    {
                        long id = kvp.Key;
                        foreach (string value in kvp.Value)
                        {
                            if(!notification.NotificationDependency.Any(a=> a.AttributeId == id && a.DomainItem == value))
                                nManager.CreateNotificationDependency(notification, Convert.ToInt64(id), value);
                        }
                    }
                }


                if (notification.Id != 0 && model.Id == 0)
                {
                    //Start -> add security ----------------------------------------

                    EntityPermissionManager pManager = new EntityPermissionManager();
                    SubjectManager subManager = new SubjectManager();

                    User user = subManager.Subjects.Where(u => u.Name == HttpContext.User.Identity.Name).FirstOrDefault() as User;

                    foreach (RightType rightType in Enum.GetValues(typeof(RightType)).Cast<RightType>())
                    {
                        pManager.CreateDataPermission(user.Id, 5, notification.Id, rightType);
                    }

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
                Dictionary<long, List<string>> dictionary = (Dictionary<long, List<string>>)Session["ResourceFilter"];
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

        private List<Schedule> GetAffectedSchedules(Dictionary<long, List<string>> dictionary, DateTime startDate, DateTime endDate)
        {
            ScheduleManager sManager = new ScheduleManager();
            SingleResourceManager srManager = new SingleResourceManager();

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
                        userToNotify.Add(u.Email);
                    }
                }

                if(s.ForPerson.Self is IndividualPerson)
                {
                    IndividualPerson iPerson = (IndividualPerson)s.ForPerson.Self;
                    userToNotify.Add(iPerson.Person.Email);
                }
            }

            string subject = "Noreply: BExIS Resource Notification - " + notification.Subject;
            string message = "";
            message += "<b>" + notification.Subject + "</b><br/>";
            message += "Startdate:" + notification.StartDate + "<br/>";
            message += "EndDate:" + notification.EndDate + "<br/>";
            message += notification.Message + "<br/>";

            SendNotificationHelper.SendNotification(userToNotify, "bexis@listserv.uni-jena.de", message, subject, true);

        }

        public ActionResult Edit(long id)
        {
            NotificationManager nManager = new NotificationManager();
            Notification notification = nManager.GetNotificationById(id);

            SingleResourceManager rManager = new SingleResourceManager();
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

        public ActionResult Delete(long id)
        {
            NotificationManager nManager = new NotificationManager();
            Notification notification = nManager.GetNotificationById(id);

            bool deleted = nManager.DeleteNotification(notification);

            if (deleted)
            {
                //delete security 
                EntityPermissionManager pManager = new EntityPermissionManager();
                pManager.DeleteDataPermissionsByEntity(5, id);
            }

            return RedirectToAction("Notification");
        }

        private bool CheckTreeDomainModel(ResourceAttributeValueModel model, Dictionary<long, List<string>> filters)
        {
            foreach (KeyValuePair<long, List<string>> kp in filters)
            {
                if (IsResult(model, kp.Key, kp.Value) == false) return false;
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
            NotificationManager nManager = new NotificationManager();
            EntityPermissionManager permissionManager = new EntityPermissionManager();
            List<Notification> data = nManager.GetAllNotifications().ToList();

            List<NotificationModel> notifications = new List<NotificationModel>();

            foreach (Notification n in data)
            {
                NotificationModel temp = new NotificationModel(n);

                //get permission from logged in user
                temp.EditAccess = permissionManager.HasRight((HttpContext.User.Identity.Name, 5, n.Id, RightType.Write);
                temp.DeleteAccess = permissionManager.HasUserDataAccess(HttpContext.User.Identity.Name, 5, n.Id, RightType.Delete);

                notifications.Add(temp);
            }

            //data.ToList().ForEach(r => notifications.Add(new NotificationModel(r)));

            return View("NotificationManager", new GridModel<NotificationModel> { Data = notifications });
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

        #region Notification Blackboard

        public ActionResult Blackboard()
        {
            NotificationManager nManager = new NotificationManager();
            List<Notification> nList = nManager.GetAllNotifications().ToList();
            List<NotificationBlackboardModel> model = new List<NotificationBlackboardModel>();

            foreach (Notification n in nList)
            {
                model.Add(new NotificationBlackboardModel(n));
            }

            return View("NotificationBlackboard", model);
        }



        #endregion


    }
}
