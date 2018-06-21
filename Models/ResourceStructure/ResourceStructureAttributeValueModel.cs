using BExIS.Dlm.Entities.DataStructure;
using BExIS.Rbm.Entities.ResourceStructure;
using BExIS.Rbm.Services.ResourceStructure;
using BExIS.Web.Shell.Areas.RBM.Models.Resource;
using BExIS.Web.Shell.Areas.RBM.Models.ResourceStructure;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace BExIS.Web.Shell.Areas.RBM.Models.ResourceStructure
{
    public class ResourceStructureAttributeValueModel
    {
        public long Id { get; set; }

        public long ResourceId { get; set; }
        public string AttributeName { get; set; }
        public long ResourceAttributeUsageId { get; set; }
        public ResourceAttributeUsageModel ResourceAttributeUsage { get; set; }

        //use a edit control (Textbox, DropDown ..) or not if the value only shown
        public bool EditMode { get; set; }

        public ResourceStructureAttributeValueModel()
        {
            ResourceId = 0;
            ResourceAttributeUsageId = 0;
           
        }

        public ResourceStructureAttributeValueModel(ResourceAttributeValue value)
        {
            Id = value.Id;
            ResourceId = value.Resource.Id;
            ResourceAttributeUsageId = value.ResourceAttributeUsage.Id;
            AttributeName = value.ResourceAttributeUsage.ResourceStructureAttribute.Name;
            ResourceAttributeUsage = new ResourceAttributeUsageModel(value.ResourceAttributeUsage);

        }
    }
    

    public class TextValueModel : ResourceStructureAttributeValueModel
    {
        public string Value { get; set; }
        public DomainConstraintModel DomainConstraint { get; set; }

        public TextValueModel()
        {
            DomainConstraint = new DomainConstraintModel();
        }

        public TextValueModel(TextValue value)
        {
            Id = value.Id;
            Value = value.Value;
            ResourceId = value.Resource.Id;
            ResourceAttributeUsageId = value.ResourceAttributeUsage.Id;
            ResourceAttributeUsage = new ResourceAttributeUsageModel(value.ResourceAttributeUsage);
            AttributeName = value.ResourceAttributeUsage.ResourceStructureAttribute.Name;

            foreach (Constraint constraint in value.ResourceAttributeUsage.ResourceStructureAttribute.Constraints)
            {
                if (constraint is DomainConstraint)
                {
                    DomainConstraint dc = (DomainConstraint)constraint;
                    dc.Materialize();
                    DomainConstraint = new DomainConstraintModel(dc);
                }
            }
        }
    }


    public class FileValueModel : ResourceStructureAttributeValueModel
    {
        public string Name { get; set; }
        public string Extention { get; set; }
        public string Minmetype { get; set; }
        public byte[] Data { get; set; }

        public bool NeedConfirmation { get; set; }

        public FileValueModel(FileValue value)
        {
            Id = value.Id;
            ResourceId = value.Resource.Id;
            ResourceAttributeUsageId = value.ResourceAttributeUsage.Id;
            Name = value.Name;
            Extention = value.Extention;
            Minmetype = value.Minmetype;
            Data = value.Data;
            AttributeName = value.ResourceAttributeUsage.ResourceStructureAttribute.Name;
            NeedConfirmation = value.NeedConfirmation;
        }

        public FileValueModel()
        {

        }
    }

}
