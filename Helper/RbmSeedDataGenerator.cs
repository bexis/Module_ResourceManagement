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

        public void GenerateSeedData()
        {
            #region ENTITIES

            Dictionary<string, Type> rbmEntities = new Dictionary<string,Type>();
            rbmEntities.Add("SingleResource", typeof(SingleResource));
            rbmEntities.Add("ResourceStructure", typeof(ResourceStructure));
            rbmEntities.Add("ResourceStructureAttribute", typeof(ResourceStructureAttribute));
            rbmEntities.Add("Activity", typeof(Activity));
            rbmEntities.Add("Event", typeof(RealEvent));
            rbmEntities.Add("Notification", typeof(Notification));
            rbmEntities.Add("Schedule", typeof(Schedule));

            using (var entityManager = new EntityManager())
            {
                foreach (var et in rbmEntities)
                {
                    Entity entity = entityManager.Entities.Where(e => e.Name.ToUpperInvariant() == et.Key.ToUpperInvariant()).FirstOrDefault();

                    if (entity == null)
                    {
                        entity = new Entity();
                        entity.Name = et.Key;
                        entity.EntityType = et.Value;
                        //entity.EntityStoreType = typeof(Xml.Helpers.DatasetStore);
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