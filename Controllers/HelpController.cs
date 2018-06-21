using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Vaiona.Web.Extensions;
using Vaiona.Web.Mvc.Models;

namespace BExIS.Web.Shell.Areas.RBM.Controllers
{
    public class HelpController : Controller
    {
        // GET: RBM/Help
        public ActionResult Index()
        {
            ViewBag.Title = PresentationModel.GetViewTitleForTenant("Resource Booking Management Manual", this.Session.GetTenant());

            return View();
        }
    }
}