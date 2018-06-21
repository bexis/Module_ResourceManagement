using R = BExIS.Rbm.Entities.Resource;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using BExIS.Rbm.Entities.Resource;
using System.ComponentModel.DataAnnotations;

namespace BExIS.Web.Shell.Areas.RBM.Models.Resource
{
    public class ResourceGroupModel
    {
        public long Id { get; set; }
        public string Name { get; set; }

        [Display(Name = "Usage Mode")]
        public R.Mode ClassifierMode { get; set; }
        public List<ResourceModel> Resources { get; set; }
        public List<ResourceModel> AllResources { get; set; }


        public ResourceGroupModel()
        {
            Name = "";
            ClassifierMode = new R.Mode();
            Resources = new List<ResourceModel>();
        }

        public ResourceGroupModel(ResourceGroup resourceClassifier)
        {
            Id = resourceClassifier.Id;
            Name = resourceClassifier.Name;
            ClassifierMode = resourceClassifier.GroupMode;
            Resources = new List<ResourceModel>();

            if (resourceClassifier.SingleResources != null)
            {
                foreach (R.SingleResource r in resourceClassifier.SingleResources)
                {
                    Resources.Add(new ResourceModel(r));
                }
            }
        }
    }

    public class ResourceGroupManagerModel
    {
        public long Id { get; set; }
        public string Name { get; set; }
        public R.Mode ClassifierMode { get; set; }
        public List<string> ResoureNames { get; set; }

        public ResourceGroupManagerModel()
        {
            Name = "";
            ClassifierMode = new Mode();
            ResoureNames = new List<string>();
        }

        public ResourceGroupManagerModel(ResourceGroup resourceSet)
        {
            Id = resourceSet.Id;
            Name = resourceSet.Name;
            ClassifierMode = resourceSet.GroupMode;
            ResoureNames = new List<string>();

            if (resourceSet.SingleResources.Count() > 0)
            {
                foreach (R.Resource resource in resourceSet.SingleResources)
                {
                    ResoureNames.Add(resource.Name);
                }
            }
        }
    }

    public class CreateResourceGroupModel
    {
        public long Id { get; set; }
        public string Name { get; set; }
        public R.Mode ClassifierMode { get; set; }
        public List<ResourceModel> Resources { get; set; }
    }

}