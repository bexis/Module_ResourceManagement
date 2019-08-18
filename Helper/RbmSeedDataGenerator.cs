using BExIS.Rbm.Entities.Booking;
using BExIS.Rbm.Entities.Resource;
using BExIS.Rbm.Entities.ResourceStructure;
using BExIS.Rbm.Services.Resource;
using BExIS.Rbm.Entities.BookingManagementTime;
using BExIS.Security.Entities.Objects;
using BExIS.Security.Services.Objects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using BExIS.Web.Shell.Areas.RBM.Models.ResourceStructure;
using BExIS.Rbm.Services.ResourceStructure;
using BExIS.Dlm.Services.DataStructure;
using BExIS.Dlm.Entities.DataStructure;

namespace BExIS.Modules.RBM.UI.Helper
{
    public class RbmSeedDataGenerator : IDisposable
    {
        public void Dispose()
        {
            //throw new NotImplementedException();
        }

        public class newResourceStructure
        {
            public string name { set; get; }
            public string description { set; get; }
            public int quantity { set; get; }
            public string color { set; get; }
            public bool withActivity { set; get; }
            public ResourceStructure resourceStructure { set; get; }
            public int duration { set; get; }
            public string type { set; get; }
            public string explo { set; get; }
        }

        public struct EntityStruct
        {
            public string Name;
            public Type Type;
            public Type StoreType;

            public EntityStruct(string name, Type type, Type storeType)
            {
                Name = name;
                Type = type;
                StoreType = storeType;
            }
        }

        public void GenerateSeedData()
        {

            #region ENTITIES

            List<EntityStruct> entities = new List<EntityStruct>();
            entities.Add(new EntityStruct("SingleResource", typeof(SingleResource), typeof(BExIS.Rbm.Services.Resource.SingleResourceStore)));
            entities.Add(new EntityStruct("ResourceStructure", typeof(ResourceStructure), typeof(BExIS.Rbm.Services.ResourceStructure.ResourceStructureStore)));
            entities.Add(new EntityStruct("ResourceStructureAttribute", typeof(ResourceStructureAttribute), typeof(BExIS.Rbm.Services.ResourceStructure.ResourceStructureAttributeStore)));
            entities.Add(new EntityStruct("Activity", typeof(Activity), typeof(BExIS.Rbm.Services.Booking.ActivityStore)));
            entities.Add(new EntityStruct("BookingEvent", typeof(BookingEvent), typeof(BExIS.Rbm.Services.Booking.BookingEventStore)));
            entities.Add(new EntityStruct("Notification", typeof(Notification), typeof(BExIS.Rbm.Services.Booking.NotificationStore)));
            entities.Add(new EntityStruct("Schedule", typeof(Schedule), typeof(BExIS.Rbm.Services.Booking.ScheduleStore)));



            Dictionary<string, Type> rbmEntities = new Dictionary<string,Type>();
            rbmEntities.Add("SingleResource", typeof(SingleResource));
            rbmEntities.Add("ResourceStructure", typeof(ResourceStructure));
            rbmEntities.Add("ResourceStructureAttribute", typeof(ResourceStructureAttribute));
            rbmEntities.Add("Activity", typeof(Activity));
            rbmEntities.Add("BookingEvent", typeof(BookingEvent));
            rbmEntities.Add("Notification", typeof(Notification));
            rbmEntities.Add("Schedule", typeof(Schedule));

            using (var entityManager = new EntityManager())
            {
                foreach (var et in entities)
                {
                    Entity entity = entityManager.Entities.Where(e => e.Name.ToUpperInvariant() == et.Name.ToUpperInvariant()).FirstOrDefault();

                    if (entity == null)
                    {
                        entity = new Entity();
                        entity.Name = et.Name;
                        entity.EntityType = et.Type;
                        entity.EntityStoreType = et.StoreType;
                        //entity.UseMetadata = true;
                        entity.Securable = true;
                        entityManager.Create(entity);
                    }
                }
            }

            try
            {

                ResourceStructureAttribute rsa = new ResourceStructureAttribute();
                using (var rsaManager = new ResourceStructureAttributeManager())
                   rsa = rsaManager.CreateResourceStructureAttribute("Exploratory", "Biodiversity Exploratories funded by DFG Priority Programme 1374. They serve as open research platform for all biodiversity and ecosystem research groups of Germany.");

                var dcManager = new DataContainerManager();

                string[] keys = { "Hainich-Dün", "Schorfheide-Chorin", "Schwäbische Alb" };
                List<DomainItem> domainItems = CreateDomainItems(keys);
                DomainConstraint dc = new DomainConstraint(ConstraintProviderSource.Internal, "", "en-US", "a simple domain validation constraint", false, null, null, null, domainItems);
                dcManager.AddConstraint(dc, rsa);


                ResourceStructureAttribute rsa2 = new ResourceStructureAttribute();
                using (var rsaManager2 = new ResourceStructureAttributeManager())
                    rsa2 = rsaManager2.CreateResourceStructureAttribute("Type", "Type of resource.");

                var dcManager2 = new DataContainerManager();

                string[]  keys2 = { "Area", "Object", "Sleeping place"};
                List<DomainItem>  domainItems2 = CreateDomainItems(keys2);
                DomainConstraint dc2 = new DomainConstraint(ConstraintProviderSource.Internal, "", "en-US", "a simple domain validation constraint", false, null, null, null, domainItems2);
                dcManager2.AddConstraint(dc2, rsa2);

                ResourceStructureManager rsManager = new ResourceStructureManager(); ;
                ResourceStructure rs = rsManager.Create("Explo resources", "Resources related to exploratories.", null, null);

                using (var rsaManager = new ResourceStructureAttributeManager())
                rsaManager.CreateResourceAttributeUsage(rsa, rs, true, false);
                using (var rsaManager = new ResourceStructureAttributeManager())
                rsaManager.CreateResourceAttributeUsage(rsa2, rs, true, false);

                ResourceManager rManager = new ResourceManager(); ;
                List<newResourceStructure> rs_new = new List<newResourceStructure>();

                rs_new.Add(new newResourceStructure() { name = "Forest - all EPs (SCH)", color = "#ec5959", description = "Visit of all forest EPs in Schorfheide-Chorin", duration = 1, quantity = 0, withActivity = true, resourceStructure = rs, type = "Area", explo = "Schorfheide-Chorin" });
                rs_new.Add(new newResourceStructure() { name = "Forest - all MIPs (SCH)", color = "#ec5959", description = "Visit of all forest MIPs in Schorfheide-Chorin", duration = 1, quantity = 0, withActivity = true, resourceStructure = rs, type = "Area", explo = "Schorfheide-Chorin" });
                rs_new.Add(new newResourceStructure() { name = "Forest - all VIPs (SCH)", color = "#ec5959", description = "Visit of all forest VIPs in Schorfheide-Chorin", duration = 1, quantity = 0, withActivity = true, resourceStructure = rs, type = "Area", explo = "Schorfheide-Chorin" });
                rs_new.Add(new newResourceStructure() { name = "Forest - district Altenhof", color = "#ec5959", description = "", duration = 1, quantity = 0, withActivity = true, resourceStructure = rs, type = "Area", explo = "Schorfheide-Chorin" });
                rs_new.Add(new newResourceStructure() { name = "Forest - district Arnimswalde", color = "#ec5959", description = "", duration = 1, quantity = 0, withActivity = true, resourceStructure = rs, type = "Area", explo = "Schorfheide-Chorin" });
                rs_new.Add(new newResourceStructure() { name = "Forest - district Chorin", color = "#ec5959", description = "", duration = 1, quantity = 0, withActivity = true, resourceStructure = rs, type = "Area", explo = "Schorfheide-Chorin" });
                rs_new.Add(new newResourceStructure() { name = "Forest - district Eichheide", color = "#ec5959", description = "", duration = 1, quantity = 0, withActivity = true, resourceStructure = rs, type = "Area", explo = "Schorfheide-Chorin" });
                rs_new.Add(new newResourceStructure() { name = "Forest - district Glambeck", color = "#ec5959", description = "", duration = 1, quantity = 0, withActivity = true, resourceStructure = rs, type = "Area", explo = "Schorfheide-Chorin" });
                rs_new.Add(new newResourceStructure() { name = "Forest - district Liepe", color = "#ec5959", description = "", duration = 1, quantity = 0, withActivity = true, resourceStructure = rs, type = "Area", explo = "Schorfheide-Chorin" });
                rs_new.Add(new newResourceStructure() { name = "Forest - district Maienpfuhl", color = "#ec5959", description = "", duration = 1, quantity = 0, withActivity = true, resourceStructure = rs, type = "Area", explo = "Schorfheide-Chorin" });
                rs_new.Add(new newResourceStructure() { name = "Forest - district Melzow", color = "#ec5959", description = "", duration = 1, quantity = 0, withActivity = true, resourceStructure = rs, type = "Area", explo = "Schorfheide-Chorin" });
                rs_new.Add(new newResourceStructure() { name = "Forest - district Poratz", color = "#ec5959", description = "", duration = 1, quantity = 0, withActivity = true, resourceStructure = rs, type = "Area", explo = "Schorfheide-Chorin" });
                rs_new.Add(new newResourceStructure() { name = "Forest - district Ringenwalde", color = "#ec5959", description = "", duration = 1, quantity = 0, withActivity = true, resourceStructure = rs, type = "Area", explo = "Schorfheide-Chorin" });
                rs_new.Add(new newResourceStructure() { name = "Forest - district Senftenthal", color = "#ec5959", description = "", duration = 1, quantity = 0, withActivity = true, resourceStructure = rs, type = "Area", explo = "Schorfheide-Chorin" });
                rs_new.Add(new newResourceStructure() { name = "Forest - district Theerofen", color = "#ec5959", description = "", duration = 1, quantity = 0, withActivity = true, resourceStructure = rs, type = "Area", explo = "Schorfheide-Chorin" });
                rs_new.Add(new newResourceStructure() { name = "Forest - district Voigtswiese", color = "#ec5959", description = "", duration = 1, quantity = 0, withActivity = true, resourceStructure = rs, type = "Area", explo = "Schorfheide-Chorin" });
                rs_new.Add(new newResourceStructure() { name = "Forest - Forstgut Altkuenkendorf", color = "#ec5959", description = "", duration = 1, quantity = 0, withActivity = true, resourceStructure = rs, type = "Area", explo = "Schorfheide-Chorin" });
                rs_new.Add(new newResourceStructure() { name = "Forest - Fuerstliche Forstverwaltung Oettingen-Spielberg", color = "#ec5959", description = "", duration = 1, quantity = 0, withActivity = true, resourceStructure = rs, type = "Area", explo = "Schorfheide-Chorin" });
                rs_new.Add(new newResourceStructure() { name = "Forest - Stiftung Schorfheide-Chorin", color = "#ec5959", description = "", duration = 1, quantity = 0, withActivity = true, resourceStructure = rs, type = "Area", explo = "Schorfheide-Chorin" });
                rs_new.Add(new newResourceStructure() { name = "Grassland - ABP Landwirtschaft Neugrimnitz GmbH", color = "#ff0000", description = "", duration = 1, quantity = 0, withActivity = true, resourceStructure = rs, type = "Area", explo = "Schorfheide-Chorin" });
                rs_new.Add(new newResourceStructure() { name = "Grassland - AEVG Stegelitz-Flieth", color = "#ff0000", description = "", duration = 1, quantity = 0, withActivity = true, resourceStructure = rs, type = "Area", explo = "Schorfheide-Chorin" });
                rs_new.Add(new newResourceStructure() { name = "Grassland - Agrargemeinschaft Greiffenberg GmbH", color = "#ff0000", description = "", duration = 1, quantity = 0, withActivity = true, resourceStructure = rs, type = "Area", explo = "Schorfheide-Chorin" });
                rs_new.Add(new newResourceStructure() { name = "Grassland - all EPs (SCH)", color = "#ff0000", description = "Visit of all grassland EPs in Schorfheide-Chorin", duration = 1, quantity = 0, withActivity = true, resourceStructure = rs, type = "Area", explo = "Schorfheide-Chorin" });
                rs_new.Add(new newResourceStructure() { name = "Grassland - all MIPs (SCH)", color = "#ff0000", description = "Visit of all grassland MIPs in Schorfheide-Chorin", duration = 1, quantity = 0, withActivity = true, resourceStructure = rs, type = "Area", explo = "Schorfheide-Chorin" });
                rs_new.Add(new newResourceStructure() { name = "Grassland - all VIPs (SCH)", color = "#ff0000", description = "Visit of all grassland VIPs in Schorfheide-Chorin", duration = 1, quantity = 0, withActivity = true, resourceStructure = rs, type = "Area", explo = "Schorfheide-Chorin" });
                rs_new.Add(new newResourceStructure() { name = "Grassland - Gut Biesenbrow", color = "#ff0000", description = "", duration = 1, quantity = 0, withActivity = true, resourceStructure = rs, type = "Area", explo = "Schorfheide-Chorin" });
                rs_new.Add(new newResourceStructure() { name = "Grassland - Ingolf Limber", color = "#ff0000", description = "", duration = 1, quantity = 0, withActivity = true, resourceStructure = rs, type = "Area", explo = "Schorfheide-Chorin" });
                rs_new.Add(new newResourceStructure() { name = "Grassland - Lehrschaeferei Friedrichsfelde", color = "#ff0000", description = "", duration = 1, quantity = 0, withActivity = true, resourceStructure = rs, type = "Area", explo = "Schorfheide-Chorin" });
                rs_new.Add(new newResourceStructure() { name = "Grassland - LW-Betrieb Haferkamp", color = "#ff0000", description = "", duration = 1, quantity = 0, withActivity = true, resourceStructure = rs, type = "Area", explo = "Schorfheide-Chorin" });
                rs_new.Add(new newResourceStructure() { name = "Grassland - Naturrind GmbH Boeckenberg", color = "#ff0000", description = "", duration = 1, quantity = 0, withActivity = true, resourceStructure = rs, type = "Area", explo = "Schorfheide-Chorin" });
                rs_new.Add(new newResourceStructure() { name = "Grassland - Reiner Wegner", color = "#ff0000", description = "", duration = 1, quantity = 0, withActivity = true, resourceStructure = rs, type = "Area", explo = "Schorfheide-Chorin" });
                rs_new.Add(new newResourceStructure() { name = "Grassland - Weidewirtschaft Liepe", color = "#ff0000", description = "", duration = 1, quantity = 0, withActivity = true, resourceStructure = rs, type = "Area", explo = "Schorfheide-Chorin" });
                rs_new.Add(new newResourceStructure() { name = "no plot visit (SCH)", color = "#e28f8f", description = "", duration = 1, quantity = 0, withActivity = true, resourceStructure = rs, type = "Area", explo = "Schorfheide-Chorin" });

                rs_new.Add(new newResourceStructure() { name = "Sleeping place (SCH)", color = "Sleeping places in Schorfheide-Chorin", description = "#9c3939", duration = 1, quantity = 7, withActivity = false, resourceStructure = rs, type = "Sleeping place", explo = "Schorfheide-Chorin" });

                rs_new.Add(new newResourceStructure() { name = "Drying cabinet (SCH)", color = "Drying cabinet in Schorfheide-Chorin", description = "#940b0b", duration = 1, quantity = 5, withActivity = false, resourceStructure = rs, type = "Object", explo = "Schorfheide-Chorin" });
                rs_new.Add(new newResourceStructure() { name = "Fridge (SCH)", color = "Fridge in Schorfheide-Chorin", description = "#940b0b", duration = 1, quantity = 2, withActivity = false, resourceStructure = rs, type = "Object", explo = "Schorfheide-Chorin" });
                rs_new.Add(new newResourceStructure() { name = "Freezer (SCH)", color = "Freezer in Schorfheide-Chorin", description = "#940b0b", duration = 1, quantity = 2, withActivity = false, resourceStructure = rs, type = "Object", explo = "Schorfheide-Chorin" });
                rs_new.Add(new newResourceStructure() { name = "Metal detector (Magna Trak 100) (SCH)", color = "Metal detector (Magna Trak 100) in Schorfheide-Chorin", description = "#940b0b", duration = 1, quantity = 1, withActivity = false, resourceStructure = rs, type = "Object", explo = "Schorfheide-Chorin" });
                rs_new.Add(new newResourceStructure() { name = "Binocular (SCH)", color = "", description = "#940b0b", duration = 1, quantity = 1, withActivity = false, resourceStructure = rs, type = "Object", explo = "Schorfheide-Chorin" });
                rs_new.Add(new newResourceStructure() { name = "Chest freezer (SCH)", color = "", description = "#940b0b", duration = 1, quantity = 1, withActivity = false, resourceStructure = rs, type = "Object", explo = "Schorfheide-Chorin" });


                rs_new.Add(new newResourceStructure() { name = "", color = "#8cca0d", description = "Forest - all EPs (HAI)", duration = 1, quantity = 0, withActivity = true, resourceStructure = rs, type = "Area", explo = "Hainich-Dün" });
                rs_new.Add(new newResourceStructure() { name = "", color = "#8cca0d", description = "Forest - all GPs (HAI)", duration = 1, quantity = 0, withActivity = true, resourceStructure = rs, type = "Area", explo = "Hainich-Dün" });
                rs_new.Add(new newResourceStructure() { name = "", color = "#8cca0d", description = "Forest - all MIPs (HAI)", duration = 1, quantity = 0, withActivity = true, resourceStructure = rs, type = "Area", explo = "Hainich-Dün" });
                rs_new.Add(new newResourceStructure() { name = "", color = "#8cca0d", description = "Forest - all VIPs (HAI)", duration = 1, quantity = 0, withActivity = true, resourceStructure = rs, type = "Area", explo = "Hainich-Dün" });
                rs_new.Add(new newResourceStructure() { name = "", color = "#8cca0d", description = "Forest - district Anrode", duration = 1, quantity = 0, withActivity = true, resourceStructure = rs, type = "Area", explo = "Hainich-Dün" });
                rs_new.Add(new newResourceStructure() { name = "", color = "#8cca0d", description = "Forest - city of Mühlhausen", duration = 1, quantity = 0, withActivity = true, resourceStructure = rs, type = "Area", explo = "Hainich-Dün" });
                rs_new.Add(new newResourceStructure() { name = "", color = "#8cca0d", description = "Forest - district Behringen", duration = 1, quantity = 0, withActivity = true, resourceStructure = rs, type = "Area", explo = "Hainich-Dün" });
                rs_new.Add(new newResourceStructure() { name = "", color = "#8cca0d", description = "Forest - district Geney", duration = 1, quantity = 0, withActivity = true, resourceStructure = rs, type = "Area", explo = "Hainich-Dün" });
                rs_new.Add(new newResourceStructure() { name = "", color = "#8cca0d", description = "Forest - district Keula", duration = 1, quantity = 0, withActivity = true, resourceStructure = rs, type = "Area", explo = "Hainich-Dün" });
                rs_new.Add(new newResourceStructure() { name = "", color = "#8cca0d", description = "Forest - district Langula", duration = 1, quantity = 0, withActivity = true, resourceStructure = rs, type = "Area", explo = "Hainich-Dün" });
                rs_new.Add(new newResourceStructure() { name = "", color = "#8cca0d", description = "Forest - district Sollstedt", duration = 1, quantity = 0, withActivity = true, resourceStructure = rs, type = "Area", explo = "Hainich-Dün" });
                rs_new.Add(new newResourceStructure() { name = "", color = "#8cca0d", description = "Forest - district Zehnsberg", duration = 1, quantity = 0, withActivity = true, resourceStructure = rs, type = "Area", explo = "Hainich-Dün" });
                rs_new.Add(new newResourceStructure() { name = "", color = "#8cca0d", description = "Forest - national park", duration = 1, quantity = 0, withActivity = true, resourceStructure = rs, type = "Area", explo = "Hainich-Dün" });
                rs_new.Add(new newResourceStructure() { name = "", color = "#8cca0d", description = "Forest - national park Weberstedterholz (core zone)", duration = 1, quantity = 0, withActivity = true, resourceStructure = rs, type = "Area", explo = "Hainich-Dün" });
                rs_new.Add(new newResourceStructure() { name = "", color = "#8cca0d", description = "Forest - district Westerwald", duration = 1, quantity = 0, withActivity = true, resourceStructure = rs, type = "Area", explo = "Hainich-Dün" });
                rs_new.Add(new newResourceStructure() { name = "", color = "#9acd32", description = "Grassland - AG Diedorf", duration = 1, quantity = 0, withActivity = true, resourceStructure = rs, type = "Area", explo = "Hainich-Dün" });
                rs_new.Add(new newResourceStructure() { name = "", color = "#9acd32", description = "Grassland - AG Großenlupnitz", duration = 1, quantity = 0, withActivity = true, resourceStructure = rs, type = "Area", explo = "Hainich-Dün" });
                rs_new.Add(new newResourceStructure() { name = "", color = "#9acd32", description = "Grassland - AG Kirchheiligen", duration = 1, quantity = 0, withActivity = true, resourceStructure = rs, type = "Area", explo = "Hainich-Dün" });
                rs_new.Add(new newResourceStructure() { name = "", color = "#9acd32", description = "Grassland - AG Bollstedt", duration = 1, quantity = 0, withActivity = true, resourceStructure = rs, type = "Area", explo = "Hainich-Dün" });
                rs_new.Add(new newResourceStructure() { name = "", color = "#9acd32", description = "Grassland - AG Lengefeld", duration = 1, quantity = 0, withActivity = true, resourceStructure = rs, type = "Area", explo = "Hainich-Dün" });
                rs_new.Add(new newResourceStructure() { name = "", color = "#9acd32", description = "Grassland - AG Mülverstedt", duration = 1, quantity = 0, withActivity = true, resourceStructure = rs, type = "Area", explo = "Hainich-Dün" });
                rs_new.Add(new newResourceStructure() { name = "", color = "#9acd32", description = "Grassland - AG Wartburgblick", duration = 1, quantity = 0, withActivity = true, resourceStructure = rs, type = "Area", explo = "Hainich-Dün" });
                rs_new.Add(new newResourceStructure() { name = "", color = "#9acd32", description = "Grassland - AG Wenigenlupnitz", duration = 1, quantity = 0, withActivity = true, resourceStructure = rs, type = "Area", explo = "Hainich-Dün" });
                rs_new.Add(new newResourceStructure() { name = "", color = "#9acd32", description = "Grassland - AG-Struth-Eigenrieden", duration = 1, quantity = 0, withActivity = true, resourceStructure = rs, type = "Area", explo = "Hainich-Dün" });
                rs_new.Add(new newResourceStructure() { name = "", color = "#9acd32", description = "Grassland - all GPs (HAI)", duration = 1, quantity = 0, withActivity = true, resourceStructure = rs, type = "Area", explo = "Hainich-Dün" });
                rs_new.Add(new newResourceStructure() { name = "", color = "#9acd32", description = "Grassland - all EPs (HAI)", duration = 1, quantity = 0, withActivity = true, resourceStructure = rs, type = "Area", explo = "Hainich-Dün" });
                rs_new.Add(new newResourceStructure() { name = "", color = "#9acd32", description = "Grassland - all MIPs (HAI)", duration = 1, quantity = 0, withActivity = true, resourceStructure = rs, type = "Area", explo = "Hainich-Dün" });
                rs_new.Add(new newResourceStructure() { name = "", color = "#9acd32", description = "Grassland - all VIPs (HAI)", duration = 1, quantity = 0, withActivity = true, resourceStructure = rs, type = "Area", explo = "Hainich-Dün" });
                rs_new.Add(new newResourceStructure() { name = "", color = "#9acd32", description = "Grassland - BEAG/TOA", duration = 1, quantity = 0, withActivity = true, resourceStructure = rs, type = "Area", explo = "Hainich-Dün" });
                rs_new.Add(new newResourceStructure() { name = "", color = "#9acd32", description = "Grassland - Bundesforst Dörnaerplatz", duration = 1, quantity = 0, withActivity = true, resourceStructure = rs, type = "Area", explo = "Hainich-Dün" });
                rs_new.Add(new newResourceStructure() { name = "", color = "#9acd32", description = "Grassland - Gut Sambach", duration = 1, quantity = 0, withActivity = true, resourceStructure = rs, type = "Area", explo = "Hainich-Dün" });
                rs_new.Add(new newResourceStructure() { name = "", color = "#9acd32", description = "Grassland - Thüringen Wagyu Becker&Becker", duration = 1, quantity = 0, withActivity = true, resourceStructure = rs, type = "Area", explo = "Hainich-Dün" });
                rs_new.Add(new newResourceStructure() { name = "", color = "#9acd32", description = "Grassland - Martin Heller", duration = 1, quantity = 0, withActivity = true, resourceStructure = rs, type = "Area", explo = "Hainich-Dün" });
                rs_new.Add(new newResourceStructure() { name = "", color = "#9acd32", description = "Grassland - Schaeferei Effenberger", duration = 1, quantity = 0, withActivity = true, resourceStructure = rs, type = "Area", explo = "Hainich-Dün" });
                rs_new.Add(new newResourceStructure() { name = "", color = "#9acd32", description = "Grassland - Nationalpark", duration = 1, quantity = 0, withActivity = true, resourceStructure = rs, type = "Area", explo = "Hainich-Dün" });
                rs_new.Add(new newResourceStructure() { name = "", color = "#9acd32", description = "Grassland - Nordagrar (Naturrind Unstruttal)", duration = 1, quantity = 0, withActivity = true, resourceStructure = rs, type = "Area", explo = "Hainich-Dün" });
                rs_new.Add(new newResourceStructure() { name = "", color = "#9acd32", description = "Grassland - Schaeferei Goepfert", duration = 1, quantity = 0, withActivity = true, resourceStructure = rs, type = "Area", explo = "Hainich-Dün" });
                rs_new.Add(new newResourceStructure() { name = "", color = "#9acd32", description = "Grassland - Roland Börner", duration = 1, quantity = 0, withActivity = true, resourceStructure = rs, type = "Area", explo = "Hainich-Dün" });
                rs_new.Add(new newResourceStructure() { name = "", color = "#9acd32", description = "Grassland - Carmen Strewe-Winterberg", duration = 1, quantity = 0, withActivity = true, resourceStructure = rs, type = "Area", explo = "Hainich-Dün" });
                rs_new.Add(new newResourceStructure() { name = "", color = "#9acd32", description = "Grassland -TUPAG (Hainich Weiderind)", duration = 1, quantity = 0, withActivity = true, resourceStructure = rs, type = "Area", explo = "Hainich-Dün" });
                rs_new.Add(new newResourceStructure() { name = "", color = "#a9fb00", description = "no plot visit", duration = 1, quantity = 0, withActivity = true, resourceStructure = rs, type = "Area", explo = "Hainich-Dün" });

                rs_new.Add(new newResourceStructure() { name = "Sleeping place (HAI)", color = "Sleeping place in Hainich-Dün", description = "#78ab0f", duration = 1, quantity = 8, withActivity = false, resourceStructure = rs, type = "Sleeping place", explo = "Hainich-Dün" });

                rs_new.Add(new newResourceStructure() { name = "Drying cabinet (HAI)", color = "Drying cabinete in Hainich-Dün", description = "#6a8a27", duration = 1, quantity = 4, withActivity = false, resourceStructure = rs, type = "Object", explo = "Hainich-Dün" });
                rs_new.Add(new newResourceStructure() { name = "Fridge (HAI)", color = "Fridge in Hainich-Dün", description = "#6a8a27", duration = 1, quantity = 2, withActivity = false, resourceStructure = rs, type = "Object", explo = "Hainich-Dün" });
                rs_new.Add(new newResourceStructure() { name = "Freezer (HAI)", color = "Freezer in Hainich-Dün", description = "#6a8a27", duration = 1, quantity = 3, withActivity = false, resourceStructure = rs, type = "Object", explo = "Hainich-Dün" });
                rs_new.Add(new newResourceStructure() { name = "House key - Mülverstedt (HAI)", color = "", description = "#6a8a27", duration = 1, quantity = 1, withActivity = false, resourceStructure = rs, type = "Object", explo = "Hainich-Dün" });
                rs_new.Add(new newResourceStructure() { name = "Metal detector (Magna Trak 100) (HAI)", color = "Metal detector (Magna Trak 100) in Hainich-Dün", description = "#6a8a27", duration = 1, quantity = 1, withActivity = false, resourceStructure = rs, type = "Object", explo = "Hainich-Dün" });
                rs_new.Add(new newResourceStructure() { name = "Metal detector (Garrett ACE  150) (HAI)", color = "Metal detector (Garrett ACE  150) in Hainich-Dün", description = "#6a8a27", duration = 1, quantity = 1, withActivity = false, resourceStructure = rs, type = "Object", explo = "Hainich-Dün" });


                rs_new.Add(new newResourceStructure() { name = "Forest - all EPs (ALB)", color = "#3a75e0", description = "Visit of all forest EPs in Schwäbische Alb", duration = 1, quantity = 0, withActivity = true, resourceStructure = rs, type = "Area", explo = "Schwäbische Alb" });
                rs_new.Add(new newResourceStructure() { name = "Forest - VIP AEW1-8 (ALB)", color = "#3a75e0", description = "Visit of all VIP AEW1-8 in Schwäbische Alb", duration = 1, quantity = 0, withActivity = true, resourceStructure = rs, type = "Area", explo = "Schwäbische Alb" });
                rs_new.Add(new newResourceStructure() { name = "Forest - VIP AEW9 (Alb)", color = "#3a75e0", description = "Visit of all VIP AEW9 in Schwäbische Alb", duration = 1, quantity = 0, withActivity = true, resourceStructure = rs, type = "Area", explo = "Schwäbische Alb" });
                rs_new.Add(new newResourceStructure() { name = "Grassland - all EPs excluding former military training area", color = "#6495ed", description = "", duration = 1, quantity = 0, withActivity = true, resourceStructure = rs, type = "Area", explo = "Schwäbische Alb" });
                rs_new.Add(new newResourceStructure() { name = "Grassland - all EPs including former military training area", color = "#6495ed", description = "", duration = 1, quantity = 0, withActivity = true, resourceStructure = rs, type = "Area", explo = "Schwäbische Alb" });
                rs_new.Add(new newResourceStructure() { name = "Grassland - all VIPs (ALB)", color = "#6495ed", description = "Visit of all grassland VIPs in Schwäbische Alb", duration = 1, quantity = 0, withActivity = true, resourceStructure = rs, type = "Area", explo = "Schwäbische Alb" });
                rs_new.Add(new newResourceStructure() { name = "no plot visit (ALB)", color = "#0057f5", description = "", duration = 1, quantity = 0, withActivity = true, resourceStructure = rs, type = "Area", explo = "Schwäbische Alb" });
                rs_new.Add(new newResourceStructure() { name = "total Exploratory outside EP", color = "#0057f6", description = "", duration = 1, quantity = 0, withActivity = true, resourceStructure = rs, type = "Area", explo = "Schwäbische Alb" });
                rs_new.Add(new newResourceStructure() { name = "Forest Multi-EPs", color = "#3a75e0", description = "Visit of new forest experimental Multi-EPs", duration = 1, quantity = 0, withActivity = true, resourceStructure = rs, type = "Area", explo = "Schwäbische Alb" });
                rs_new.Add(new newResourceStructure() { name = "Grassland Multi-EPs", color = "#6495ed", description = "Visit of new grassland experimental Multi-EPs", duration = 1, quantity = 0, withActivity = true, resourceStructure = rs, type = "Area", explo = "Schwäbische Alb" });

                rs_new.Add(new newResourceStructure() { name = "Sleeping place (ALB)", color = "Sleeping places in Schwäbische Alb", description = "#27509a", duration = 1, quantity = 8, withActivity = false, resourceStructure = rs, type = "Sleeping place", explo = "Schwäbische Alb" });

                rs_new.Add(new newResourceStructure() { name = "Drying cabinet (ALB)", color = "Drying cabinet in Schwäbische Alb", description = "#7e95bf", duration = 1, quantity = 5, withActivity = false, resourceStructure = rs, type = "Object", explo = "Schwäbische Alb" });
                rs_new.Add(new newResourceStructure() { name = "Binocular (ALB)", color = "", description = "#7e95bf", duration = 1, quantity = 1, withActivity = false, resourceStructure = rs, type = "Object", explo = "Schwäbische Alb" });
                rs_new.Add(new newResourceStructure() { name = "Metal detector (Magna Trak 100) (ALB)", color = "Metal detector (Magna Trak 100) in Schwäbische Alb", description = "#7e95bf", duration = 1, quantity = 1, withActivity = false, resourceStructure = rs, type = "Object", explo = "Schwäbische Alb" });



                foreach (newResourceStructure rs_item in rs_new)
                {
                    var duration = new TimeDuration();
                    duration.Value = rs_item.duration;
                    var resource = rManager.CreateResource(rs_item.name, rs_item.description, rs_item.quantity, rs_item.color, rs_item.withActivity, rs_item.resourceStructure, duration);
                    var valueManager = new ResourceStructureAttributeManager();
                    ResourceAttributeUsage usage = valueManager.GetResourceAttributeUsageById(1);
                    valueManager.CreateResourceAttributeValue(rs_item.explo, rManager.GetResourceById(resource.Id), usage);
                    ResourceAttributeUsage usage2 = valueManager.GetResourceAttributeUsageById(2);
                    valueManager.CreateResourceAttributeValue(rs_item.type, rManager.GetResourceById(resource.Id), usage);

                }
            }
            catch (Exception e)
            {
                Console.WriteLine("{0} Exception caught.", e);
            }


            #endregion

            #region SECURITY

            OperationManager operationManager = new OperationManager();
            FeatureManager featureManager = new FeatureManager();

            try
            {
                List<Feature> features = featureManager.FeatureRepository.Get().ToList();

                Feature ResourceBooking = features.FirstOrDefault(f => f.Name.Equals("Resource Booking"));
                if (ResourceBooking == null)
                    ResourceBooking = featureManager.Create("Resource Booking", "Resource Booking");



                Feature ResourceAdmin = features.FirstOrDefault(f => f.Name.Equals("Resource Administration"));
                if (ResourceAdmin == null)
                    ResourceAdmin = featureManager.Create("Resource Administration", "Resource Administration");

                Feature ResourceManagement = features.FirstOrDefault(f => f.Name.Equals("Resource Management"));
                if (ResourceManagement == null)
                    ResourceManagement = featureManager.Create("Resource Management", "Resource Management", ResourceAdmin);

                Feature ResourceStructureManagement = features.FirstOrDefault(f => f.Name.Equals("Resource Structure Management"));
                if (ResourceStructureManagement == null)
                    ResourceStructureManagement = featureManager.Create("Resource Structure Management", "Resource Structure Management", ResourceAdmin);

                Feature ResourceStructureAttributeManagement = features.FirstOrDefault(f => f.Name.Equals("Resource Structure Attribute Management"));
                if (ResourceStructureAttributeManagement == null)
                    ResourceStructureAttributeManagement = featureManager.Create("Resource Structure Attribute Management", "Resource Structure Attribute Management", ResourceAdmin);

                Feature NotificationManagement = features.FirstOrDefault(f => f.Name.Equals("Notification Management"));
                if (NotificationManagement == null)
                    NotificationManagement = featureManager.Create("Notification Management", "Notification Management", ResourceAdmin);

                Feature ActivityManagement = features.FirstOrDefault(f => f.Name.Equals("Activity Management"));
                if (ActivityManagement == null)
                    ActivityManagement = featureManager.Create("Activity Management", "Activity Management", ResourceAdmin);


                operationManager.Create("RBM", "Schedule", "*", ResourceBooking);
                operationManager.Create("RBM", "Calendar", "*", ResourceBooking);


                operationManager.Create("RBM", "Resource", "*", ResourceManagement);
                operationManager.Create("RBM", "ResourceStructure", "*", ResourceStructureManagement);
                operationManager.Create("RBM", "ResourceStructure", "*", ResourceStructureAttributeManagement);
                operationManager.Create("RBM", "Notification", "*", NotificationManagement);
                operationManager.Create("RBM", "Activity", "*", ActivityManagement);
            }
            catch (Exception ex)
            {
                throw ex;
            }

            finally
            {
                operationManager.Dispose();
                featureManager.Dispose();

            }

            #endregion
        }

        private List<DomainItem> CreateDomainItems(string[] keys)
        {
            List<DomainItem> domainItems = new List<DomainItem>();
            for (int i = 0; i < keys.Length; i++)
            {
                DomainItem domainItem = new DomainItem();
                domainItem.Key = keys[i];
                //for this implemention values are not needed now
                //domainItem.Value = value[i];
                domainItems.Add(domainItem);
            }

            return domainItems;
        }
    }
}
