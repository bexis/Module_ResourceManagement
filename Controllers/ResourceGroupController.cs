using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using BExIS.Web.Shell.Areas.RBM.Models.Resource;
using BExIS.Rbm.Services.Resource;
using BExIS.Rbm.Entities.Resource;
using Telerik.Web.Mvc;
using Vaiona.Web.Mvc.Models;
using Vaiona.Web.Extensions;

namespace BExIS.Modules.RBM.UI.Controllers
{
    public class ResourceGroupController : Controller
    {
        //
        // GET: /RBM/ResourceSet/

        public ActionResult ResourceGroup()
        {
            ViewBag.Title = PresentationModel.GetViewTitleForTenant("Manage Resource Group", this.Session.GetTenant());
            return View("ResourceGroupManager");
        }

        public ActionResult Create()
        {
            CreateResourceGroupModel model = new CreateResourceGroupModel();
            return View("CreateResourceGroup", model);
        }

        [HttpPost]
        public ActionResult Create(CreateResourceGroupModel model)
        {
           ViewBag.Title = PresentationModel.GetViewTitleForTenant("Create Group Manager", this.Session.GetTenant());
           SingleResourceManager rManager = new SingleResourceManager();
           ResourceGroupModel rsModel = new ResourceGroupModel(rManager.CreateResourceGroup(model.Name, model.ClassifierMode, null));

            return View("EditResourceGroup", rsModel);
        }

        public ActionResult Edit(long id)
        {
            ViewBag.Title = PresentationModel.GetViewTitleForTenant("Edit Group Manager", this.Session.GetTenant());
            SingleResourceManager rManager = new SingleResourceManager();
            ResourceGroup rc = rManager.GetResourceGroupById(id);

            return View("EditResourceGroup", new ResourceGroupModel(rc));
        }

        [HttpPost]
        public ActionResult Edit(ResourceGroupModel model)
        {
            SingleResourceManager rManager = new SingleResourceManager();
            ResourceGroup rc = rManager.GetResourceGroupById(model.Id);
            if (rc != null)
            {
                rc.Name = model.Name;
                rc.GroupMode = model.ClassifierMode;
                rManager.UpdateResourceGroup(rc);
            }
            else
            {
                //resource set does not exist!
            }

            return View("ResourceGroupManager");
        }

        public ActionResult Delete(long id)
        {
            SingleResourceManager rManager = new SingleResourceManager();
            ResourceGroup rc = rManager.GetResourceGroupById(id);
            rManager.DeleteResourceGroup(rc);

            return RedirectToAction("ResourceGroup");
        }

        public ActionResult ChooseResourceToGroup(long id)
        {
            ResourceGroupModel model = new ResourceGroupModel();
            model.Id = id;
            SingleResourceManager rManager = new SingleResourceManager();
            IQueryable<SingleResource> resoures = rManager.GetAllResources();
            List<ResourceModel> resourceMList = new List<ResourceModel>();
            foreach (SingleResource r in resoures)
            {
                resourceMList.Add(new ResourceModel(r));
            }
            model.AllResources = resourceMList;

            return PartialView("_chooseResources", model);
        }

        public ActionResult AddResourceToGroup(long setId, long resourceId)
        {
            SingleResourceManager rManager = new SingleResourceManager();
            ResourceGroup rc = rManager.GetResourceGroupById(setId);
            SingleResource r = rManager.GetResourceById(resourceId);
            rc.SingleResources.Add(r);

            rManager.UpdateResourceGroup(rc);
            ResourceGroupModel model = new ResourceGroupModel(rc);

            return View("EditResourceGroup", model);
        }

        public ActionResult AddResourcesToGroup(long setId, string resourceIds)
        {
            SingleResourceManager rManager = new SingleResourceManager();
            ResourceGroup rc = rManager.GetResourceGroupById(setId);

            if (rc != null)
            {
                if (!string.IsNullOrEmpty(resourceIds))
                {
                    var selectedResources = resourceIds.Split(',').Select(n => int.Parse(n)).ToList();

                    foreach (int i in selectedResources)
                    {
                        SingleResource r = rManager.GetResourceById(i);
                        rc.SingleResources.Add(r);
                    }

                    rManager.UpdateResourceGroup(rc);
                }
                ResourceGroupModel model = new ResourceGroupModel(rc);
                return View("EditResourceSet", model);
            }
            else
            {
                //set not exsits
                return View("EditResourceGroup");
            }
        }

        public ActionResult RemoveResourceFromGroup(long setId, long resourceId)
        {
            SingleResourceManager rManager = new SingleResourceManager();
            ResourceGroup rc = rManager.GetResourceGroupById(setId);
            SingleResource r = rManager.GetResourceById(resourceId);
            rc.SingleResources.Remove(r);

            rManager.UpdateResourceGroup(rc);
            ResourceGroupModel model = new ResourceGroupModel(rc);

            return View("EditResourceGroup", model);
        }

        [GridAction]
        public ActionResult ResourceGroup_Select()
        {
            SingleResourceManager rManager = new SingleResourceManager();
            IQueryable<ResourceGroup> data = rManager.GetAllResourceClassifiers();
            List<ResourceGroupManagerModel> model = new List<ResourceGroupManagerModel>();

            ResourceGroupManagerModel temp = new ResourceGroupManagerModel();
            data.ToList().ForEach(r => model.Add(new ResourceGroupManagerModel(r)));

            return View("ResourceManager", new GridModel<ResourceGroupManagerModel> { Data = model });
        }

        [GridAction]
        public ActionResult Resource_Select()
        {
            SingleResourceManager rManager = new SingleResourceManager();

            //Resource rsss = resourceManager.GetResourceById(1);
            IQueryable<SingleResource> data = rManager.GetAllResources();

            List<ResourceManagerModel> resources = new List<ResourceManagerModel>();

            data.ToList().ForEach(r => resources.Add(new ResourceManagerModel(r)));

            return View("ResourceManager", new GridModel<ResourceManagerModel> { Data = resources });
        }

    }
}
