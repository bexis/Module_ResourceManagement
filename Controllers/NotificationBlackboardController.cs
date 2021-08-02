using BExIS.Rbm.Entities.Booking;
using BExIS.Rbm.Services.Booking;
using BExIS.Web.Shell.Areas.RBM.Models.Booking;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;


namespace BExIS.Modules.RBM.UI.Controllers
{
    public class NotificationBlackboardController : Controller
    {

        #region Notification Blackboard

        public ActionResult Index()
        {
            using (NotificationManager nManager = new NotificationManager())
            {
                DateTime today = DateTime.Now;
                List<Notification> nList = nManager.GetNotificationsFromTime(today).ToList();
                List<NotificationBlackboardModel> model = new List<NotificationBlackboardModel>();

                foreach (Notification n in nList)
                {
                    model.Add(new NotificationBlackboardModel(n));
                }

                return View("NotificationBlackboard", model);
            }
            
        }



        #endregion

    }
}
