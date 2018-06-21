using RS= BExIS.Rbm.Entities.ResourceStructure;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using BExIS.Rbm.Services.ResourceStructure;
using BExIS.Rbm.Entities.ResourceStructure;
using BExIS.Dlm.Entities.DataStructure;

namespace BExIS.Web.Shell.Areas.RBM.Models.ResourceStructure
{
    public class ResourceAttributeUsageModel
    {
        public long Id { get; set; }
        public bool IsValueOptional { get; set; }

        public bool IsFileDataType { get; set; }

        public RS.ResourceStructureAttribute ResourceStructureAttribute { get; set; }
        public RS.ResourceStructure ResourceStructure { get; set; }

        public ResourceAttributeUsageModel(RS.ResourceAttributeUsage resourceAttributeUsage)
        {
            Id = resourceAttributeUsage.Id;
            IsValueOptional = resourceAttributeUsage.IsValueOptional;
            ResourceStructureAttribute = resourceAttributeUsage.ResourceStructureAttribute;
            ResourceStructure = resourceAttributeUsage.ResourceStructure;
            IsFileDataType = resourceAttributeUsage.IsFileDataType;
        }
    }


    public class ResourceStructureAttributeUsageModel
    {
        public long UsageId { get; set; }
        public long ResourceAttributeId { get; set; }
        public string ResourceAttributeName { get; set; }
        public string ResourceAttributeDescription { get; set; }
        public bool IsValueOptional { get; set; }
        public bool IsFileDataType { get; set; }
        public DomainConstraintModel DomainConstraint { get; set; }
        public string ParentAttributeName { get; set; }


        public ResourceStructureAttributeUsageModel(long usageId, long resourceAttributeId, string parentName)
        {
            UsageId = usageId;
            ResourceAttributeId = resourceAttributeId;

            //set Parent if exsits
            if (parentName != null)
                ParentAttributeName = parentName;
            else
                ParentAttributeName = "";

            ResourceStructureAttributeManager rsaManager = new ResourceStructureAttributeManager();
            ResourceAttributeUsage usage = rsaManager.GetResourceAttributeUsageById(usageId);

            IsValueOptional = usage.IsValueOptional;
            IsFileDataType = usage.IsFileDataType;

            RS.ResourceStructureAttribute attr = rsaManager.GetResourceStructureAttributesById(resourceAttributeId);

            foreach (Constraint constraint in attr.Constraints)
            {
                if (constraint is DomainConstraint)
                {
                    DomainConstraint dc = (DomainConstraint)constraint;
                    dc.Materialize();
                    DomainConstraint = new DomainConstraintModel(dc);
                }
            }

            ResourceAttributeName = attr.Name;
            ResourceAttributeDescription = attr.Description;
        }

        public ResourceStructureAttributeUsageModel(long usageId)
        {
            UsageId = usageId;
            ResourceStructureAttributeManager rsaManager = new ResourceStructureAttributeManager();
            ResourceAttributeUsage usage = rsaManager.GetResourceAttributeUsageById(usageId);
            IsValueOptional = usage.IsValueOptional;
            ResourceAttributeId = usage.ResourceStructureAttribute.Id;
            ResourceAttributeName = usage.ResourceStructureAttribute.Name;
            ResourceAttributeDescription = usage.ResourceStructureAttribute.Description;
        }

    }

}