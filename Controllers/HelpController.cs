using BExIS.Xml.Helpers;
using System.Web.Mvc;
using System.IO;
using System.Xml.Linq;
using Vaiona.Utils.Cfg;

namespace BExIS.Modules.RBM.UI.Controllers
{
    public class HelpController : Controller
    {
        // GET: RBM/Help
        public ActionResult Index()
        {
            string filePath = Path.Combine(AppConfiguration.GetModuleWorkspacePath("RBM"), "Rbm.Settings.xml");
            XDocument settings = XDocument.Load(filePath);
            XElement help = XmlUtility.GetXElementByAttribute("entry", "key", "help", settings);

            string helpurl = help.Attribute("value")?.Value;


            return Redirect(helpurl);

        }
    }
}