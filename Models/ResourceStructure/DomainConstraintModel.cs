using BExIS.Dlm.Entities.DataStructure;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace BExIS.Web.Shell.Areas.RBM.Models.ResourceStructure
{
    public class DomainConstraintModel
    {
        public List<DomainItemModel> Items { set; get; }
        public string AttributeName { set; get; }

        public DomainConstraintModel()
        {
            Items = new List<DomainItemModel>();
        }

        public DomainConstraintModel(DomainConstraint dc)
        {
            dc.Materialize();
            AttributeName = dc.DataContainer.Name;
            Items = new List<DomainItemModel>();
            if (dc.Items != null)
            {
                foreach (DomainItem i in dc.Items)
                {
                    Items.Add(new DomainItemModel(i));
                }
            }
        }

    }


    public class DomainItemModel
    {
        public string Key { get; set; }   
        public string Value { get; set; }
        public bool Selected { get; set; }

        public DomainItemModel()
        {
            Key = "";
            Value = "";
        }

        public DomainItemModel(DomainItem item)
        {
            Key = item.Key;
            Value = item.Value;
        }
    }
}