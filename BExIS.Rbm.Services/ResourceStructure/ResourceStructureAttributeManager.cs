using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Vaiona.Persistence.Api;
using RS = BExIS.Rbm.Entities.ResourceStructure;
using R = BExIS.Rbm.Entities.Resource;
using BExIS.Dlm.Entities.DataStructure;
using BExIS.Dlm.Services;
using System.Diagnostics.Contracts;
using BExIS.Dlm.Services.DataStructure;
using BExIS.Rbm.Entities.ResourceStructure;
using BExIS.Rbm.Entities.Resource;

namespace BExIS.Rbm.Services.ResourceStructure
{
    public class ResourceStructureAttributeManager :IDisposable
    {
        private readonly IUnitOfWork _guow;
        private bool _isDisposed;

        public ResourceStructureAttributeManager()
        {
            _guow = this.GetIsolatedUnitOfWork();
            this.ResourceStructureAttrRepo = _guow.GetReadOnlyRepository<RS.ResourceStructureAttribute>();
            this.ResourceAttributeValueRepro = _guow.GetReadOnlyRepository<RS.ResourceAttributeValue>();
            this.ResourceStructureRepro = _guow.GetReadOnlyRepository<RS.ResourceStructure>();
            this.ResourceAttributeUsageRepro = _guow.GetReadOnlyRepository<RS.ResourceAttributeUsage>();
            this.TextValueRepro = _guow.GetReadOnlyRepository<RS.TextValue>();

        }

        ~ResourceStructureAttributeManager()
        {
            Dispose(true);
        }

        public void Dispose()
        {
            Dispose(true);
        }

        public void Dispose(bool disposing)
        {
            if (!_isDisposed)
            {
                if (disposing)
                {
                    if (_guow != null)
                        _guow.Dispose();
                    _isDisposed = true;
                }
            }
        }

        #region ResourceStructureAttribute

        public RS.ResourceStructureAttribute CreateResourceStructureAttribute(string name, string description)
        {
            Contract.Requires(!string.IsNullOrWhiteSpace(name));
            Contract.Requires(!String.IsNullOrWhiteSpace(description));

            DataTypeManager dataTypeManager = new DataTypeManager(); 
            DataType type = dataTypeManager.Repo.Get(p => p.SystemType.Equals("String")).FirstOrDefault();
            if (type == null)
            {
                type = dataTypeManager.Create("String", "string", TypeCode.String);

            }

            RS.ResourceStructureAttribute resourceStrucAtt = new RS.ResourceStructureAttribute()
            {
                Name = name,
                Description = description,
                DataType = type,
            };

            using (IUnitOfWork uow = this.GetUnitOfWork())
            {
                IRepository<RS.ResourceStructureAttribute> repo = uow.GetRepository<RS.ResourceStructureAttribute>();
                repo.Put(resourceStrucAtt);
                uow.Commit();
            }

            return resourceStrucAtt;
        }

        public bool DeleteResourceStructureAttribute(RS.ResourceStructureAttribute resourceStrucAtt)
        {
            Contract.Requires(resourceStrucAtt != null);
            Contract.Requires(resourceStrucAtt.Id >= 0);

            using (IUnitOfWork uow = this.GetUnitOfWork())
            {
                IRepository<RS.ResourceStructureAttribute> repoAttr = uow.GetRepository<RS.ResourceStructureAttribute>();
                repoAttr.Delete(resourceStrucAtt);
                uow.Commit();
            }

            return true;
        }

        public bool DeleteResourceStructureAttribute(IEnumerable<RS.ResourceStructureAttribute> resourceStrucAtts)
        {
            using (IUnitOfWork uow = this.GetUnitOfWork())
            {
                IRepository<RS.ResourceStructureAttribute> repo = uow.GetRepository<RS.ResourceStructureAttribute>();
                foreach (var resourceStrucAtt in resourceStrucAtts)
                {
                    var latest = repo.Reload(resourceStrucAtt);
                    repo.Delete(latest);
                }

                uow.Commit();
            }

            return true;
        }

        public RS.ResourceStructureAttribute UpdateResourceStructureAttribute(RS.ResourceStructureAttribute resourceStructureAttribute)
        {
            Contract.Requires(resourceStructureAttribute != null);

            using (IUnitOfWork uow = this.GetUnitOfWork())
            {
                IRepository<RS.ResourceStructureAttribute> repo = uow.GetRepository<RS.ResourceStructureAttribute>();
                repo.Merge(resourceStructureAttribute);
                var merged = repo.Get(resourceStructureAttribute.Id);
                repo.Put(merged);
                uow.Commit();
            }

            return resourceStructureAttribute;
        }

        public bool IsAttributeInUse(long attrId)
        {
            if (ResourceAttributeUsageRepro.Query(u => u.ResourceStructureAttribute.Id == attrId).Count() > 0)
                return true;
            else
                return false;
        }

        public RS.ResourceStructureAttribute GetResourceStructureAttributesById(long id)
        {
            return ResourceStructureAttrRepo.Query(u => u.Id == id).FirstOrDefault();
        }

        public RS.ResourceStructureAttribute GetResourceStructureAttributesByName(string name)
        {
            return ResourceStructureAttrRepo.Query(u => u.Name.ToLower() == name.ToLower()).FirstOrDefault();
        }

        public IQueryable<RS.ResourceStructureAttribute> GetAllResourceStructureAttributes()
        {
            return ResourceStructureAttrRepo.Query();
        }

        #endregion

        #region ResourceAttributeUsage

        public RS.ResourceAttributeUsage CreateResourceAttributeUsage(RS.ResourceStructureAttribute rsa, RS.ResourceStructure rs, bool isOptional, bool isFileDataType)
        {
            RS.ResourceAttributeUsage resourceAttributeUsage = new RS.ResourceAttributeUsage()
            {
                Label = rsa.Name,
                ResourceStructure = rs,
                ResourceStructureAttribute = rsa,
                IsValueOptional = isOptional,
                IsFileDataType = isFileDataType
            };

            using (IUnitOfWork uow = this.GetUnitOfWork())
            {
                IRepository<RS.ResourceAttributeUsage> repo = uow.GetRepository<RS.ResourceAttributeUsage>();
                repo.Put(resourceAttributeUsage);
                uow.Commit();
            }

            return resourceAttributeUsage;
        }

        public bool DeleteResourceAttributeUsage(RS.ResourceAttributeUsage usage)
        {
            Contract.Requires(usage != null);
            Contract.Requires(usage.Id >= 0);

            using (IUnitOfWork uow = this.GetUnitOfWork())
            {
                IRepository<RS.ResourceAttributeUsage> repoUsage = uow.GetRepository<RS.ResourceAttributeUsage>();
                repoUsage.Delete(usage);
                uow.Commit();
            }

            return true;
        }

        public bool DeleteResourceAttributeUsages(IEnumerable<RS.ResourceAttributeUsage> usages)
        {
            using (IUnitOfWork uow = this.GetUnitOfWork())
            {
                IRepository<RS.ResourceAttributeUsage> repoUsage = uow.GetRepository<RS.ResourceAttributeUsage>();

                foreach (RS.ResourceAttributeUsage usage in usages)
                {
                    Contract.Requires(usage != null);
                    Contract.Requires(usage.Id >= 0);
                    repoUsage.Delete(usage);
                }
                uow.Commit();
            }

            return true;
        }

        public bool DeleteUsagesByRSId(long id)
        {
            List<RS.ResourceAttributeUsage> list = ResourceAttributeUsageRepro.Query(a => a.ResourceStructure.Id == id).ToList();
            foreach (RS.ResourceAttributeUsage s in list)
            {
                using (IUnitOfWork uow = this.GetUnitOfWork())
                {
                    IRepository<RS.ResourceAttributeUsage> repo = uow.GetRepository<RS.ResourceAttributeUsage>();
                    RS.ResourceAttributeUsage deletedUsage = s;
                    deletedUsage = repo.Reload(s);
                    repo.Delete(deletedUsage);
                    uow.Commit();
                }
            }

            return true;
        }

        public RS.ResourceAttributeUsage UpdateResourceAttributeUsage(RS.ResourceAttributeUsage usage)
        {
            Contract.Requires(usage != null);

            using (IUnitOfWork uow = this.GetUnitOfWork())
            {
                IRepository<RS.ResourceAttributeUsage> repo = uow.GetRepository<RS.ResourceAttributeUsage>();
                repo.Merge(usage);
                var merged = repo.Get(usage.Id);
                repo.Put(merged);
                uow.Commit();
            }

            return usage;
        }

        public RS.ResourceAttributeUsage GetResourceAttributeUsageById(long id)
        {
            return ResourceAttributeUsageRepro.Query(u => u.Id == id).FirstOrDefault();
        }


        public List<RS.ResourceAttributeUsage> GetResourceStructureAttributeUsagesByRSId(long rsId)
        {
            List<RS.ResourceAttributeUsage> list = new List<RS.ResourceAttributeUsage>();
            return ResourceAttributeUsageRepro.Query(u => u.ResourceStructure.Id == rsId).ToList();
        }

        #endregion

        #region ResourceAttributeValues

        public RS.TextValue CreateResourceAttributeValue(string value, R.Resource resource, RS.ResourceAttributeUsage resourceAttributeUsage)
        {
            Contract.Requires(!string.IsNullOrWhiteSpace(value));

            RS.TextValue textValue = new RS.TextValue()
            {
                Value = value,
                Resource = resource,
                ResourceAttributeUsage = resourceAttributeUsage
            };

            using (IUnitOfWork uow = this.GetUnitOfWork())
            {
                IRepository<RS.TextValue> repo = uow.GetRepository<RS.TextValue>();
                repo.Put(textValue);
                uow.Commit();
            }
            return textValue;
        }

        public RS.FileValue CreateResourceAttributeValue(string name, string extention, string minmetype, byte[] data, bool needConfirmation, R.Resource resource, RS.ResourceAttributeUsage resourceAttributeUsage)
        {
            //Contract.Requires(!string.IsNullOrWhiteSpace(value));

            RS.FileValue fileValue = new RS.FileValue()
            {
                Name = name,
                Extention = extention,
                Minmetype = minmetype,
                Data = data,
                NeedConfirmation = needConfirmation,
                Resource = resource,
                ResourceAttributeUsage = resourceAttributeUsage
            };

            using (IUnitOfWork uow = this.GetUnitOfWork())
            {
                IRepository<RS.FileValue> repo = uow.GetRepository<RS.FileValue>();
                repo.Put(fileValue);
                uow.Commit();
            }
            return fileValue;
        }

        public RS.TextValue UpdateResourceAttributeValue(long id, string value)
        {
            Contract.Requires(value != null);
            TextValue tv = GetTextValueById(id);
            tv.Value = value;
           
            using (IUnitOfWork uow = this.GetUnitOfWork())
            {
                IRepository<RS.TextValue> repo = uow.GetRepository<RS.TextValue>();
                repo.Put(tv);
                uow.Commit();
            }

            return tv;
        }

        public RS.FileValue UpdateResourceAttributeValue(long id, string name, string extention, string minmetype, byte[] data)
        {
            Contract.Requires(id != null);
            FileValue fv = GetFileValueById(id);
            fv.Name = name;
            fv.Extention = extention;
            fv.Minmetype = minmetype;
            fv.Data = data;

            using (IUnitOfWork uow = this.GetUnitOfWork())
            {
                IRepository<RS.FileValue> repo = uow.GetRepository<RS.FileValue>();
                repo.Put(fv);
                uow.Commit();
            }

            return fv;
        }



        public bool DeleteResourceAttributeValue(RS.ResourceAttributeValue value)
        {
            Contract.Requires(value != null);
            Contract.Requires(value.Id >= 0);

            using (IUnitOfWork uow = this.GetUnitOfWork())
            {
                IRepository<RS.ResourceAttributeValue> repo = uow.GetRepository<RS.ResourceAttributeValue>();
                value = repo.Reload(value);
                repo.Delete(value);

                uow.Commit();
            }

            return true;
        }

        public bool DeleteResourceStructureAttributeValues(IEnumerable<RS.ResourceAttributeValue> values)
        {
            using (IUnitOfWork uow = this.GetUnitOfWork())
            {
                IRepository<RS.ResourceAttributeValue> repo = uow.GetRepository<RS.ResourceAttributeValue>();
                foreach (var value in values)
                {
                    var latest = repo.Reload(value);
                    repo.Delete(latest);
                }
                uow.Commit();
            }

            return true;
        }

        public RS.ResourceAttributeValue UpdateResourceAttributeValue(RS.ResourceAttributeValue value)
        {
            Contract.Requires(value != null);

            using (IUnitOfWork uow = this.GetUnitOfWork())
            {
                IRepository<RS.ResourceAttributeValue> repo = uow.GetRepository<RS.ResourceAttributeValue>();
                repo.Merge(value);
                var merged = repo.Get(value.Id);
                repo.Put(merged);
                uow.Commit();
            }

            return value;
        }

        public List<RS.ResourceAttributeValue> GetValuesByResource(R.Resource resource)
        {
            return ResourceAttributeValueRepro.Query(u => u.Resource.Id == resource.Id).ToList();         
        }

        public TextValue GetTextValueById(long id)
        {
            return (TextValue)ResourceAttributeValueRepro.Query(u => u.Id == id).FirstOrDefault();
        }

        public FileValue GetFileValueById(long id)
        {
            return (FileValue)ResourceAttributeValueRepro.Query(u => u.Id == id).FirstOrDefault();
        }

        public TextValue GetTextValueByUsageAndResource(long usageId, long resouceId)
        {
            return (TextValue)ResourceAttributeValueRepro.Query(u => u.ResourceAttributeUsage.Id == usageId && u.Resource.Id == resouceId).FirstOrDefault();
        }

        public ResourceAttributeValue GetValueByUsageAndResource(long usageId, long resouceId)
        {
            return (ResourceAttributeValue)ResourceAttributeValueRepro.Query(u => u.ResourceAttributeUsage.Id == usageId && u.Resource.Id == resouceId).FirstOrDefault();
        }

        public List<long> GetResourcesByValues(List<string> values)
        {
            List<long> result = new List<long>();
            foreach (string v in values)
            {
                List<long> temp = TextValueRepro.Query(u => u.Value==v).Select(u=>u.Resource.Id).ToList();
                if (result.Count() == 0)
                {
                    result = temp;
                }
                else
                {
                    result = result.Intersect(temp).ToList();
                }
            }
            return result;


            //List<TextValue> t = TextValueRepro.Query(u=>u.Value.Contains()).ToList();
            //return t.Select(a => a.Resource).ToList();
        }

        #endregion

        #region Data reader

        public IReadOnlyRepository<RS.ResourceStructureAttribute> ResourceStructureAttrRepo { get; private set; }
        public IReadOnlyRepository<RS.ResourceAttributeValue> ResourceAttributeValueRepro { get; private set; }
        public IReadOnlyRepository<RS.TextValue> TextValueRepro { get; private set; }
        public IReadOnlyRepository<RS.ResourceStructure> ResourceStructureRepro { get; private set; }
        public IReadOnlyRepository<RS.ResourceAttributeUsage> ResourceAttributeUsageRepro { get; private set; }

        #endregion
    }
}
