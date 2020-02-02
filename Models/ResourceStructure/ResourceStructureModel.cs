using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
//using NHibernate.Linq;
using RSE = BExIS.Rbm.Entities.ResourceStructure;
using BExIS.Rbm.Services.ResourceStructure;
using BExIS.Rbm.Entities.ResourceStructure;
using System.ComponentModel.DataAnnotations;

namespace BExIS.Web.Shell.Areas.RBM.Models.ResourceStructure
{
    public class ResourceStructureModel
    {
        [Display(Name = "ID")]
        [Editable(false)]
        [Required]
        public long Id { get; set; }

        [Display(Name = "Name")]
        [StringLength(50, ErrorMessage = "The resource structure name must be {2} - {1} characters long.", MinimumLength = 3)]
        //[Remote("ValidateResourceStructureName", "ResourceStructures")]
        [Required]
        public string Name { get; set; }

        [Display(Name = "Description")]
        [StringLength(250, ErrorMessage = "The description must be less than {1} characters long.")]
        public string Description { get; set; }

        public ResourceStructureAttributeModel SelectedItem { get; set; }

        //usages with attr properties
        public List<ResourceStructureAttributeUsageModel> ResourceStructureAttributeUsages { get; set; }

        public string Message { get; set; }

        public List<ResourceStructureModel> AllResourceStructures { get; set; }

        public ResourceStructureModel Parent { get; set; }

        public bool FirstCreated { get; set; }

        public ResourceStructureModel()
        {
            Id = 0;
            Name = "";
            Description = "";
            AllResourceStructures = new List<ResourceStructureModel>();
            ResourceStructureAttributeUsages = new List<ResourceStructureAttributeUsageModel>();
            SelectedItem = new ResourceStructureAttributeModel();
            FirstCreated = false;
            //Parent = new ResourceStructureModel();
        }

        public ResourceStructureModel(RSE.ResourceStructure resourceStructure)
        {
            Id = resourceStructure.Id;
            Name = resourceStructure.Name;
            Description = resourceStructure.Description;
            AllResourceStructures = new List<ResourceStructureModel>();
            ResourceStructureAttributeUsages = new List<ResourceStructureAttributeUsageModel>();

            using (ResourceStructureManager rManager = new ResourceStructureManager())
            using (ResourceStructureAttributeManager rsaManager = new ResourceStructureAttributeManager())
            {

                if (resourceStructure.Parent != null)
                {
                    RSE.ResourceStructure parent = rManager.GetResourceStructureById(resourceStructure.Parent.Id);
                    Parent = Convert(parent);
                    //Get Parent attributes(usages) and add it to ResourceStructureAttributeUsages List
                    List<ResourceAttributeUsage> parentUsages = rsaManager.GetResourceStructureAttributeUsagesByRSId(Parent.Id);
                    if (parentUsages.Count > 0)
                    {
                        foreach (ResourceAttributeUsage usage in parentUsages)
                        {
                            ResourceStructureAttributeUsages.Add(new ResourceStructureAttributeUsageModel(usage.Id, usage.ResourceStructureAttribute.Id, Parent.Name));
                        }
                    }
                }
                else
                    Parent = null;


                List<ResourceAttributeUsage> usages = rsaManager.GetResourceStructureAttributeUsagesByRSId(resourceStructure.Id);

                if (usages.Count > 0)
                {
                    foreach (ResourceAttributeUsage usage in usages)
                    {
                        ResourceStructureAttributeUsages.Add(new ResourceStructureAttributeUsageModel(usage.Id, usage.ResourceStructureAttribute.Id, null));
                    }
                }

                SelectedItem = new ResourceStructureAttributeModel();
            }
        }


        //public List<RSE.ResourceStructure> Children { get; set; }

        public void FillRSM()
        {
            using (ResourceStructureManager m = new ResourceStructureManager())
            {
                List<RSE.ResourceStructure> list = m.GetAllResourceStructures().ToList();
                foreach (RSE.ResourceStructure r in list)
                {
                    this.AllResourceStructures.Add(Convert(r));
                }
            }
        }

        public ResourceStructureModel Convert(RSE.ResourceStructure resourceStructure)
        {

            using (ResourceStructureAttributeManager rsaManager = new ResourceStructureAttributeManager())
            {
                List<ResourceAttributeUsage> usages = rsaManager.GetResourceStructureAttributeUsagesByRSId(resourceStructure.Id);
                List<ResourceStructureAttributeUsageModel> list = new List<ResourceStructureAttributeUsageModel>();

                if (usages != null)
                {
                    foreach (ResourceAttributeUsage usage in usages)
                    {
                        list.Add(new ResourceStructureAttributeUsageModel(usage.Id));
                    }
                }

                return new ResourceStructureModel()
                {
                    Id = resourceStructure.Id,
                    Name = resourceStructure.Name,
                    Description = resourceStructure.Description,
                    ResourceStructureAttributeUsages = list,
                    //Children = resourceStructure.Children.ToList()
                    //Parent = new ResourceStructureModel(resourceStructure.Parent),
                    SelectedItem = new ResourceStructureAttributeModel(),
                    AllResourceStructures = new List<ResourceStructureModel>()
                };
            }
        }
    }

    public class CreateResourceStructureModel
    {
        public long Id { get; set; }

        [Display(Name = "Name")]
        [StringLength(50, ErrorMessage = "The resource structure name must be {2} - {1} characters long.", MinimumLength = 3)]
        //[Remote("ValidateResourceStructureName", "ResourceStructures")]
        [Required]
        public string Name { get; set; }

        [Display(Name = "Description")]
        [StringLength(250, ErrorMessage = "The description must be less than {1} characters long.")]
        public string Description { get; set; }

    }


    //to fill the UI manager
    public class ResourceStructureManagerModel
    {
        public long Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string Parent { get; set; }
        public bool InUse { get; set; }

        //define if user has edit access to the attribute
        public bool EditAccess { get; set; }

        //define if user has delete access to the attribute
        public bool DeleteAccess { get; set; }
        public List<string> ResourceStructureAttributesNames { get; set; }

        public ResourceStructureManagerModel(RSE.ResourceStructure resourceStructure)
        {
            Id = resourceStructure.Id;
            Name = resourceStructure.Name;
            Description = resourceStructure.Description;
            InUse = false;
            EditAccess = false;
            DeleteAccess = false;

            using (ResourceStructureManager rManager = new ResourceStructureManager())
            using (ResourceStructureAttributeManager rsaManager = new ResourceStructureAttributeManager())
            {

                if (resourceStructure.Parent != null)
                {
                    RSE.ResourceStructure parent = rManager.GetResourceStructureById(resourceStructure.Parent.Id);
                    Parent = parent.Name;
                }
                else
                    Parent = "";

                List<ResourceAttributeUsage> usages = rsaManager.GetResourceStructureAttributeUsagesByRSId(resourceStructure.Id);
                List<ResourceAttributeUsageModel> list = new List<ResourceAttributeUsageModel>();
                List<string> listNames = new List<string>();
                if (usages != null)
                {
                    foreach (ResourceAttributeUsage usage in usages)
                    {
                        list.Add(new ResourceAttributeUsageModel(usage));
                    }
                }


                if (list != null)
                    listNames = list.Select(e => e.ResourceStructureAttribute.Name).ToList();

                ResourceStructureAttributesNames = listNames;

            }
        }
    }

    public class ResourceStructureParentChoosingModel
    {
        public long RsId { get; set; }

        [Display(Name = "Id")]
        public long ParentId { get; set; }

        public string Name { get; set; }
        public string Description { get; set; }
        //is locked if a circulation in the partnership will happen
        public bool Locked { get; set; }
        public List<string> ResourceStructureAttributesNames { get; set; }

        public ResourceStructureParentChoosingModel()
        {

        }

        public ResourceStructureParentChoosingModel(RSE.ResourceStructure resourceStructure)
        {
            RsId = resourceStructure.Id;
            Name = resourceStructure.Name;
            Description = resourceStructure.Description;
            Locked = false;

            using (ResourceStructureAttributeManager rsaManager = new ResourceStructureAttributeManager())
            {
                List<ResourceAttributeUsage> usages = rsaManager.GetResourceStructureAttributeUsagesByRSId(resourceStructure.Id);
                List<ResourceStructureAttributeModel> list = new List<ResourceStructureAttributeModel>();
                List<string> listNames = new List<string>();
                if (usages != null)
                {
                    foreach (ResourceAttributeUsage usage in usages)
                    {
                        list.Add(new ResourceStructureAttributeModel(usage.ResourceStructureAttribute));
                    }
                }


                if (list != null)
                    listNames = list.Select(e => e.AttributeName).ToList();

                ResourceStructureAttributesNames = listNames;
            }
        }
    }
}