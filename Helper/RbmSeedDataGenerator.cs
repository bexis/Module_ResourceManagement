using BExIS.Rbm.Entities.Resource;
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
            throw new NotImplementedException();
        }

        public void GenerateSeedData()
        {
            #region ENTITIES

            List<string> rbmEntities = new List<string>();
            rbmEntities.Add("Activity");
            rbmEntities.Add("SingleResource");
            rbmEntities.Add("Event");
            rbmEntities.Add("Notification");
            rbmEntities.Add("Schedule");

            using (var entityManager = new EntityManager())
            {
                foreach (string et in rbmEntities)
                {
                    Entity entity = entityManager.Entities.Where(e => e.Name.ToUpperInvariant() == et.ToUpperInvariant()).FirstOrDefault();

                    if (entity == null)
                    {
                        entity = new Entity();
                        entity.Name = et;
                        entity.EntityType = Type.GetType(et);
                        //entity.EntityStoreType = typeof(Xml.Helpers.DatasetStore);
                        //entity.UseMetadata = true;
                        //entity.Securable = true;

                        entityManager.Create(entity);
                    }
                }
            }
            #endregion

            #region SECURITY



            #endregion
        }
    }
}