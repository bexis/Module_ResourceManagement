using BExIS.Rbm.Entities.Booking;
using BExIS.Rbm.Services.Booking;
using BExIS.Web.Shell.Areas.RBM.Models.Booking;
using BExIS.Web.Shell.Areas.RBM.Models.Resource;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace BExIS.Modules.RBM.UI.Helper
{
    public static class ResourceFilterHelper
    {
        //internal useage for resóurce filtering

        public struct FilterTreeItem
        {
            public long Id;
            public string Value;
        }

        //Create dictionary for every attribute filter, e.g. attr id 1 with 3 domain items
        public static Dictionary<long, List<string>> GetFilterDic(List<FilterTreeItem> filters)
        {
            Dictionary<long, List<string>> filterDic = new Dictionary<long, List<string>>();

            foreach (FilterTreeItem item in filters)
            {
                if (filterDic.Keys.Contains(item.Id))
                    filterDic[item.Id].Add(item.Value);
                else
                {
                    List<string> values = new List<string>();
                    values.Add(item.Value);
                    filterDic.Add(item.Id, values);
                }
            }

            return filterDic;
        }

        public static bool CheckTreeDomainModel(ResourceAttributeValueModel model, Dictionary<long, List<string>> filters)
        {
            foreach (KeyValuePair<long, List<string>> kp in filters)
            {
                if (IsResult(model, kp.Key, kp.Value) == false) return false;
            }

            return true;
        }

        //checks if is a filter result
        private static bool IsResult(ResourceAttributeValueModel model, long id, List<string> values)
        {
            bool temp = false;

            foreach (string value in values)
            {
                //int index = model.AttributeIds.IndexOf(id);

                //if (model.Values.ElementAt(index).Equals(value))
                if (model.Values.Contains(value))
                {
                    temp = true;
                }
            }

            return temp;
        }

    }
}