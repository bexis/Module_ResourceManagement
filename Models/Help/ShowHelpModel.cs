using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace BExIS.Web.Shell.Areas.RBM.Models.Help
{
    public class ShowHelpModel
    {
        public string Title { get; set; }
        public string Description { get; set; }

        public ShowHelpModel()
        {
            Title = "";
            Description = "";
        }
    }
}