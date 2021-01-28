using BExIS.Rbm.Entities.BookingManagementTime;
using BExIS.Rbm.Entities.Resource;
using BExIS.Rbm.Entities.ResourceConstraint;
using BExIS.Rbm.Entities.ResourceStructure;
using BExIS.Rbm.Services.Resource;
using BExIS.Rbm.Services.ResourceConstraints;
using BExIS.Rbm.Services.ResourceStructure;
using BExIS.Security.Services.Authorization;
using BExIS.Web.Shell.Areas.RBM.Models.Resource;
using BExIS.Web.Shell.Areas.RBM.Models.ResourceStructure;
using BExIS.Security.Entities.Subjects;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Telerik.Web.Mvc;
using BExIS.Security.Services.Subjects;
using BExIS.Security.Entities.Objects;
using Vaiona.Web.Mvc.Models;
using BExIS.Web.Shell.Areas.RBM.Models.Booking;
using BExIS.Rbm.Services.Users;
using BExIS.Rbm.Services.BookingManagementTime;
using BExIS.Rbm.Entities.Users;
using BExIS.Web.Shell.Areas.RBM.Helpers;
using BExIS.Web.Shell.Areas.RBM.Models.BookingManagementTime;
using Vaiona.Web.Extensions;
using BExIS.Security.Entities.Authorization;
using BExIS.Modules.RBM.UI.Helper;
using BExIS.Security.Services.Objects;
using BExIS.Dlm.Services.Party;
using BExIS.Dlm.Entities.Party;

namespace BExIS.Modules.RBM.UI.Controllers
{
    public class ResourceController : Controller
    {
        // GET: /RBM/Resource/
        public ActionResult Resource()
        {
            ViewBag.Title = PresentationModel.GetViewTitleForTenant("Manage Resources", this.Session.GetTenant());
            return View("ResourceManager");
        }

        #region Create and Save Resource

        public ActionResult Create()
        {
            Session["Resource"] = null;

            ViewBag.Title = PresentationModel.GetViewTitleForTenant("Create new Resource", this.Session.GetTenant());
            EditResourceModel model = new EditResourceModel();
            Session["Resource"] = model;

            return View("EditResource", model);
        }

        //Save Resource after create or edit
        public ActionResult Save()
        {
            using (var rManager = new ResourceManager())
            using (var rsManager = new ResourceStructureManager())
            using (var valueManager = new ResourceStructureAttributeManager())
            {
                //get filled model
                EditResourceModel model = (EditResourceModel)Session["Resource"];

                ValidateResource(model);

                if (ModelState.IsValid)
                {
                    //start check model state
                    ResourceStructure rs = rsManager.GetResourceStructureById(model.ResourceStructure.Id);
                    SingleResource resource = new SingleResource();
                    if (model.Id == 0)
                    {
                        resource = rManager.CreateResource(model.Name, model.Description, model.Quantity, model.Color, model.WithActivity, rs, model.Duration);
                    }
                    else
                    {
                        resource = rManager.GetResourceById(model.Id);
                        resource.Name = model.Name;
                        resource.Description = model.Description;
                        resource.Quantity = model.Quantity;
                        resource.Color = model.Color;
                        resource.Duration = model.Duration;
                        resource.WithActivity = model.WithActivity;
                        resource.ResourceStructure = rs;

                        //remove constrains
                        List<ResourceConstraintModel> tempList = model.ResourceConstraints;
                        if (tempList != null)
                        {
                            if (tempList.Count() > 0)
                            {
                                for (int i = 0; i < tempList.Count(); i++)
                                {
                                    if (tempList[i].Deleted)
                                    {
                                        //remove it from resource
                                        ResourceConstraint rc = resource.ResourceConstraints.Where(e => e.Id == tempList[i].Id).FirstOrDefault();
                                        resource.ResourceConstraints.Remove(rc);
                                        //remove it from model
                                        ResourceConstraintModel temp = model.ResourceConstraints.Where(a => a.Id == tempList[i].Id).FirstOrDefault();
                                        model.ResourceConstraints.Remove(temp);
                                    }
                                }
                            }
                        }

                        rManager.UpdateResource(resource);
                    }

                    //Begin -> save/update resource structure attribute values -------------------------

                    if (model.TextValues.Count > 0)
                    {
                        //if one text value which is not optional has no Id the RS has changed and all old text values must be deleted
                        if (model.TextValues.Where(u => u.ResourceAttributeUsage.IsValueOptional == false).ToList().Select(a => a.Id).ToList().Contains(0))
                        {
                            if (model.Id != 0)
                            {
                                SingleResource r = rManager.GetResourceById(model.Id);
                                List<ResourceAttributeValue> oldValues = valueManager.GetValuesByResource(r);
                                foreach (ResourceAttributeValue v in oldValues)
                                {
                                    valueManager.DeleteResourceAttributeValue(v);
                                }
                            }
                        }

                        foreach (TextValueModel tv in model.TextValues)
                        {
                            ResourceAttributeUsage usage = valueManager.GetResourceAttributeUsageById(tv.ResourceAttributeUsageId);
                            if (tv.Id == 0)
                            {
                                valueManager.CreateResourceAttributeValue(tv.Value, rManager.GetResourceById(resource.Id), usage);
                            }
                            else
                            {
                                TextValue tValue = valueManager.GetTextValueById(tv.Id);
                                tValue.Value = tv.Value;
                                valueManager.UpdateResourceAttributeValue(tValue);
                            }
                        }
                    }
                    if (model.FileValues.Count > 0)
                    {
                        //if one text value has no Id the RS has changed and all old text values must be deleted
                        //if (model.FileValues.Select(a => a.Id).ToList().Contains(0))
                        //{
                        //    //if (model.Id != 0)
                        //    //{
                        //    //    SingleResource r = rManager.GetResourceById(model.Id);
                        //    //    List<ResourceAttributeValue> oldValues = valueManager.GetValuesByResource(r);
                        //    //    foreach (ResourceAttributeValue v in oldValues)
                        //    //    {
                        //    //        valueManager.DeleteResourceAttributeValue(v);
                        //    //    }
                        //    //}
                        //}

                        foreach (FileValueModel fv in model.FileValues)
                        {
                            if (fv.Data != null && fv.Id == 0)
                            {
                                valueManager.CreateResourceAttributeValue(fv.Name, fv.Extention, fv.Minmetype, fv.Data, fv.NeedConfirmation, rManager.GetResourceById(resource.Id), valueManager.GetResourceAttributeUsageById(fv.ResourceAttributeUsageId));
                            }
                            else if (fv.Data != null && fv.Id != 0)
                            {
                                FileValue fValue = valueManager.GetFileValueById(fv.Id);
                                fValue.Name = fv.Name;
                                fValue.Extention = fv.Extention;
                                fValue.Minmetype = fv.Minmetype;
                                fValue.Data = fv.Data;
                                fv.NeedConfirmation = fv.NeedConfirmation;
                                valueManager.UpdateResourceAttributeValue(fValue);
                            }
                            else if (fv.Data == null && fv.Id != 0)
                            {
                                FileValue fValue = valueManager.GetFileValueById(fv.Id);
                                valueManager.DeleteResourceAttributeValue(fValue);
                            }
                        }
                    }

                    // End -> save resource structure attribute values -------------------------

                    // Begin -> constraints saving, Index says from wich row the specialConstaint cames to get the non spezific values like mode, description

                    if (model.ResourceConstraints != null && model.ResourceConstraints.Count() > 0)
                    {
                        foreach (ResourceConstraintModel rc in model.ResourceConstraints)
                        {
                            SaveConstraint(resource, rc.Index);
                        }
                    }

                    // End -> constraints saving -----------------------------------

                    //Start -> add security ----------------------------------------

                    using (var pManager = new EntityPermissionManager())
                    using (var entityTypeManager = new EntityManager())
                    using (UserManager userManager = new UserManager())
                    {
                        var userTask = userManager.FindByNameAsync(HttpContext.User.Identity.Name);
                        userTask.Wait();
                        var user = userTask.Result;

                        //get entity type
                        Entity entityType = entityTypeManager.FindByName("SingleResource");

                        //31 is the sum from all rights:  Read = 1,  Write = 4, Delete = 8, Grant = 16
                        int rights = (int)RightType.Read + (int)RightType.Write + (int)RightType.Delete + (int)RightType.Grant;
                        pManager.Create(user, entityType, resource.Id, rights);
                    }

                    //End -> add security ------------------------------------------

                    return View("ResourceManager");
                }
                else
                    return View("EditResource", model);
            }
        }

        private void ValidateResource(EditResourceModel model)
        {
            using (ResourceManager rManager = new ResourceManager())
            using (ResourceStructureManager rsManager = new ResourceStructureManager())
            using (ResourceStructureAttributeManager valueManager = new ResourceStructureAttributeManager())
            {

                //check name is filled
                if (model.Name == null)
                    ModelState.AddModelError("Errors", "Name is required.");
                else
                {
                    //check length of the 
                    if (model.Name.Length < 3)
                        ModelState.AddModelError("Errors", "The resource name must be 3 characters long.");

                    //check name is unique
                    SingleResource temp = rManager.GetResourceByName(StringHelper.CutSpaces(model.Name));
                    if (temp != null && model.Id == 0)
                        ModelState.AddModelError("Errors", "Name already exist.");
                }

                //check duration cant not be 0
                if (model.Duration.Value == 0)
                    ModelState.AddModelError("Errors", "Duration can not be 0.");

                //check color can not be empty
                if (model.Color == null)
                    ModelState.AddModelError("Errors", "Color is required.");

                //check if resource struture is selected
                if (model.ResourceStructure == null)
                    ModelState.AddModelError("Errors", "Resource Structure is required, please choose one.");

                //check rsource structure attribute values
                if (model.TextValues.Count > 0)
                {
                    foreach (TextValueModel tvm in model.TextValues)
                    {
                        ResourceAttributeUsage usage = valueManager.GetResourceAttributeUsageById(tvm.ResourceAttributeUsageId);
                        if (usage.IsValueOptional == false && (tvm.Value == null || tvm.Value == ""))
                        {
                            ModelState.AddModelError("Errors", "Set a value to " + usage.ResourceStructureAttribute.Name + " is required.");
                        }
                        else if (usage.IsValueOptional == true && tvm.Value == null)
                        {
                            continue;
                        }
                    }
                }

                if (model.FileValues.Count > 0)
                {
                    foreach (FileValueModel fvm in model.FileValues)
                    {
                        ResourceAttributeUsage usage = valueManager.GetResourceAttributeUsageById(fvm.ResourceAttributeUsageId);
                        if (usage.IsValueOptional == false && fvm.Name == null)
                        {
                            ModelState.AddModelError("Errors", "Upload a file to " + usage.ResourceStructureAttribute.Name + " is required");
                        }
                        else if (usage.IsValueOptional == true && fvm.Name == "")
                        {
                            continue;
                        }
                    }
                }

                //check resource constraints
                if (model.DependencyConstraints != null && model.DependencyConstraints.Count > 0)
                {
                    foreach (DependencyConstraintModel dcM in model.DependencyConstraints)
                    {
                        ValidateConstraint(dcM);
                    }
                }

                if (model.BlockingConstraints != null && model.BlockingConstraints.Count > 0)
                {
                    foreach (BlockingConstraintModel bcM in model.BlockingConstraints)
                    {
                        ValidateConstraint(bcM);
                    }
                }

                if (model.QuantityConstraints != null && model.QuantityConstraints.Count > 0)
                {
                    foreach (QuantityConstraintModel qcM in model.QuantityConstraints)
                    {
                        ValidateConstraint(qcM);
                    }
                }
            }
        }

        // type for resource struture attribute value types
        public ActionResult OnChangeResourceItem(string element, string value, string valuetype, string constraintelement, string constraintindex, string periodicTimeIntervalElement, string dayId)
        {
            EditResourceModel model = (EditResourceModel)Session["Resource"];
            using (ResourceStructureManager rsManager = new ResourceStructureManager())
            {
                if (model != null)
                {
                    switch (element)
                    {
                        case "name":
                            model.Name = value;
                            break;
                        case "description":
                            model.Description = value;
                            break;
                        case "quantity":
                            model.Quantity = Convert.ToInt32(value);
                            break;
                        case "color":
                            model.Color = value;
                            break;
                        case "duration":
                            model.Duration.Value = Convert.ToInt32(value);
                            break;
                        case "timeunit":
                            model.Duration.TimeUnit = (SystemDefinedUnit)Enum.Parse(typeof(SystemDefinedUnit), value);
                            break;
                        case "withactivity":
                            model.WithActivity = Convert.ToBoolean(value);
                            break;
                        case "resourcestructureid":
                            model.ResourceStructure = new ResourceStructureModel(rsManager.GetResourceStructureById(Convert.ToInt64(value)));
                            break;
                        case "resourcestructurevalue":
                            ResourceStructureAttributeValueModel temp = model.ResourceStructureAttributeValues.Where(a => a.AttributeName == valuetype).FirstOrDefault();
                            if (temp is TextValueModel)
                            {
                                TextValueModel tv = (TextValueModel)temp;
                                tv.Value = value;

                                var l = model.TextValues.FindIndex(p => p.AttributeName == valuetype);
                                model.ResourceStructureAttributeValues[l] = tv;
                            }
                            if (temp is FileValueModel)
                            {
                                //save file in session???????
                                FileValueModel fv = (FileValueModel)temp;
                                fv.NeedConfirmation = Convert.ToBoolean(value);
                                var z = model.ResourceStructureAttributeValues.FindIndex(p => p.AttributeName == valuetype);
                                model.ResourceStructureAttributeValues[z] = fv;
                            }
                            break;
                        case "constraint":
                            ResourceConstraintModel tempC = new ResourceConstraintModel();
                            tempC = model.ResourceConstraints.Where(a => a.Index == int.Parse(constraintindex)).FirstOrDefault();
                            switch (constraintelement)
                            {
                                case "description":
                                    tempC.Description = value;
                                    break;
                                case "mode":
                                    tempC.SelectedMode = (ConstraintMode)Enum.Parse(typeof(ConstraintMode), value);
                                    break;
                                case "negated":
                                    tempC.Negated = Convert.ToBoolean(value);
                                    break;
                                case "startdate":
                                    tempC.ForTimeInterval.StartTime.Instant = DateTime.Parse(value);
                                    break;
                                case "enddate":
                                    tempC.ForTimeInterval.EndTime.Instant = DateTime.Parse(value);
                                    break;
                                case "periodictimeintervalisset":
                                    tempC.ForPeriodicTimeInterval.IsSet = Boolean.Parse(value);
                                    break;
                                case "periodictimeinterval":
                                    switch (periodicTimeIntervalElement)
                                    {
                                        case "resetfrequency":
                                            tempC.ForPeriodicTimeInterval.PeriodicTimeInstant.ResetFrequency = (ResetFrequency)Enum.Parse(typeof(ResetFrequency), value);
                                            break;
                                        case "resetinterval":
                                            tempC.ForPeriodicTimeInterval.PeriodicTimeInstant.ResetInterval = int.Parse(value);
                                            break;
                                        case "starttime":
                                            tempC.ForPeriodicTimeInterval.StartTime = DateTime.Parse(value);
                                            break;
                                        case "endtime":
                                            tempC.ForPeriodicTimeInterval.EndTime = DateTime.Parse(value);
                                            break;
                                        case "monthlyselectionstart":
                                            tempC.ForPeriodicTimeInterval.StartDate = DateTime.Parse(value);
                                            break;
                                        case "monthlyselectionend":
                                            tempC.ForPeriodicTimeInterval.EndDate = DateTime.Parse(value);
                                            break;
                                        case "repeaton":
                                            bool selected = bool.Parse(value);
                                            if (selected)
                                                tempC.ForPeriodicTimeInterval.SelectedDays.Add(int.Parse(dayId));
                                            else
                                                tempC.ForPeriodicTimeInterval.SelectedDays.Remove(int.Parse(dayId));
                                            break;
                                    }
                                    break;
                            }
                            var d = model.ResourceConstraints.FindIndex(p => p.Index == Convert.ToInt32(constraintindex));
                            model.ResourceConstraints[d] = tempC;
                            break;
                        case "blockingconstraint":
                            BlockingConstraintModel tempBC = new BlockingConstraintModel();
                            tempBC = model.BlockingConstraints.Where(a => a.Index == int.Parse(constraintindex)).FirstOrDefault();
                            switch (constraintelement)
                            {
                                case "forever":
                                    tempBC.ForEver = Convert.ToBoolean(value);
                                    break;
                                case "allusers":
                                    tempBC.AllUsers = Convert.ToBoolean(value);
                                    break;
                                default:
                                    break;
                            }
                            var i = model.BlockingConstraints.FindIndex(p => p.Index == Convert.ToInt32(constraintindex));
                            model.BlockingConstraints[i] = tempBC;
                            break;
                        case "dependencyconstraint":
                            DependencyConstraintModel tempDC = new DependencyConstraintModel();
                            tempDC = model.DependencyConstraints.Where(a => a.Index == int.Parse(constraintindex)).FirstOrDefault();
                            switch (constraintelement)
                            {
                                case "quantity":
                                    tempDC.Quantity = Convert.ToInt32(value);
                                    break;
                                case "implicit":
                                    tempDC.Implicit = Convert.ToBoolean(value);
                                    break;
                                case "objectid":
                                    tempDC.ObjectId = value;
                                    break;
                                case "objectname":
                                    tempDC.ObjectName = value;
                                    break;
                                default:
                                    break;
                            }
                            var j = model.DependencyConstraints.FindIndex(p => p.Index == Convert.ToInt32(constraintindex));
                            model.DependencyConstraints[j] = tempDC;
                            break;
                        case "quantityconstraint":
                            QuantityConstraintModel tempQC = new QuantityConstraintModel();
                            tempQC = model.QuantityConstraints.Where(a => a.Index == int.Parse(constraintindex)).FirstOrDefault();
                            switch (constraintelement)
                            {
                                case "forever":
                                    tempQC.ForEver = Convert.ToBoolean(value);
                                    break;
                                case "allusers":
                                    tempQC.AllUsers = Convert.ToBoolean(value);
                                    break;
                                case "quantity":
                                    tempQC.Quantity = Convert.ToInt32(value);
                                    break;
                                default:
                                    break;
                            }
                            var k = model.QuantityConstraints.FindIndex(p => p.Index == Convert.ToInt32(constraintindex));
                            model.QuantityConstraints[k] = tempQC;
                            break;
                    }
                }

                Session["Resource"] = model;

                return View("EditResource", model);
            }
        }

        public ActionResult SaveFileTemp(HttpPostedFileBase file, long id)
        {
            EditResourceModel model = (EditResourceModel)Session["Resource"];

            foreach (FileValueModel fv in model.FileValues)
            {
                if (id == fv.ResourceAttributeUsageId)
                {
                    FileValueModel fvm = (FileValueModel)fv;
                    fvm.Name = file.FileName;
                    fvm.Minmetype = file.ContentType;
                    byte[] data = null;
                    using (var binaryReader = new BinaryReader(file.InputStream))
                    {
                        data = binaryReader.ReadBytes(file.ContentLength);
                    }
                    fvm.Data = data;
                }
            }


            Session["Resource"] = model;

            return Content("");

        }


        public ActionResult DeleteFile(long id)
        {

            EditResourceModel model = (EditResourceModel)Session["Resource"];
            ResourceStructureAttributeValueModel m = model.ResourceStructureAttributeValues.Where(a => a.Id == id).FirstOrDefault();

            using (var valueManager = new ResourceStructureAttributeManager())
            {
                FileValue fValue = valueManager.GetFileValueById(id);
                valueManager.DeleteResourceAttributeValue(fValue);
            }

            model.ResourceStructureAttributeValues.Remove(m);

            Session["Resource"] = model;

            return RedirectToAction("Edit", new { id = model.Id });




        }

        #endregion

        #region Edit Resource

        public ActionResult Edit(long id)
        {
            Session["Resource"] = null;

            ViewBag.Title = PresentationModel.GetViewTitleForTenant("Edit Resource", this.Session.GetTenant());

            using (ResourceManager rManager = new ResourceManager())
            using (ResourceStructureAttributeManager rsaManager = new ResourceStructureAttributeManager())
            {
                SingleResource resource = rManager.GetResourceById(id);
                List<ResourceStructureAttributeValueModel> valuesModel = new List<ResourceStructureAttributeValueModel>();
                List<TextValueModel> textvalues = new List<TextValueModel>();
                List<FileValueModel> fileValues = new List<FileValueModel>();

                foreach (ResourceAttributeValue value in resource.ResourceAttributeValues)
                {
                    if (value is TextValue)
                    {
                        TextValue tv = (TextValue)value;
                        TextValueModel tvm = new TextValueModel(tv);
                        tvm.EditMode = true;
                        textvalues.Add(tvm);
                        valuesModel.Add(tvm);
                    }
                    else if (value is FileValue)
                    {
                        FileValue fv = (FileValue)value;
                        FileValueModel fvm = new FileValueModel(fv);
                        fvm.EditMode = true;
                        fileValues.Add(fvm);
                        valuesModel.Add(fvm);
                    }
                }

                EditResourceModel model = new EditResourceModel(resource, valuesModel, textvalues, fileValues);

                Session["Resource"] = model;

                return View("EditResource", model);
            }
        }

        #endregion

        #region Delete Resource

        public ActionResult Delete(long id)
        {
            using (var rManager = new ResourceManager())
            using (var valueManager = new ResourceStructureAttributeManager())
            using (var permissionManager = new EntityPermissionManager())
            using (var entityTypeManager = new EntityManager())
            {
                SingleResource resource = rManager.GetResourceById(id);

                //Delete values before delete resource
                List<ResourceAttributeValue> valuesToDelete = valueManager.GetValuesByResource(resource);
                valueManager.DeleteResourceStructureAttributeValues(valuesToDelete);

                bool deleted = rManager.DeleteResource(resource);

                if (deleted)
                {
                    Type entityType = entityTypeManager.FindByName("SingleResource").EntityType;
                    //delete security 
                    permissionManager.Delete(entityType, id);
                }
            }

            return RedirectToAction("Resource");
        }

        #endregion

        #region Show Resource

        public ActionResult ShowDetails(string id)
        {
            using (ResourceManager rManager = new ResourceManager())
            {
                SingleResource r = rManager.GetResourceById(long.Parse(id));

                List<ResourceStructureAttributeValueModel> valuesModel = new List<ResourceStructureAttributeValueModel>();

                foreach (ResourceAttributeValue value in r.ResourceAttributeValues)
                {
                    ResourceStructureAttributeValueModel temp = new ResourceStructureAttributeValueModel(value);
                    temp.EditMode = true;
                    valuesModel.Add(temp);
                }

                ShowResourceModel model = new ShowResourceModel(r, valuesModel);

                return PartialView("_showResource", model);
            }

        }

        #endregion

        #region ResourceStructure in Resource

        public ActionResult LoadResourceStructure(long selectedResourceStructureId)
        {
            EditResourceModel model = (EditResourceModel)Session["Resource"];

            using (ResourceStructureManager rsManager = new ResourceStructureManager())
            using (ResourceStructureAttributeManager rsaManager = new ResourceStructureAttributeManager())
            {
                if (selectedResourceStructureId != 0)
                {
                    model.TextValues.Clear();
                    model.FileValues.Clear();
                    model.ResourceStructureAttributeValues.Clear();

                    ResourceStructure rs = rsManager.GetResourceStructureById(selectedResourceStructureId);
                    model.ResourceStructure = new ResourceStructureModel(rs);

                    //get all attr also from the parent RS
                    List<ResourceAttributeUsage> temp = GetAllAttributes(rs);

                    // default values loading, default values needed????
                    foreach (ResourceAttributeUsage attrUsage in temp)
                    {
                        ResourceStructureAttributeValueModel rsav = new ResourceStructureAttributeValueModel();
                        ResourceStructureAttributeUsageModel rsaUsage = new ResourceStructureAttributeUsageModel(attrUsage.Id, attrUsage.ResourceStructureAttribute.Id, null);

                        if (rsaUsage.IsFileDataType == false)
                        {
                            TextValueModel tv = new TextValueModel();
                            tv.AttributeName = rsaUsage.ResourceAttributeName;
                            tv.ResourceAttributeUsageId = rsaUsage.UsageId;
                            tv.DomainConstraint = rsaUsage.DomainConstraint;
                            tv.ResourceAttributeUsage = new ResourceAttributeUsageModel(attrUsage);
                            tv.EditMode = true;
                            model.TextValues.Add(tv);
                            model.ResourceStructureAttributeValues.Add(tv);
                        }
                        else
                        {
                            FileValueModel fv = new FileValueModel();
                            fv.AttributeName = rsaUsage.ResourceAttributeName;
                            fv.ResourceAttributeUsageId = rsaUsage.UsageId;
                            fv.ResourceAttributeUsage = new ResourceAttributeUsageModel(attrUsage);
                            fv.EditMode = true;
                            model.FileValues.Add(fv);
                            model.ResourceStructureAttributeValues.Add(fv);
                        }
                    }
                }
                //Add Attribute without set values, eg. if value is optional 
                else
                {
                    ResourceStructure rs = rsManager.GetResourceStructureById(model.ResourceStructure.Id);
                    List<ResourceAttributeUsage> temp = GetAllAttributes(rs);
                    foreach (ResourceAttributeUsage u in temp)
                    {
                        if (!model.ResourceStructureAttributeValues.Select(a => a.ResourceAttributeUsageId).ToList().Contains(u.Id))
                        {
                            ResourceStructureAttributeValueModel rsav = new ResourceStructureAttributeValueModel();
                            ResourceStructureAttributeUsageModel rsaUsage = new ResourceStructureAttributeUsageModel(u.Id, u.ResourceStructureAttribute.Id, null);

                            if (rsaUsage.IsFileDataType == false)
                            {
                                TextValueModel tv = new TextValueModel();
                                tv.AttributeName = rsaUsage.ResourceAttributeName;
                                tv.ResourceAttributeUsageId = rsaUsage.UsageId;
                                tv.DomainConstraint = rsaUsage.DomainConstraint;
                                tv.EditMode = true;
                                model.TextValues.Add(tv);
                                model.ResourceStructureAttributeValues.Add(tv);
                            }
                            else
                            {
                                FileValueModel fv = new FileValueModel();
                                fv.AttributeName = rsaUsage.ResourceAttributeName;
                                fv.ResourceAttributeUsageId = rsaUsage.UsageId;
                                fv.EditMode = true;
                                model.FileValues.Add(fv);
                                model.ResourceStructureAttributeValues.Add(fv);
                            }
                        }
                    }

                }

                Session["Resource"] = model;

                return PartialView("_fillResourceStructure", model.ResourceStructureAttributeValues);
            }
        }

        private List<ResourceAttributeUsage> GetAllAttributes(ResourceStructure rs)
        {
            List<ResourceAttributeUsage> temp = new List<ResourceAttributeUsage>();

            if (rs.ResourceAttributeUsages.Count() > 0)
            {
                temp.AddRange(rs.ResourceAttributeUsages);
            }

            if (rs.Parent != null)
                temp.AddRange(GetAllAttributes(rs.Parent));

            return temp;
        }

        #endregion

        #region Resource Manager

        [GridAction]
        public ActionResult Resource_Select()
        {
            using (var rManager = new ResourceManager())
            using (var permissionManager = new EntityPermissionManager())
            using (var entityTypeManager = new EntityManager())
            {
                IQueryable<SingleResource> data = rManager.GetAllResources();
                List<ResourceManagerModel> resources = new List<ResourceManagerModel>();

                long userId = UserHelper.GetUserId(HttpContext.User.Identity.Name);
                Entity entity = entityTypeManager.FindByName("SingleResource");

                foreach (SingleResource r in data)
                {
                    ResourceManagerModel temp = new ResourceManagerModel(r);
                    temp.InUse = rManager.IsResourceInSet(r.Id);

                    //get permission from logged in user
                    temp.EditAccess = permissionManager.HasEffectiveRight(userId, new List<long>() { entity.Id}, r.Id, RightType.Write);
                    temp.DeleteAccess = permissionManager.HasEffectiveRight(userId, new List<long>() { entity.Id }, r.Id, RightType.Delete);

                    resources.Add(temp);
                }

                //ResourceManagerModel temp = new ResourceManagerModel();
                //data.ToList().ForEach(r => resources.Add(temp.Convert(r)));

                return View("ResourceManager", new GridModel<ResourceManagerModel> { Data = resources });
            }
        }

        #endregion

        #region Resource Constraints

        private void ValidateConstraint(ResourceConstraintModel constraint)
        {
            if (constraint is DependencyConstraintModel)
            {
                DependencyConstraintModel dcModel = (DependencyConstraintModel)constraint;
                if (dcModel.ObjectId == null)
                    ModelState.AddModelError("Errors", "Dependency Constraint: Please select a dependent resource.");
                if(dcModel.Quantity < 1)
                    ModelState.AddModelError("Errors", "Dependency Constraint: Quanity of the dependent resource can not be less than 1.");
            }
            if (constraint is BlockingConstraintModel)
            {
                BlockingConstraintModel bcModel = (BlockingConstraintModel)constraint;

                if (bcModel.ForPeriodicTimeInterval.IsSet)
                {
                    if (bcModel.ForTimeInterval.StartTime.Instant == null)
                    {
                        ModelState.AddModelError("Errors", "Blocking Constraint: Please select a Start Time.");
                    }
                }
                else
                {
                    if ((bcModel.ForPersons.Count() == 0 && bcModel.AllUsers == false) && ((bcModel.ForTimeInterval.StartTime.Instant < DateTime.Now && bcModel.ForTimeInterval.EndTime.Instant < DateTime.Now) && bcModel.ForEver == false))
                        ModelState.AddModelError("Errors", "Blocking Constraint: Please select Persons or a time period for what the resource is blocked.");
                    if (bcModel.ForTimeInterval.StartTime.Instant != null && bcModel.ForTimeInterval.EndTime.Instant != null)
                    {
                        if (CheckDateInconsistency((DateTime)bcModel.ForTimeInterval.StartTime.Instant, (DateTime)bcModel.ForTimeInterval.EndTime.Instant) && bcModel.ForEver == false)
                            ModelState.AddModelError("Errors", "Blocking Constraint: The end date is befor start date.");
                    }
                }
            }
            if (constraint is QuantityConstraintModel)
            {
                QuantityConstraintModel qcModel = (QuantityConstraintModel)constraint;

                if (qcModel.ForPeriodicTimeInterval.IsSet)
                {
                    if (qcModel.ForTimeInterval.StartTime.Instant == null)
                    {
                        ModelState.AddModelError("Errors", "Quantity Constraint: Please select a Start Time.");
                    }
                }

                    if (qcModel.ForTimeInterval.StartTime.Instant != null && qcModel.ForTimeInterval.EndTime.Instant != null)
                    {
                        if (CheckDateInconsistency((DateTime)qcModel.ForTimeInterval.StartTime.Instant, (DateTime)qcModel.ForTimeInterval.EndTime.Instant))
                            ModelState.AddModelError("Errors", "Quantity Constraint: The end date is befor start date.");
                    }

                if (qcModel.Quantity ==0)
                    ModelState.AddModelError("Errors", "Quantity Constraint: If you what to set a quantity constraint the quantity must be grater then 0.");
             
            }
        }


        private bool CheckDateInconsistency(DateTime startDate, DateTime endDate)
        {
            if (startDate > endDate)
            {
                return true;
            }

            return false;
        }

        // constaint for general info e.g. desc., specialConstaint specific constrain values inside
        private void SaveConstraint(Resource resource, int index)
        {
            EditResourceModel model = (EditResourceModel)Session["Resource"];
            ResourceConstraintModel rcm = model.ResourceConstraints.Where(p => p.Index == index).FirstOrDefault();

            using (TimeManager timeManager = new TimeManager())
            using (TimeManager tManager = new TimeManager())
            {

                //Create PeriodicTimeInterval
                PeriodicTimeInterval ptInterval = new PeriodicTimeInterval();
                PeriodicTimeInstant ptInstanz = new PeriodicTimeInstant();

                if (rcm.ForPeriodicTimeInterval != null)
                {
                  
                    if (rcm.ForPeriodicTimeInterval.IsSet)
                    {
                        ptInstanz = rcm.ForPeriodicTimeInterval.PeriodicTimeInstant;
                        ptInterval.PeriodicTimeInstant = ptInstanz;
                        ptInterval.Duration = rcm.ForPeriodicTimeInterval.Duration;
                    }
                    else
                    {
                        if (rcm.ForPeriodicTimeInterval.Id != 0)
                            tManager.DeletePeriodicTimeInterval(tManager.GetPeriodicTimeIntervalById(rcm.ForPeriodicTimeInterval.Id));
                    }
                }

                using (ResourceConstraintManager rcManager = new ResourceConstraintManager())
                using (ResourceManager srManager = new ResourceManager())
                {
                    if (rcm is DependencyConstraintModel)
                    {
                        DependencyConstraintModel dcm = model.DependencyConstraints.Where(p => p.Index == index).FirstOrDefault();

                        DependencyConstraint dc = new DependencyConstraint();
                        if (dcm.Id != 0)
                            dc = rcManager.GetDependencyConstraintById(dcm.Id);

                        //if (dcm.SelectedType == "Single Resource")
                        //{
                        SingleResource singleResource = srManager.GetResourceById(Convert.ToInt64(dcm.ObjectId));
                        dc.ForResource = singleResource;
                        //}
                        //else if (dcm.SelectedType == "Resource Group")
                        //{
                        //    ResourceGroup resourceGroup = srManager.GetResourceGroupById(Convert.ToInt64(dcm.ObjectId));
                        //    dc.ForResource = resourceGroup;
                        //}

                        dc.Implicit = dcm.Implicit;
                        dc.Quantity = dcm.Quantity;
                        dc.Index = dcm.Index;

                        dc.Negated = rcm.Negated;
                        dc.Description = rcm.Description;
                        dc.Mode = rcm.SelectedMode;


                        if (dcm.Id == 0)
                        {
                            dc.Resource = resource;
                            rcManager.SaveConstraint(dc);
                        }
                        else
                            rcManager.UpdateConstraint(dc);
                    }
                    else if (rcm is BlockingConstraintModel)
                    {
                        BlockingConstraintModel bcm = model.BlockingConstraints.Where(a => a.Index == index).FirstOrDefault();

                        BlockingConstraint bc = new BlockingConstraint();
                        if (bcm.Id != 0)
                            bc = rcManager.GetBlockingConstraintById(bcm.Id);
                        //needed for delete later after update constraint
                        PeriodicTimeInterval tempPeriodicTimeInterval = bc.ForPeriodicTimeInterval;

                        bc.Negated = rcm.Negated;
                        bc.Description = rcm.Description;
                        bc.Mode = rcm.SelectedMode;
                        bc.Index = bcm.Index;

                        bc.ForEver = bcm.ForEver;
                        bc.AllUsers = bcm.AllUsers;

                        //create TimeIntervall and Person before save constraint
                        if (bcm.Id == 0)
                        {
                            bc.Resource = resource;
                            if (rcm.ForTimeInterval.StartTime.Instant != null)
                                bc.ForTimeInterval = this.CreateTimeInterval(rcm.ForTimeInterval);

                            //if (bcm.ForPersons.Count() > 0)
                            //{
                            if (rcm.ForTimeInterval.StartTime.Instant != null)
                                bc.ForTimeInterval = this.CreateTimeInterval(rcm.ForTimeInterval);
                            //}

                            if (bcm.ForPersons.Count() != 0)
                                bc.ForPerson = CreatePerson(bcm.ForPersons);

                            if (rcm.ForPeriodicTimeInterval != null)
                                if (rcm.ForPeriodicTimeInterval.IsSet)
                                    bc.ForPeriodicTimeInterval = ptInterval;

                            rcManager.SaveConstraint(bc);
                        }
                        else
                        {
                            if (bc.ForTimeInterval != null)
                                bc.ForTimeInterval = timeManager.UpdateTimeInterval(bcm.ForTimeInterval);

                            if (rcm.ForPeriodicTimeInterval != null)
                            {
                                if (rcm.ForPeriodicTimeInterval.IsSet)
                                    bc.ForPeriodicTimeInterval = ptInterval;
                            }
                            else
                                bc.ForPeriodicTimeInterval = null;

                            //if person exstists and the persons form the UI is grater then 0 then update person
                            if (bcm.ForPersons.Count() > 0)
                                bc.ForPerson = UpdatePerson(bcm.ForPersons);

                            //if person exsist and count auf Persons from UI is 0 then delete the Person
                            else if (bc.ForPerson != null && bcm.ForPersons.Count() == 0)
                            {
                                using (PersonManager pManager = new PersonManager())
                                {
                                    if (bc.ForPerson is IndividualPerson)
                                        pManager.DeleteIndividualPerson((IndividualPerson)bc.ForPerson.Self);
                                    else
                                        pManager.DeletePersonGroup((PersonGroup)bc.ForPerson.Self);
                                }

                                bc.ForPerson = null;
                            }
                            rcManager.UpdateConstraint(bc);

                            //delete PeriodicTimeInterval if exsist bevor and where deleted
                            if (bc.ForPeriodicTimeInterval == null && tempPeriodicTimeInterval != null)
                            {
                                timeManager.DeletePeriodicTimeInterval(tempPeriodicTimeInterval);
                            }
                        }
                    }
                    else if (rcm is QuantityConstraintModel)
                    {
                        QuantityConstraintModel qcm = model.QuantityConstraints.Where(e => e.Index == index).FirstOrDefault();

                        QuantityConstraint qc = new QuantityConstraint();
                        if (qcm.Id != 0)
                            qc = rcManager.GetQuantityConstraintById(qcm.Id);

                        //needed for delete later after update constraint
                        PeriodicTimeInterval tempPeriodicTimeInterval = qc.ForPeriodicTimeInterval;

                        qc.Quantity = qcm.Quantity;
                        qc.Index = qcm.Index;

                        qc.Negated = rcm.Negated;
                        qc.Description = rcm.Description;
                        qc.Mode = rcm.SelectedMode;

                        qc.ForEver = qcm.ForEver;
                        qc.AllUsers = qcm.AllUsers;


                        if (rcm.ForPeriodicTimeInterval.IsSet)
                            qc.ForPeriodicTimeInterval = ptInterval;


                        //create TimeIntervall and Person before save constraint
                        if (qcm.Id == 0)
                        {
                            qc.Resource = resource;
                            if (qcm.ForTimeInterval.StartTime.Instant != null)
                                qc.ForTimeInterval = this.CreateTimeInterval(qcm.ForTimeInterval);

                            //if (qcm.ForPersons.Count() > 0)
                            //    {
                            //        if (qcm.ForTimeInterval != null)
                            //            qc.ForTimeInterval = this.CreateTimeInterval(qcm.ForTimeInterval);
                            //    }

                            if (qcm.ForPersons.Count() != 0)
                                qc.ForPerson = this.CreatePerson(qcm.ForPersons);

                            rcManager.SaveConstraint(qc);
                        }
                        else
                        {
                            qc.ForTimeInterval = timeManager.UpdateTimeInterval(qcm.ForTimeInterval);

                            //if person exstists and the persons form the UI is grater then 0 then update person
                            if (qcm.ForPersons.Count() > 0)
                                qc.ForPerson = UpdatePerson(qcm.ForPersons);
                            //if person exsist and count auf Persons from UI is 0 then delete the Person
                            else if (qc.ForPerson != null && qcm.ForPersons.Count() == 0)
                            {
                                using (PersonManager pManager = new PersonManager())
                                {
                                    pManager.DeletePerson(qc.ForPerson);
                                    qc.ForPerson = null;
                                }
                            }
                            rcManager.UpdateConstraint(qc);

                            //delete PeriodicTimeInterval if exsist bevor and where deleted
                            if (qc.ForPeriodicTimeInterval == null && tempPeriodicTimeInterval != null)
                            {
                                timeManager.DeletePeriodicTimeInterval(tempPeriodicTimeInterval);
                            }
                        }
                    }
                }
            }
        }

        private TimeInterval CreateTimeInterval(TimeInterval timeIntervall)
        {
            using (TimeManager timeManager = new TimeManager())
            {
                TimeInstant start = timeManager.CreateTimeInstant(timeIntervall.StartTime.Precision, (DateTime)timeIntervall.StartTime.Instant);
                TimeInstant end = timeManager.CreateTimeInstant(timeIntervall.EndTime.Precision, timeIntervall.EndTime.Instant);

                return timeManager.CreateTimeInterval(start, end);
            }  
        }

        private Person CreatePerson(List<PersonInConstraint> forPersons)
        {
            using (SubjectManager subManager = new SubjectManager())
            using (PersonManager pManager = new PersonManager())
            {
                if (forPersons.Count() > 1)
                {
                    List<User> users = new List<User>();
                    foreach (PersonInConstraint user in forPersons)
                    {
                        User u = subManager.Subjects.Where(a => a.Id == user.UserId).FirstOrDefault() as User;
                        users.Add(u);
                    }
                    return pManager.CreatePersonGroup(users);
                }
                else
                {
                    User u = subManager.Subjects.Where(a => a.Id == forPersons[0].UserId).FirstOrDefault() as User;
                    return pManager.CreateIndividualPerson(u);
                }
            }
        }

        private Person UpdatePerson(List<PersonInConstraint> forPersons)
        {
            using (UserManager userManager = new UserManager())
            using (var pManager = new PersonManager())
            {
                Person newPerson = new Person();

                if (forPersons.Count() > 1)
                {
                    //exsists a PersonsGroup? is there any person with a id
                    var e = forPersons.Where(a => a.Id != 0).ToList();
                    if (e.Count() > 0)
                    {
                        Person person = pManager.GetPersonById(e[0].Id);
                        PersonGroup pGroup = new PersonGroup();

                        List<User> users = new List<User>();
                        foreach (PersonInConstraint p in forPersons)
                        {
                            users.Add(userManager.FindByIdAsync(p.UserId).Result);
                        }
                         
                        if (person.Self is IndividualPerson)
                        {
                            IndividualPerson iPerson = (IndividualPerson)person.Self;
                            forPersons.Remove(e.Where(a => a.Id == person.Self.Id).FirstOrDefault());
                            pManager.DeletePerson(iPerson);
                            newPerson = pManager.CreatePersonGroup(users);
                        }
                        else
                        {
                            pGroup = (PersonGroup)person.Self;
                            pGroup.Users = users;
                            newPerson = pManager.UpdatePersonGroup(pGroup);
                        }
                    }
                    else
                    {
                        newPerson = CreatePerson(forPersons);
                    }
                }
                else
                {
                    if (forPersons[0].Id != 0)
                    {
                        Person person = pManager.GetPersonById(forPersons[0].Id);
                        if (person.Self is PersonGroup)
                        {
                            //PersonGroup pG = (PersonGroup)person;
                            //pManager.DeletePersonGroup(pG);
                            newPerson = pManager.CreateIndividualPerson(userManager.FindByIdAsync(forPersons[0].UserId).Result);
                        }
                        else if (person.Self is IndividualPerson)
                        {
                            IndividualPerson iPerson = pManager.GetIndividualPersonById(forPersons[0].Id);
                            iPerson.Person = userManager.FindByIdAsync(forPersons[0].UserId).Result;

                            newPerson = pManager.UpdateIndividualPerson(iPerson);
                        }
                    }
                    else
                    {
                        newPerson = pManager.CreateIndividualPerson(userManager.FindByIdAsync(forPersons[0].UserId).Result);
                    }
                }

                return newPerson;
            }
        }

        public ActionResult LoadUsers(string index)
        {
            EditResourceModel model = (EditResourceModel)Session["Resource"];

            using (var partyManager = new PartyManager())
            using (var partyTypeManager = new PartyTypeManager())
            using (UserManager userManager = new UserManager())
            {

                //get party type person
                var partyType = partyTypeManager.PartyTypes.Where(p => p.Title == "Person").FirstOrDefault();
                //get all parties with person party type
                List<Party> partyPersons = partyManager.PartyRepository.Query(p => p.PartyType == partyType).ToList();

                List<PersonInConstraint> personListSelected = new List<PersonInConstraint>();
                List<PersonInConstraint> personList = new List<PersonInConstraint>();
                ResourceConstraintModel tempConstraint = model.ResourceConstraints.Where(a => a.Index == int.Parse(index)).FirstOrDefault();
                var users = userManager.Users;

                foreach (var user in users)
                {
                   // var userTask = userManager.FindByIdAsync(partyManager.GetUserIdByParty(partyPerson.Id));
                   // userTask.Wait();
                   // var user = userTask.Result;
                  
                    PersonInConstraint pc = new PersonInConstraint(user, 0, int.Parse(index));
                    pc.UserFullName = user.DisplayName;
                    pc.Index = int.Parse(index);
                    if (tempConstraint.ForPersons.Select(a => a.UserId).ToList().Contains(pc.UserId))
                    {
                        pc.Id = tempConstraint.ForPersons.Where(a => a.UserId == pc.UserId).FirstOrDefault().Id;
                        pc.IsSelected = true;
                        personListSelected.Add(pc);
                    }

                    personList.Add(pc);
                }

                tempConstraint.ForPersons = personListSelected;

                var i = model.ResourceConstraints.FindIndex(p => p.Index == int.Parse(index));
                model.ResourceConstraints[i] = tempConstraint;

                Session["Resource"] = model;

                return PartialView("_chooseUsers", personList);

            }

            //foreach (User u in users)
            //{
            //    PersonInConstraint pc = new PersonInConstraint(u, 0, int.Parse(index));
            //    pc.Index = int.Parse(index);
            //    if (tempConstraint.ForPersons.Select(a=>a.UserId).ToList().Contains(pc.UserId))
            //    {
            //        pc.Id = tempConstraint.ForPersons.Where(a => a.UserId == pc.UserId).FirstOrDefault().Id;
            //        pc.IsSelected = true;
            //        personListSelected.Add(pc);
            //    }

            //    personList.Add(pc);
            //}

           
        }

        public ActionResult ChangeSelectedUserConstraint(string userId, string selected, string index)
        {
            EditResourceModel model = (EditResourceModel)Session["Resource"];
            ResourceConstraintModel tempConstraint = model.ResourceConstraints.Where(p=>p.Index == int.Parse(index)).FirstOrDefault();

            using (UserManager userManager = new UserManager())
            using (var pManager = new PersonManager())
            {
                User user = userManager.FindByIdAsync(Convert.ToInt64(userId)).Result;

                if (selected == "true")
                {
                    tempConstraint.ForPersons.Add(new PersonInConstraint(user, 0, int.Parse(index)));
                }
                else
                {
                    int i = tempConstraint.ForPersons.FindIndex(a => a.UserId == Convert.ToInt64(userId));
                    tempConstraint.ForPersons.RemoveAt(i);
                }

                var pos = model.ResourceConstraints.FindIndex(p => p.Index == int.Parse(index));
                model.ResourceConstraints[pos] = tempConstraint;

                Session["Resource"] = model;
            }

            return View();
        }

        public ActionResult ShowUsersInConstraint(string index)
        {
            EditResourceModel model = (EditResourceModel)Session["Resource"];
            List<ResourceConstraintModel> tempListContraints = model.ResourceConstraints;

            ResourceConstraintModel tempConstraint = tempListContraints.Where(a => a.Index == int.Parse(index)).FirstOrDefault();

            tempConstraint.ForPersons.ForEach(a =>
            {
                Party partyPerson = UserHelper.GetPartyByUserId(a.UserId);
                a.UserFullName = partyPerson.Name;
            });

            if (tempConstraint.ForPersons != null)
                return PartialView("_constraintUsers", tempConstraint.ForPersons);
            else
                return new EmptyResult();
        }

        public ActionResult ShowAllUsers(string index)
        {
            EditResourceModel model = (EditResourceModel)Session["Resource"];
            ResourceConstraintModel tempConstraint = model.ResourceConstraints.Where(a => a.Index == int.Parse(index)).FirstOrDefault();
            tempConstraint.ForPersons.ForEach(a =>
            {
                Party partyPerson = UserHelper.GetPartyByUserId(a.UserId);
                a.UserFullName = partyPerson.Name;
            });

            return PartialView("_showUsers", tempConstraint.ForPersons);
        }
            
        public ActionResult AddUsersToConstraint(string index)
        {
            EditResourceModel model = (EditResourceModel)Session["Resource"];
            ResourceConstraintModel tempConstraint = model.ResourceConstraints.Where(p => p.Index == int.Parse(index)).FirstOrDefault();

            return PartialView("_constraintUsers", tempConstraint.ForPersons);
        }

        //Remove user from a constraint
        public ActionResult RemoveUserFromConstraint(string userId, string index, string source)
        {
            EditResourceModel model = (EditResourceModel)Session["Resource"];
            ResourceConstraintModel tempConstraint = model.ResourceConstraints.Where(p => p.Index == int.Parse(index)).FirstOrDefault();

            tempConstraint.ForPersons.RemoveAll(a => a.UserId == int.Parse(userId));

            var i = model.ResourceConstraints.FindIndex(p => p.Index == int.Parse(index));
            model.ResourceConstraints[i] = tempConstraint;

            Session["Resource"] = model;

            if(source=="page")
                return PartialView("_constraintUsers", tempConstraint.ForPersons);
            else
                return PartialView("_showUsers", tempConstraint.ForPersons);

        }


        public ActionResult AddResourceConstraint(string type)
        {
            EditResourceModel model = (EditResourceModel)Session["Resource"];
            List<ResourceConstraintModel> tempList = model.ResourceConstraints;

            if (tempList == null)
                tempList = new List<ResourceConstraintModel>();

            var a = tempList.Take(tempList.Count()).LastOrDefault();
            int index = 0;
            if(a != null)
                index = (int)a.Index + 1;

            switch (type)
            {
                case "Blocking":
                    BlockingConstraintModel bcModel = new BlockingConstraintModel();
                    bcModel.SelectedType = type;
                    bcModel.Index = index;
                    //tempList.Add(bcModel);
                    model.BlockingConstraints.Add(bcModel);
                    model.ResourceConstraints.Add(bcModel);
                    //tempResource.ResourceConstraints.AddRange(tempList);
                    Session["Resource"] = model;
                    //Session["ResourceConstraints"] = tempList;
                    return PartialView("_rowConstraint", bcModel);
                case "Quantity":
                    QuantityConstraintModel qcModel = new QuantityConstraintModel();
                    qcModel.SelectedType = type;
                    qcModel.Index = index;
                    //tempList.Add(qcModel);
                    model.QuantityConstraints.Add(qcModel);
                    model.ResourceConstraints.Add(qcModel);
                    //tempResource.ResourceConstraints.AddRange(tempList);
                    Session["Resource"] = model;
                    //Session["ResourceConstraints"] = tempList;
                    return PartialView("_rowConstraint", qcModel);
                case "Dependency":
                    DependencyConstraintModel dcModel = new DependencyConstraintModel();
                    dcModel.SelectedType = type;
                    dcModel.Index = index;
                    //tempList.Add(bcModel);
                    model.DependencyConstraints.Add(dcModel);
                    model.ResourceConstraints.Add(dcModel);
                    //tempResource.ResourceConstraints.AddRange(tempList);
                    Session["Resource"] = model;
                    //Session["ResourceConstraints"] = tempList;
                    return PartialView("_rowConstraint", dcModel);
                default:
                    ResourceConstraintModel rcModel = new ResourceConstraintModel();
                    rcModel.SelectedType = type;
                    rcModel.Index = index;
                    //tempList.Add(rcModel);
                    //tempResource.ResourceConstraints.AddRange(tempList);
                    Session["Resource"] = model;
                    //Session["ResourceConstraints"] = tempList;
                    return PartialView("_rowConstraint", rcModel);
            }
        }

        public ActionResult RemoveResourceConstraint(string index)
        {
            EditResourceModel model = (EditResourceModel)Session["Resource"];

            ResourceConstraintModel tempC = model.ResourceConstraints.Where(p => p.Index == Convert.ToInt64(index)).FirstOrDefault();
            if(tempC is DependencyConstraintModel)
            {
                var dr = model.DependencyConstraints.Where(p => p.Index == Convert.ToInt64(index)).FirstOrDefault();
                if (dr != null)
                    model.DependencyConstraints.Remove(dr);
            }
            if(tempC is QuantityConstraintModel)
            {
                var qr = model.QuantityConstraints.Where(p => p.Index == Convert.ToInt64(index)).FirstOrDefault();
                if (qr != null)
                    model.QuantityConstraints.Remove(qr);
            }
            if(tempC is BlockingConstraintModel)
            {
                var br = model.BlockingConstraints.Where(p => p.Index == Convert.ToInt64(index)).FirstOrDefault();
                if (br != null)
                    model.BlockingConstraints.Remove(br);
            }

            //var itemToRemove = tempC;
            //if (itemToRemove != null)
            //    model.ResourceConstraints.Remove(itemToRemove);

            model.ResourceConstraints.Where(a => a.Index == long.Parse(index)).FirstOrDefault().Deleted = true;

            Session["Resource"] = model;

            return View();
        }


        public ActionResult EditResourceConstraint(string index)
        {
            EditResourceModel model = (EditResourceModel)Session["Resource"];
            ResourceConstraintModel rcModel = model.ResourceConstraints.Where(p => p.Index == int.Parse(index)).FirstOrDefault();

            if (rcModel is DependencyConstraintModel)
            {
                rcModel.SelectedType = "Dependency";
                DependencyConstraintModel dcModel = (DependencyConstraintModel)rcModel;
                int i = model.ResourceConstraints.FindIndex(c => c.Index == int.Parse(index));
                model.ResourceConstraints[i] = dcModel;
            }

            if (rcModel is BlockingConstraintModel)
            {
                rcModel.SelectedType = "Blocking";
                BlockingConstraintModel bcModel = (BlockingConstraintModel)rcModel;
                int i = model.ResourceConstraints.FindIndex(c => c.Index == int.Parse(index));
                model.ResourceConstraints[i] = bcModel;
            }

            if (rcModel is QuantityConstraintModel)
            {
                rcModel.SelectedType = "Quantity";
            }

            

            Session["Resource"] = model;

            return PartialView("_rowConstraint", rcModel);
        }


        public ActionResult ChooseDependencyObject(string selectedObject, int index)
        {
            using (ResourceManager rManager = new ResourceManager())
            {
                switch (selectedObject)
                {
                    case "Single Resource":

                        List<SingleResource> resources = rManager.GetAllResources().ToList();
                        List<ResourceManagerModel> listSingleResources = new List<ResourceManagerModel>();

                        foreach (SingleResource sr in resources)
                        {
                            ResourceManagerModel rmm = new ResourceManagerModel(sr);
                            rmm.Index = index;
                            listSingleResources.Add(rmm);
                        }

                        //resources.ToList().ForEach(r => listSingleResources.Add(new ResourceManagerModel(r)));
                        return PartialView("_chooseSingleResource", listSingleResources);
                    case "Resource Group":
                        List<ResourceGroup> resourceGroups = rManager.GetAllResourceGroups().ToList();
                        List<ResourceGroupManagerModel> listResourceGroups = new List<ResourceGroupManagerModel>();
                        resourceGroups.ToList().ForEach(r => listResourceGroups.Add(new ResourceGroupManagerModel(r)));
                        return PartialView("_chooseResourceGroup", listResourceGroups);
                    default:
                        return PartialView("");
                }
            }
        }

        public ActionResult OpenRepetition(string index)
        {
            EditResourceModel resourceModel = (EditResourceModel)Session["Resource"];
            PeriodicTimeIntervalModel model = new PeriodicTimeIntervalModel();

            ResourceConstraintModel rcm = resourceModel.ResourceConstraints.Where(a => a.Index == int.Parse(index)).FirstOrDefault();

            if (rcm.ForPeriodicTimeInterval.IsSet && rcm.ForPeriodicTimeInterval.Id == 0)
            {
                List<int> ids = new List<int>();

                //ids.Add(periodicTimeInterval.PeriodicTimeInstant.Off_Set);

                if (rcm.ForPeriodicTimeInterval.SelectedDays.Count() >0)
                {
                    for (int j = 0; j < rcm.ForPeriodicTimeInterval.Duration.Value; j++)
                    {
                        ids.Add(rcm.ForPeriodicTimeInterval.PeriodicTimeInstant.Off_Set + j);
                    }
                    string days = "";
                    int count = 0;
                    foreach (int id in ids)
                    {
                        count++;
                        var temp = rcm.ForPeriodicTimeInterval.Days.Where(a => a.Id == id).FirstOrDefault();
                        if (count == ids.Count())
                            days += temp.Name;
                        else
                            days += temp.Name + ", ";

                        rcm.ForPeriodicTimeInterval.Days.Where(w => w.Id == id).ToList().ForEach(s => s.Checked = true);
                    }
                }

                return PartialView("_editPeriodicTimeInterval", rcm.ForPeriodicTimeInterval);
            }

            if (rcm.ForPeriodicTimeInterval.Id != 0)
            {
                rcm.ForPeriodicTimeInterval.IsSet = true;
                model = rcm.ForPeriodicTimeInterval;
            }
            else
            {
                //DateTime value = new DateTime(1888, 1, 18);
                if (rcm is BlockingConstraintModel)
                {
                    BlockingConstraintModel bcm = (BlockingConstraintModel)rcm;
                    if (bcm.ForTimeInterval.StartTime.Instant != null)
                        model.StartDate = bcm.ForTimeInterval.StartTime.Instant;
                    if (bcm.ForTimeInterval.EndTime != null)
                        model.EndDate = bcm.ForTimeInterval.EndTime.Instant;
                }
                if (rcm is QuantityConstraintModel)
                {
                    QuantityConstraintModel qcm = (QuantityConstraintModel)rcm;
                    if (qcm.ForTimeInterval.StartTime.Instant!= null)
                        model.StartDate = qcm.ForTimeInterval.StartTime.Instant;
                    if (qcm.ForTimeInterval.EndTime.Instant!= null)
                        model.EndDate = qcm.ForTimeInterval.EndTime.Instant;
                }

                model.Days = TimeHelper.GetDays();

                model.IsSet = true;
                model.Index = int.Parse(index);
                rcm.ForPeriodicTimeInterval = model;
                var i = resourceModel.ResourceConstraints.FindIndex(a => a.Index == int.Parse(index));
                resourceModel.ResourceConstraints[i] = rcm;
                Session["Resource"] = resourceModel;
            }

            model.ForEverIsSet = rcm.ForEver;

            return PartialView("_editPeriodicTimeInterval", model);
        }

        public ActionResult AddRepetition(int index)
        {
            EditResourceModel model = (EditResourceModel)Session["Resource"];
            ResourceConstraintModel rcm = model.ResourceConstraints.Where(p=>p.Index == index).FirstOrDefault();

            TimeDuration timeDuration = new TimeDuration();

            switch (rcm.ForPeriodicTimeInterval.PeriodicTimeInstant.ResetFrequency)
            {
                case ResetFrequency.Daily:

                    //check if days are checked and delete the list of days
                    if (rcm.ForPeriodicTimeInterval.SelectedDays != null)
                        rcm.ForPeriodicTimeInterval.SelectedDays.Clear();

                    rcm.ForPeriodicTimeInterval.PeriodicTimeInstant.Off_Set = rcm.ForPeriodicTimeInterval.StartTime.Hour;
                    rcm.ForPeriodicTimeInterval.PeriodicTimeInstant.Off_Set_Unit = SystemDefinedUnit.hour;
                    timeDuration.TimeUnit = SystemDefinedUnit.hour;

                    timeDuration.Value = TimeHelper.GetDifferenceInHours(rcm.ForPeriodicTimeInterval.StartTime, rcm.ForPeriodicTimeInterval.EndTime);
                    if (rcm.ForPeriodicTimeInterval.PeriodicTimeInstant.ResetInterval <= 1)
                        rcm.ForPeriodicTimeInterval.Summary = "Daily";
                    else
                        rcm.ForPeriodicTimeInterval.Summary = String.Format("Every {0} days, start at {1} and end at {2}", rcm.ForPeriodicTimeInterval.PeriodicTimeInstant.ResetInterval, rcm.ForPeriodicTimeInterval.StartTime.ToString("HH:mm"), rcm.ForPeriodicTimeInterval.EndTime.ToString("HH:mm"));

                    break;
                case ResetFrequency.Weekly:

                    rcm.ForPeriodicTimeInterval.PeriodicTimeInstant.Off_Set = rcm.ForPeriodicTimeInterval.SelectedDays.Min();
                    rcm.ForPeriodicTimeInterval.PeriodicTimeInstant.Off_Set_Unit = SystemDefinedUnit.day;
                    timeDuration.TimeUnit = SystemDefinedUnit.day;
                    int min = rcm.ForPeriodicTimeInterval.SelectedDays.Min();
                    int max = rcm.ForPeriodicTimeInterval.SelectedDays.Max();
                    timeDuration.Value = max - min +1;

                    string days = "";
                    List<CheckModel> allDays = TimeHelper.GetDays();
                    int count = 0;
                    foreach (int d in rcm.ForPeriodicTimeInterval.SelectedDays)
                    {
                        count++;
                        var temp = allDays.Where(a => a.Id == d).FirstOrDefault();
                        if(count == rcm.ForPeriodicTimeInterval.SelectedDays.Count())
                            days += temp.Name;
                        else
                            days += temp.Name + ", ";
                    }
                    if (rcm.ForPeriodicTimeInterval.PeriodicTimeInstant.ResetInterval <= 1)
                        rcm.ForPeriodicTimeInterval.Summary = String.Format("Weekly on {0}", days);
                    else
                        rcm.ForPeriodicTimeInterval.Summary = String.Format("Every {0} weeks on {1}", rcm.ForPeriodicTimeInterval.PeriodicTimeInstant.ResetInterval, days);
                    break;
                case ResetFrequency.Monthly:

                    //check if days are checked and delete the list of days
                    if (rcm.ForPeriodicTimeInterval.SelectedDays != null)
                        rcm.ForPeriodicTimeInterval.SelectedDays.Clear();

                    rcm.ForPeriodicTimeInterval.PeriodicTimeInstant.Off_Set = DateTime.Parse(rcm.ForTimeInterval.StartTime.Instant.ToString()).Day;
                    rcm.ForPeriodicTimeInterval.PeriodicTimeInstant.Off_Set_Unit = SystemDefinedUnit.day;
                    timeDuration.TimeUnit = SystemDefinedUnit.month;
                    timeDuration.Value = 1;
                    //timeDuration.Value = TimeHelper.GetDifferenceInDays(DateTime.Parse(rcm.ForTimeInterval.StartTime.Instant.ToString()), DateTime.Parse(rcm.ForTimeInterval.EndTime.Instant.ToString()));
                    if (rcm.ForPeriodicTimeInterval.PeriodicTimeInstant.ResetInterval <= 1)
                        rcm.ForPeriodicTimeInterval.Summary = String.Format("Monthly on day {0}", rcm.ForPeriodicTimeInterval.PeriodicTimeInstant.Off_Set);
                    else
                        rcm.ForPeriodicTimeInterval.Summary = String.Format("Every {0} months on day {1}", rcm.ForPeriodicTimeInterval.PeriodicTimeInstant.ResetInterval, rcm.ForPeriodicTimeInterval.PeriodicTimeInstant.Off_Set);
                    break;
            }

            rcm.ForPeriodicTimeInterval.Duration = timeDuration;

            var i = model.ResourceConstraints.FindIndex(p => p.Index == index);
            model.ResourceConstraints[i] = rcm;

            Session["Resource"] = model;

            return Content(rcm.ForPeriodicTimeInterval.Summary);
        }

        public ActionResult RemoveRepetition(string index)
        {
            EditResourceModel model = (EditResourceModel)Session["Resource"];
            ResourceConstraintModel rcm = model.ResourceConstraints.Where(p => p.Index == int.Parse(index)).FirstOrDefault();
            rcm.ForPeriodicTimeInterval.IsSet = false;

            var i = model.ResourceConstraints.FindIndex(p => p.Index == int.Parse(index));
            model.ResourceConstraints[i] = rcm;

            Session["Resource"] = model;

            return Content("");
        }


        #endregion
    }
}
