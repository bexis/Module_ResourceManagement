using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using BExIS.Modules.RBM.UI.Helper;
using BExIS.Rbm.Entities.Booking;
using BExIS.Rbm.Services.Booking;
using BExIS.Security.Entities.Authorization;
using BExIS.Security.Entities.Objects;
using BExIS.Security.Services.Authorization;
using BExIS.Security.Services.Objects;
using BExIS.Security.Services.Subjects;
using BExIS.Web.Shell.Areas.RBM.Helpers;
using BExIS.Web.Shell.Areas.RBM.Models.Booking;
using Telerik.Web.Mvc;
using Vaiona.Web.Extensions;
using Vaiona.Web.Mvc.Models;

namespace BExIS.Modules.RBM.UI.Controllers
{
    public class ActivityController : Controller
    {
        public ActionResult Activity()
        {
            ViewBag.Title = PresentationModel.GetViewTitleForTenant("Manage Activities", Session.GetTenant());
            List<ActivityModel> model = new List<ActivityModel>();

            using (var rManager = new ActivityManager())
            using (var permissionManager = new EntityPermissionManager())
            using (var entityTypeManager = new EntityManager())
            {
                List<Activity> data = rManager.GetAllActivities().ToList();

                // get id from loged in user
                long userId = UserHelper.GetUserId(HttpContext.User.Identity.Name);

                // get entity type id
                long entityTypeId = entityTypeManager.FindByName("Activity").Id;

                foreach (Activity a in data)
                {
                    ActivityModel temp = new ActivityModel(a);
                    // temp.InUse = rManager.IsInEvent(a.Id);

                    // get permission from logged in user
                    temp.EditAccess = permissionManager.HasEffectiveRight(userId, new List<long> { entityTypeId }, a.Id, RightType.Read);
                    temp.DeleteAccess = permissionManager.HasEffectiveRight(userId, new List<long> { entityTypeId }, a.Id, RightType.Delete);

                    model.Add(temp);
                }
            }
                return View("ActivityManager", model);
        }

        public ActionResult CreateActivity()
        {
            ViewBag.Title = PresentationModel.GetViewTitleForTenant("Create Activity", Session.GetTenant());

            return PartialView("_createActivity", new ActivityModel());
        }

        // [HttpPost]
        public ActionResult Create(ActivityModel model)
        {
            using (ActivityManager aManager = new ActivityManager())
            {
                // check name
                Activity temp = aManager.GetActivityByName(StringHelper.CutSpaces(model.Name));
                if (temp != null)
                {
                    ModelState.AddModelError("NameExist", "Name already exist");
                }

                if (ModelState.IsValid)
                {
                    Activity a = aManager.CreateActivity(model.Name, model.Description, model.Disable);

                    // Start -> add security ----------------------------------------
                    using (EntityPermissionManager pManager = new EntityPermissionManager())
                    using (SubjectManager subManager = new SubjectManager())
                    using (var entityTypeManager = new EntityManager())
                    using (UserManager userManager = new UserManager())
                    {
                        var userTask = userManager.FindByNameAsync(HttpContext.User.Identity.Name);
                        userTask.Wait();
                        var user = userTask.Result;

                        Entity entityType = entityTypeManager.FindByName("Activity");

                        // 31 is the sum from all rights:  Read = 1, Write = 4, Delete = 8, Grant = 16
                        int rights = (int)RightType.Read + (int)RightType.Write + (int)RightType.Delete + (int)RightType.Grant;
                        pManager.Create(user, entityType, a.Id, rights);

                        // End -> add security ------------------------------------------
                    }

                    // return View("ActivityManager");
                    return Json(new { success = true });
                }
                else
                {
                    return PartialView("_createActivity", model);
                }
            }
        }

        public ActionResult Edit(long id)
        {
            ViewBag.Title = PresentationModel.GetViewTitleForTenant("Edit Activity", Session.GetTenant());

            using (var aManager = new ActivityManager())
            {
                Activity activity = aManager.GetActivityById(id);

                return PartialView("_editActivity", new ActivityModel(activity));
            }
        }

        [HttpPost]
        [ValidateInput(true)]
        public ActionResult Edit(ActivityModel model)
        {
            using (ActivityManager aManager = new ActivityManager())
            {
                // check name
                Activity temp = aManager.GetActivityByName(StringHelper.CutSpaces(model.Name));
                if (temp != null && temp.Id != model.Id)
                {
                    ModelState.AddModelError("NameExist", "Name already exist");
                }

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

                // return View("ActivityManager");
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
            using (ActivityManager aManager = new ActivityManager())
            {
                Activity activity = aManager.GetActivityById(id);

                if (activity != null)
                {
                    aManager.DeleteActivity(activity);
                }
            }

            return View("ActivityManager");
        }
    }
}
