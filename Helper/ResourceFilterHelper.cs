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

        //internal usage to store a filter query
        public struct Filter
        {
            public DateTime StartDate;
            public DateTime EndDate;
            public int Quantity;
            //true if filter is active
            public bool IsSet;
            public List<FilterTreeItem> TreeFilterItems;
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

        public static List<SelectResourceForEventModel> ApplyFilter(List<SelectResourceForEventModel> list, ResourceFilterHelper.Filter filter)
        {
            List<SelectResourceForEventModel> endResultList = new List<SelectResourceForEventModel>();
            using (var schManager = new ScheduleManager())
            {
                //Filter the result list withe the other criterias if given
                foreach (SelectResourceForEventModel r in list)
                {
                    int resourceQuantity = 0;
                    if (r.ResourceQuantity == "no limitation")
                        resourceQuantity = 0;
                    else
                        resourceQuantity = int.Parse(r.ResourceQuantity);


                    //get all schedule where resource is booked
                    List<Schedule> allSchedules = schManager.GetAllSchedulesByResource(r.Id);
                    List<Schedule> inTimeSchedules = new List<Schedule>();
                    int schedulesQuantity = 0;
                    foreach (Schedule s in allSchedules)
                    {
                        //get all schedule in the given time period
                        if ((DateTime.Compare(filter.StartDate, s.StartDate) >= 0 && DateTime.Compare(filter.StartDate, s.EndDate) <= 0) || (DateTime.Compare(filter.EndDate, s.StartDate) >= 0 && DateTime.Compare(filter.EndDate, s.EndDate) <= 0))
                        {
                            //Count all quantities in in time schedules
                            schedulesQuantity = schedulesQuantity + s.Quantity;
                        }
                    }
                    if (resourceQuantity != 0)
                    {
                        //if diff of resourceQuantity and schedulesQuantity minus requested quantity is posive add to resultlist
                        if (((resourceQuantity - schedulesQuantity) - filter.Quantity) >= 0)
                        {
                            r.AvailableQuantity = resourceQuantity - schedulesQuantity;
                            endResultList.Add(r);
                        }
                    }
                    else
                        endResultList.Add(r);
                }
            }

            return endResultList;
        }


    }
}