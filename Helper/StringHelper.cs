using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace BExIS.Web.Shell.Areas.RBM.Helpers
{
    public class StringHelper
    {

        public static string CutSpaces(string str)
        {
            if (str != "" && str != null)
            {
                str = str.Trim();
                if (str.Length > 255)
                    str = str.Substring(0, 255);
            }
            return (str);
        }

    }
}