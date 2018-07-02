using BExIS.Rbm.Entities.Booking;
using BExIS.Rbm.Services.Booking;
using BExIS.Security.Entities.Objects;
using BExIS.Security.Services.Authorization;
using BExIS.Security.Services.Subjects;
using BExIS.Security.Entities.Subjects;
using BExIS.Web.Shell.Areas.RBM.Models.Booking;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Telerik.Web.Mvc;
using Vaiona.Web.Mvc.Models;
using BExIS.Web.Shell.Areas.RBM.Helpers;
using Vaiona.Web.Extensions;
using BExIS.Security.Entities.Authorization;
using BExIS.Modules.RBM.UI.Helper;
using BExIS.Security.Services.Objects;

namespace BExIS.Modules.RBM.UI.Controllers
{
    public class ActivityController : Controller
    {

        public ActionResult Activity()
        {
            ViewBag.Title = PresentationModel.GetViewTitleForTenant("Manage Activities", this.Session.GetTenant());
            return View("ActivityManager");
        }


        public ActionResult CreateA()
        {
            ViewBag.Title = PresentationModel.GetViewTitleForTenant("Create Activity", this.Session.GetTenant());
            return PartialView("_createActivity", new ActivityModel());
        }

        //[HttpPost]
        public ActionResult Create(ActivityModel model)
        {
            ActivityManager aManager = new ActivityManager();

            //check name
            Activity temp = aManager.GetActivityByName(StringHelper.CutSpaces(model.Name));
            if(temp != null)
                ModelState.AddModelError("NameExist", "Name already exist");

            if (ModelState.IsValid)
            {
                Activity a = aManager.CreateActivity(model.Name, model.Description, model.Disable);

                //Start -> add security ----------------------------------------

                using (EntityPermissionManager pManager = new EntityPermissionManager())
                using (SubjectManager subManager = new SubjectManager())
                using (var entityTypeManager = new EntityManager())
                {

                    UserManager userManager = new UserManager();
                    var userTask = userManager.FindByNameAsync(HttpContext.User.Identity.Name);
                    userTask.Wait();
                    var user = userTask.Result;

                    Entity entityType = entityTypeManager.FindByName("Notification");

                    pManager.Create(user, entityType, a.Id, Enum.GetValues(typeof(RightType)).Cast<RightType>().ToList());

                    //End -> add security ------------------------------------------
                }

                    //return View("ActivityManager");
                    return Json(new { success = true });
                }
            else
            {
                    return PartialView("_createActivity", model);
            }
            
            
        }

        public ActionResult Edit(long id)
        {
            ViewBag.Title = PresentationModel.GetViewTitleForTenant("Edit Activity", this.Session.GetTenant());

            using (var aManager = new ActivityManager())
            {
                Activity activity = aManager.GetActivityById(id);

                return PartialView("_editActivity", new ActivityModel(activity));
            }
        }

        [HttpPost]
        public ActionResult Edit(ActivityModel model)
        {
            using (ActivityManager aManager = new ActivityManager())
            {

                //check name
                Activity temp = aManager.GetActivityByName(StringHelper.CutSpaces(model.Name));
                if (temp != null && temp.Id != model.Id)
                    ModelState.AddModelError("NameExist", "Name already exist");

                if (ModelState.IsValid)
                {
                    Activity activity = aManager.GetActivityById(model.Id);

                    if (activity != null)
                    {
                        activity.Name = model.Name;
                        activity.Description = model.Description;
                        activity.Disable = model.Disable;
                        aManager.UpdateActivity(activity);
                    }


                    //return View("ActivityManager");
                    return Json(new { success = true });
                }

                else
                {
                    return PartialView("_editActivity", model);
                }
            }
        }

        public ActionResult Delete(long id)
        {
             ActivityManager aManager = new ActivityManager();
             Activity activity = aManager.GetActivityById(id);

             if (activity != null)
             {
                 aManager.DeleteActivity(activity);
             }

             return View("ActivityManager");
        }


        [GridAction]
        public ActionResult Activity_Select()
        {
            using (var rManager = new ActivityManager())
            using (var permissionManager = new EntityPermissionManager())
            using(var entityTypeManager = new EntityManager())
            {
                List<Activity> data = rManager.GetAllActivities().ToList();
                List<ActivityModel> activities = new List<ActivityModel>();

                //get id from loged in user
                long userId = UserHelper.GetUserId(HttpContext.User.Identity.Name);
                //get entity type id
                long entityTypeId = entityTypeManager.FindByName("Activity").Id;

                foreach (Activity a in data)
                {
                    ActivityModel temp = new ActivityModel(a);
                    temp.InUse = rManager.IsInEvent(a.Id);

                    //get permission from logged in user
                    temp.EditAccess = permissionManager.HasEffectiveRight(userId, entityTypeId, a.Id, RightType.Read);
                    temp.DeleteAccess = permissionManager.HasEffectiveRight(userId, entityTypeId, a.Id, RightType.Delete);

                    activities.Add(temp);
                }
                //data.ToList().ForEach(r => activities.Add(new ActivityModel(r)));

                return View("ActivityManager", new GridModel<ActivityModel> { Data = activities });
            }
        }

    }
}
