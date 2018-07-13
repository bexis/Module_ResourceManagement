using BExIS.Web.Shell.Areas.RBM.Models.ResourceStructure;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using BExIS.Rbm.Entities.ResourceStructure;
using BExIS.Rbm.Services.ResourceStructure;
using Telerik.Web.Mvc;
using BExIS.Dlm.Services.DataStructure;
using BExIS.Dlm.Entities.DataStructure;
using BExIS.Security.Services.Authorization;
using BExIS.Security.Services.Subjects;
using BExIS.Security.Entities.Subjects;
using BExIS.Security.Entities.Objects;
using Vaiona.Web.Mvc.Models;
using BExIS.Web.Shell.Areas.RBM.Helpers;
using Vaiona.Web.Extensions;
using BExIS.Security.Entities.Authorization;
using BExIS.Security.Services.Objects;
using BExIS.Modules.RBM.UI.Helper;

namespace BExIS.Modules.RBM.UI.Controllers
{
    public class ResourceStructureController : Controller
    {
        #region ResourceStructure

        public ActionResult ResourceStructure()
        {
            ViewBag.Title = PresentationModel.GetViewTitleForTenant("Manage Resource Structures", this.Session.GetTenant());
            return View("ResourceStructureManager");
        }

        public ActionResult Create()
        {
            ViewBag.Title = PresentationModel.GetViewTitleForTenant("Create Resource Structures", this.Session.GetTenant());
            CreateResourceStructureModel resourceStructure = new CreateResourceStructureModel();
            return View("_createResourceStructure", resourceStructure);
        }

        [HttpPost]
        public ActionResult Create(CreateResourceStructureModel model)
        {
            ResourceStructureManager rsManager = new ResourceStructureManager();
            //check name
            ResourceStructure temp = rsManager.GetResourceStructureByName(StringHelper.CutSpaces(model.Name));
            if (temp != null)
                ModelState.AddModelError("NameExist", "Name already exist");

            if (ModelState.IsValid)
            {
                ResourceStructure rS = rsManager.Create(model.Name, model.Description, null, null);

                //Start -> add security ----------------------------------------

                using (var pManager = new EntityPermissionManager())
                using (var entityTypeManager = new EntityManager())
                {
                    UserManager userManager = new UserManager();
                    var userTask = userManager.FindByNameAsync(HttpContext.User.Identity.Name);
                    userTask.Wait();
                    var user = userTask.Result;

                    Entity entityType = entityTypeManager.FindByName("ResourceStructure");

                    //31 is the sum from all rights:  Read = 1, Download = 2, Write = 4, Delete = 8, Grant = 16
                    pManager.Create(user, entityType, rS.Id, 31);
                }

               //End -> add security ------------------------------------------


                  return View("_editResourceStructure", new ResourceStructureModel(rS));
            }
            
            else
            {
                return View("_createResourceStructure", model);
            }
        }

        public ActionResult Edit(long id)
        {
            ViewBag.Title = PresentationModel.GetViewTitleForTenant("Edit Resource Structures", this.Session.GetTenant());

            ResourceStructureManager rsManager = new ResourceStructureManager();
            ResourceStructure resourceStructure = rsManager.GetResourceStructureById(id);
            ViewData["RSID"] = id;
            //ResourceStructureModel tem = new ResourceStructureModel();
            return View("_editResourceStructure", new ResourceStructureModel(resourceStructure));
        }

        [HttpPost]
        public ActionResult EditResourceStructure(ResourceStructureModel model)
        {
            ResourceStructureManager rsManager = new ResourceStructureManager();

            //check name
            ResourceStructure temp = rsManager.GetResourceStructureByName(StringHelper.CutSpaces(model.Name));
            if (temp != null && temp.Id != model.Id)
                ModelState.AddModelError("NameExist", "Name already exist");


            ResourceStructure rs = rsManager.GetResourceStructureById(model.Id);
            if (rs == null)
                ModelState.AddModelError("Errors", "ResourceStructure does not exist.");

            if (rs.ResourceAttributeUsages.Select(a => a.ResourceStructureAttribute).ToList().Count() < 1)
                    ModelState.AddModelError("Errors", "Resource Structure has no attributes.");

                if(rs.Parent != null)
                {
                    if (rs.Parent.Id == rs.Id)
                    {
                        ModelState.AddModelError("Errors", "Parent resource structure can not be themselves.");
                    }
                }



                if (ModelState.IsValid)
                {
                    rs.Name = model.Name;
                    rs.Description = model.Description;

                    rsManager.Update(rs);

                    return RedirectToAction("ResourceStructure");

                }
                else
                    return View("_editResourceStructure", model);
        }

        public ActionResult EditIsOptionalUsage(long usageId, string isOptional)
        {
            ResourceStructureAttributeManager rsaManger = new ResourceStructureAttributeManager();
            ResourceAttributeUsage usage = rsaManger.GetResourceAttributeUsageById(usageId);
            usage.IsValueOptional = Convert.ToBoolean(isOptional);
            rsaManger.UpdateResourceAttributeUsage(usage);

            ResourceStructureManager rsManager = new ResourceStructureManager();
            ResourceStructure resourceStructure = rsManager.GetResourceStructureById(usage.ResourceStructure.Id);

            return View("_editResourceStructure", new ResourceStructureModel(resourceStructure));

        }

        public ActionResult EditIsFileUsage(long usageId, string isFile)
        {
            ResourceStructureAttributeManager rsaManger = new ResourceStructureAttributeManager();
            ResourceAttributeUsage usage = rsaManger.GetResourceAttributeUsageById(usageId);
            usage.IsFileDataType = Convert.ToBoolean(isFile);
            rsaManger.UpdateResourceAttributeUsage(usage);

            ResourceStructureManager rsManager = new ResourceStructureManager();
            ResourceStructure resourceStructure = rsManager.GetResourceStructureById(usage.ResourceStructure.Id);

            return View("_editResourceStructure", new ResourceStructureModel(resourceStructure));

        }

        public ActionResult RemoveParent(long id)
        {
            ResourceStructureManager rsManager = new ResourceStructureManager();
            ResourceStructure resourceStructure = rsManager.GetResourceStructureById(id);
            resourceStructure.Parent = null;
            rsManager.Update(resourceStructure);

            return RedirectToAction("Edit", new { id = resourceStructure.Id });
        }

        public ActionResult Delete(long id)
        {
            using (var rsManager = new ResourceStructureManager())
            using (var permissionManager = new EntityPermissionManager())
            using(var entityTypeManager = new EntityManager())
            {
                ResourceStructure resourceStructure = rsManager.GetResourceStructureById(id);
                bool deleted = rsManager.Delete(resourceStructure);

                if (deleted)
                {
                    Type entityType = entityTypeManager.FindByName("ResourceStructure").EntityType;
                    //delete security 
                    permissionManager.Delete(entityType, id);
                }
            }

            return RedirectToAction("ResourceStructure");
        }

        [GridAction]
        public ActionResult ResourceStructure_Select()
        {
            using (var rsManager = new ResourceStructureManager())
            using (var permissionManager = new EntityPermissionManager())
            using (var entityTypeManager = new EntityManager())
            {
                IQueryable<ResourceStructure> data = rsManager.GetAllResourceStructures();

                //List<ResourceStructureModel> resourceStructures = new List<ResourceStructureModel>();
                List<ResourceStructureManagerModel> resourceStructures = new List<ResourceStructureManagerModel>();

                //get id from loged in user
                long userId = UserHelper.GetUserId(HttpContext.User.Identity.Name);
                //get entity type id
                long entityTypeId = entityTypeManager.FindByName("ResourceStructure").Id;

                foreach (ResourceStructure rs in data)
                {
                    ResourceStructureManagerModel temp = new ResourceStructureManagerModel(rs);
                    temp.InUse = rsManager.IsResourceStructureInUse(rs.Id);

                    //get permission from logged in user
                    temp.EditAccess = permissionManager.HasEffectiveRight(userId, entityTypeId, rs.Id, RightType.Write);
                    temp.DeleteAccess = permissionManager.HasEffectiveRight(userId, entityTypeId, rs.Id, RightType.Delete);

                    resourceStructures.Add(temp);
                }


                return View("ResourceStructureManager", new GridModel<ResourceStructureManagerModel> { Data = resourceStructures });
            }
        }

        public ActionResult ChooseParentResourceStructure(long id)
        {
            List<ResourceStructureParentChoosingModel> model = new List<ResourceStructureParentChoosingModel>();
            ViewData["RSID"] = id;
            

            //ResourceStructureManager rsManager = new ResourceStructureManager();
            //ResourceStructure resourceStructure = rsManager.GetResourceStructureById(id);
            //ResourceStructureModel model = new ResourceStructureModel(resourceStructure);
            //model.FillRSM();
            return PartialView("_chooseResourceStructure", model);
        }


        public ActionResult AddParent(long id, long parentId)
        {
            ResourceStructureManager rsManager = new ResourceStructureManager();
           
                ResourceStructure parentResourceStructure = rsManager.GetResourceStructureById(parentId);
                ResourceStructure resourceStructure = rsManager.GetResourceStructureById(id);

                resourceStructure.Parent = parentResourceStructure;
                rsManager.Update(resourceStructure);

            return RedirectToAction("Edit", new {id = resourceStructure.Id });
               // return View("_editResourceStructure", new ResourceStructureModel(resourceStructure));
        }

        [GridAction]
        public ActionResult ResourceStructureParent_Select(long rsId)
        {
            ResourceStructureManager rsManager = new ResourceStructureManager();
            IQueryable<ResourceStructure> data = rsManager.GetAllResourceStructures();

            //List<ResourceStructureModel> resourceStructures = new List<ResourceStructureModel>();
            List<ResourceStructureParentChoosingModel> resourceStructures = new List<ResourceStructureParentChoosingModel>();

            foreach (ResourceStructure rs in data)
            {
                if (rs.Id != rsId)
                {
                    ResourceStructureParentChoosingModel temp = new ResourceStructureParentChoosingModel(rs);
                    temp.Locked = this.CheckParentPossibility(rsId, rs.Id);
                    temp.RsId = rsId;
                    temp.ParentId = rs.Id;
                    resourceStructures.Add(temp);
                }
            }

            return View("_chooseResourceStructure", new GridModel<ResourceStructureParentChoosingModel> { Data = resourceStructures });
        }

        //prevents circulation in parents
        private bool CheckParentPossibility(long rsId,long parentId)
        {
            ResourceStructureManager rsManager = new ResourceStructureManager();
            ResourceStructure rsParent = rsManager.GetResourceStructureById(parentId);
            if (rsParent.Parent != null)
            {
                if (rsParent.Parent.Id != rsId)
                {
                    CheckParentPossibility(rsId, rsParent.Parent.Id);
                }
                else
                    return true;

            }
            return false;
            
        }


        #endregion

        #region ResourceStructureAttributes

        public ActionResult ResourceStructureAttribute()
        {
            ViewBag.Title = PresentationModel.GetViewTitleForTenant("Manage Resource Structure Attributes", this.Session.GetTenant());
            return View("ResourceStructureAttributeManager");
        }

        //create with usage to resource structure
        public ActionResult CreateResourceStructureAttributeWithUsage(long id)
        {
            EditResourceStructureAttributeModel model = new EditResourceStructureAttributeModel();
            model.rsID = id;

            return PartialView("_createResourceStructureAttribute",model);
        }

        //Create new attr independent from ResourceStructure without usage
        public ActionResult CreateResourceStructureAttributeWithoutUsage()
        {
            return PartialView("_createResourceStructureAttribute", new EditResourceStructureAttributeModel());
        }

        [HttpPost]
        public ActionResult SaveResourceStructureAttribute(EditResourceStructureAttributeModel model, string[] keys)
        {
            ResourceStructureAttributeManager rsaManager = new ResourceStructureAttributeManager();
            //check name
            if (model.AttributeName != null)
            {
                ResourceStructureAttribute tempRS = rsaManager.GetResourceStructureAttributesByName(StringHelper.CutSpaces(model.AttributeName));
                if (tempRS != null && tempRS.Id != model.Id)
                    ModelState.AddModelError("NameExist", "Name already exist.");
            }

            if (ModelState.IsValid)
            {
                ResourceStructureManager rsManager = new ResourceStructureManager();
                ResourceStructureAttribute rsa = new ResourceStructureAttribute();
                if(model.Id == 0)
                {
                    rsa = rsaManager.CreateResourceStructureAttribute(model.AttributeName, model.AttributeDescription);
                }
                else
                {
                    rsa = rsaManager.GetResourceStructureAttributesById(model.Id);
                    rsa.Name = model.AttributeName;
                    rsa.Description = model.AttributeDescription;
                    rsaManager.UpdateResourceStructureAttribute(rsa);
                }

                if (rsa != null && model.Id == 0)
                {
                    //Start -> add security ----------------------------------------

                    using (EntityPermissionManager pManager = new EntityPermissionManager())
                    using (var entityTypeManager = new EntityManager())
                    {
                        UserManager userManager = new UserManager();
                        var userTask = userManager.FindByNameAsync(HttpContext.User.Identity.Name);
                        userTask.Wait();
                        var user = userTask.Result;

                        Entity entityType = entityTypeManager.FindByName("ResourceStructureAttribute");

                        pManager.Create(user, entityType, rsa.Id, Enum.GetValues(typeof(RightType)).Cast<RightType>().ToList());
                    }

                    //End -> add security ------------------------------------------
                }

                if (keys != null)
                {
                    List<DomainItem> domainItems = CreateDomainItems(keys);
                    DataContainerManager dcManager = new DataContainerManager();

                    if (model.Id == 0 || rsa.Constraints.Count() == 0)
                    {
                        DomainConstraint dc = new DomainConstraint(ConstraintProviderSource.Internal, "", "en-US", "a simple domain validation constraint", false, null, null, null, domainItems);
                        dcManager.AddConstraint(dc, rsa);
                    }
                    else
                    {
                       DomainConstraint temp = (DomainConstraint)rsa.Constraints.ElementAt(0);
                       temp.Materialize();
                       temp.Items = domainItems;
                       dcManager.AddConstraint(temp, rsa); 
                    }

                }

                //Creation with usage
                if (model.rsID != 0)
                {
                    ResourceStructure resourceStructure = rsManager.GetResourceStructureById(model.rsID);
                    rsaManager.CreateResourceAttributeUsage(rsa,resourceStructure, true, false);
                    //resourceStructure.ResourceStructureAttributes.Add(rsa);
                    //rsManager.Update(resourceStructure);
                    //return View("_editResourceStructure", new ResourceStructureModel(resourceStructure));
                    return Json(new { success = true });
                }
                else
                    return Json(new { success = true });
            }
            else
                return PartialView("_createResourceStructureAttribute", model);
        }

        

        public ActionResult OpenEditResourceStructureAttribute(long id)
        {
            ResourceStructureAttributeManager rsaManager = new ResourceStructureAttributeManager();
            ResourceStructureAttribute rsa = rsaManager.GetResourceStructureAttributesById(id);

            EditResourceStructureAttributeModel model = new EditResourceStructureAttributeModel();
            model.Id = id;
            model.AttributeName = rsa.Name;
            model.AttributeDescription = rsa.Description;

            if (rsa.Constraints != null)
            {
                foreach (Constraint c in rsa.Constraints)
                {
                    if (c is DomainConstraint)
                    {
                        DomainConstraint dc = (DomainConstraint)c;
                        dc.Materialize();
                        List<DomainItemModel> domainItemModelList = new List<DomainItemModel>();
                        dc.Items.ToList().ForEach(r => domainItemModelList.Add(new DomainItemModel(r)));
                        model.DomainItems = domainItemModelList;
                    }
                }
            }

            return PartialView("_createResourceStructureAttribute", model);
        }

        //Create a list of domain items from a string array
        private List<DomainItem> CreateDomainItems(string[] keys)
        {
            List<DomainItem> domainItems = new List<DomainItem>();
            for (int i = 0; i < keys.Length; i++)
            {
                DomainItem domainItem = new DomainItem();
                domainItem.Key = keys[i];
                //for this implemention values are not needed now
                //domainItem.Value = value[i];
                domainItems.Add(domainItem);
            }

            return domainItems;
        }

        public ActionResult DeleteResourceStructureAttribute(long id)
        {
            using (var rsaManager = new ResourceStructureAttributeManager())
            using (var permissionManager = new EntityPermissionManager())
            using (var entityTypeManager = new EntityManager())
            {
                ResourceStructureAttribute rsa = rsaManager.GetResourceStructureAttributesById(id);
                if (rsa != null)
                {
                    bool deleted = rsaManager.DeleteResourceStructureAttribute(rsa);

                    if (deleted)
                    {
                        Type entityType = entityTypeManager.FindByName("Notification").EntityType;
                        //delete security 
                        permissionManager.Delete(entityType, id);
                    }
                }
                else
                {
                    //rsa not exsits, need implemention here
                }
            }
            return View("ResourceStructureAttributeManager");
        }

        public ActionResult ChooseResourceStructureAttributes(long id)
        {
            ViewData["RSID"] = id;

            return PartialView("_chooseResourceStructureAttributes");
        }

      
        [GridAction]
        public ActionResult ResourceStructureAttributesAll_Select(long id)
        {
            ResourceStructureManager rsManager = new ResourceStructureManager();
            ResourceStructureAttributeManager rsaManager = new ResourceStructureAttributeManager();
            IQueryable<ResourceStructureAttribute> rsaList = rsaManager.GetAllResourceStructureAttributes();
            List<ResourceStructureAttributeModel> list = new List<ResourceStructureAttributeModel>();

            foreach (ResourceStructureAttribute a in rsaList)
            {
                ResourceStructureAttributeModel rsaModel = new ResourceStructureAttributeModel(a);
                rsaModel.rsID = id;
                list.Add(rsaModel);
            }
            return View("_chooseResourceStructureAttributes", new GridModel<ResourceStructureAttributeModel> { Data = list });
        }

        [GridAction]
        public ActionResult ResourceStructureAttributesAllManager_Select()
        {
            using (var rsManager = new ResourceStructureManager())
            using (var rsaManager = new ResourceStructureAttributeManager())
            using (var permissionManager = new EntityPermissionManager())
            using (var entityTypeManager = new EntityManager())
            {
                IQueryable<ResourceStructureAttribute> rsaList = rsaManager.GetAllResourceStructureAttributes();
                List<ResourceStructureAttributeModel> list = new List<ResourceStructureAttributeModel>();

                foreach (ResourceStructureAttribute a in rsaList)
                {
                    ResourceStructureAttributeModel rsaModel = new ResourceStructureAttributeModel(a);
                    if (rsaManager.IsAttributeInUse(a.Id))
                        rsaModel.InUse = true;

                    //get id from loged in user
                    long userId = UserHelper.GetUserId(HttpContext.User.Identity.Name);
                    //get entity type id
                    long entityTypeId = entityTypeManager.FindByName("Notification").Id;

                    //get permission from logged in user
                    rsaModel.EditAccess = permissionManager.HasEffectiveRight(userId, entityTypeId, a.Id, RightType.Write);
                    rsaModel.DeleteAccess = permissionManager.HasEffectiveRight(userId, entityTypeId, a.Id, RightType.Delete);
                    list.Add(rsaModel);
                }
                return View("ResourceStructureAttributeManager", new GridModel<ResourceStructureAttributeModel> { Data = list });
            }
        }

        #endregion

        #region ResourceStructureUsages

        public ActionResult RemoveUsageFromResourceStructure(long usageId, long rsId)
        {

            ResourceStructureAttributeManager rsaManager = new ResourceStructureAttributeManager();
            ResourceAttributeUsage usage = rsaManager.GetResourceAttributeUsageById(usageId);
            rsaManager.DeleteResourceAttributeUsage(usage);

            return RedirectToAction("Edit", new { id = rsId });
        }

        [GridAction]
        public ActionResult ResourceStructureUsages_Select(long id)
        {
            ResourceStructureManager rsm = new ResourceStructureManager();
            ResourceStructure resourceStructure = rsm.GetResourceStructureById(id);

            List<ResourceStructureAttributeUsageModel> list = new List<ResourceStructureAttributeUsageModel>();
            foreach (ResourceAttributeUsage usage in resourceStructure.ResourceAttributeUsages)
            {
                list.Add(new ResourceStructureAttributeUsageModel(usage.Id));
            }
            return View("_showResourceStructureAttributes", new GridModel<ResourceStructureAttributeUsageModel> { Data = list });
        }

        public ActionResult AddResourceAttributeUsages(string rsId, string rsaIds)
        {
            ResourceStructureManager rsManger = new ResourceStructureManager();
            ResourceStructure rs = rsManger.GetResourceStructureById(Convert.ToInt64(rsId));
            ResourceStructureAttributeManager rsaManager = new ResourceStructureAttributeManager();
            string message = "";
            if (rs != null)
            {
                if (!string.IsNullOrEmpty(rsaIds))
                {
                    var selectedRsa = rsaIds.Split(',').Select(n => int.Parse(n)).ToList();
                    foreach (int i in selectedRsa)
                    {
                        ResourceStructureAttribute rsa = rsaManager.GetResourceStructureAttributesById(i);
                        if (!IsAttributeInStructure(rs, rsa))
                            rsaManager.CreateResourceAttributeUsage(rsa, rs, false, false);
                        else
                            message += "Resource Structure Attribute " + rsa.Name + "is allready in the structure.";
                    }
                }

                ResourceStructureModel model = new ResourceStructureModel(rs);
                model.Message = message;
                return View("_editResourceStructure", model);
            }
            else
            {
                //rs don't exsits ??
                return View("_editResourceStructure");
            }
        }

        public ActionResult AddResourceAttributeUsage(long rsId, long rsaId)
        {
            ResourceStructureManager rsManger = new ResourceStructureManager();
            ResourceStructure rs = rsManger.GetResourceStructureById(rsId);
            ResourceStructureAttributeManager rsaManager = new ResourceStructureAttributeManager();
            ResourceStructureAttribute rsa = rsaManager.GetResourceStructureAttributesById(rsaId);
            string message = "";

            if (!IsAttributeInStructure(rs, rsa))
                rsaManager.CreateResourceAttributeUsage(rsa, rs, false, false);
            else
                message += "Resource Structure Attribute " + rsa.Name + "is allready in the structure.";

            ResourceStructureModel model = new ResourceStructureModel(rs);
            model.Message = message;
            return View("_editResourceStructure", model);
        }

        private bool IsAttributeInStructure(ResourceStructure rs, ResourceStructureAttribute rsa)
        {
            if (rs.ResourceAttributeUsages.Select(a => a.ResourceStructureAttribute.Id).ToList().Contains(rsa.Id))
                return true;
            else
            return false;
        }


        #endregion

        #region ResourceStructureAttributeConstraints

        public void CreateDomainConstraint(ResourceStructureAttribute attr, List<DomainItem> domainItems)
        {
            DataContainerManager dcManager = new DataContainerManager();
            DomainConstraint c3 = new DomainConstraint(ConstraintProviderSource.Internal, "", "en-US", "a simple domain validation constraint", false, null, null, null, domainItems);
            dcManager.AddConstraint(c3, attr);
        }

        public ActionResult LoadDomainItems(EditResourceStructureAttributeModel model)
        {
            return PartialView("_domainItems", model);
        }

        public ActionResult Add()
        {
            return PartialView("_createDomainItem", new DomainItemModel());
        }

        #endregion

    }
}
