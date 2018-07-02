using BExIS.Modules.RBM.UI.Helper;
using System;
using Vaiona.Logging;
using Vaiona.Web.Mvc.Modularity;

namespace BExIS.Modules.RBM.UI
{
    public class RBMModule : ModuleBase
    {
        public RBMModule() : base("RBM")
        {

        }

        public override void Start()
        {
            base.Start();
            LoggerFactory.GetFileLogger().LogCustom("Start RBM");
        }

        public override void Install()
        {
            LoggerFactory.GetFileLogger().LogCustom("... start install of RBM ...");
            try
            {
                base.Install();
                using (var rbmSeedDataGenerator = new RbmSeedDataGenerator())
                {
                    rbmSeedDataGenerator.GenerateSeedData();
                }
            }
            catch (Exception e)
            {
                LoggerFactory.GetFileLogger().LogCustom(e.Message);
                LoggerFactory.GetFileLogger().LogCustom(e.StackTrace);
            }

            LoggerFactory.GetFileLogger().LogCustom("... end install of RBM ...");
        }
    }
}
