using BExIS.Dlm.Entities.Party;
using BExIS.Dlm.Services.Party;
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
            using (UserManager userManager = new UserManager())
            {
                return userManager.FindByNameAsync(userName).Result.Id;
            }
        }

        public static Party GetPartyByUserId(long userId)
        {
            using (var partyManager = new PartyManager())
            {
                return partyManager.GetPartyByUser(userId);
            }
        }

    }
}