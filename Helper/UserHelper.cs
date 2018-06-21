using BExIS.Security.Services.Subjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace BExIS.Modules.RBM.UI.Helper
{
    public static class UserHelper
    {
        public static long GetUserId(string userName)
        {
            UserManager userManager = new UserManager();
            return userManager.FindByNameAsync(userName).Id;
        }

    }
}