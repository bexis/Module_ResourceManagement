using BExIS.Rbm.Entities.Booking;
using BExIS.Rbm.Entities.Resource;
using BExIS.Rbm.Entities.ResourceStructure;
using BExIS.Security.Entities.Objects;
using BExIS.Security.Services.Objects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace BExIS.Modules.RBM.UI.Helper
{
    public class RbmSeedDataGenerator : IDisposable
    {
        public void Dispose()
        {
            //throw new NotImplementedException();
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
            #endregion

            #region SECURITY

            using (var featureManager = new FeatureManager())
            {
                List<Feature> features = featureManager.FeatureRepository.Get().ToList();

                Feature ResourceBooking = features.FirstOrDefault(f => f.Name.Equals("Resource Booking"));
                if (ResourceBooking == null) ResourceBooking = featureManager.Create("Resource Booking", "Resource Booking");
            }


            #endregion
        }
    }
}