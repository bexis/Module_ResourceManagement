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

namespace BExIS.Web.Shell.Areas.RBM.Controllers
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

                EntityPermissionManager pManager = new EntityPermissionManager();
                SubjectManager subManager = new SubjectManager();

                User user = subManager.Subjects.Where(u=>u.Name == HttpContext.User.Identity.Name).FirstOrDefault() as User;

                foreach (RightType rightType in Enum.GetValues(typeof(RightType)).Cast<RightType>())
                {
                    //pManager.CreateDataPermission(user.Id, 7, a.Id, rightType);
                }

                //End -> add security ------------------------------------------

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
            ActivityManager aManager = new ActivityManager();
            Activity activity = aManager.GetActivityById(id);

            return PartialView("_editActivity", new ActivityModel(activity));
        }

        [HttpPost]
        public ActionResult Edit(ActivityModel model)
        {
            ActivityManager aManager = new ActivityManager();

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
            ActivityManager rManager = new ActivityManager();
            EntityPermissionManager permissionManager = new EntityPermissionManager();

            List<Activity> data = rManager.GetAllActivities().ToList();

            List<ActivityModel> activities = new List<ActivityModel>();
            foreach (Activity a in data)
            {
                ActivityModel temp = new ActivityModel(a);
                temp.InUse = rManager.IsInEvent(a.Id);

                //get permission from logged in user
                temp.EditAccess = permissionManager.HasUserDataAccess(HttpContext.User.Identity.Name, 7, a.Id, RightType.Read);
                temp.DeleteAccess = permissionManager.HasUserDataAccess(HttpContext.User.Identity.Name, 7, a.Id, RightType.Delete);

                activities.Add(temp);
            }
            //data.ToList().ForEach(r => activities.Add(new ActivityModel(r)));

            return View("ActivityManager", new GridModel<ActivityModel> { Data = activities });
        }

    }
}
