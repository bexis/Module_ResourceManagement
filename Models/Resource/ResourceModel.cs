using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using RS = BExIS.Rbm.Entities.ResourceStructure;
using R = BExIS.Rbm.Entities.Resource;
using BExIS.Rbm.Services.ResourceStructure;
using BExIS.Web.Shell.Areas.RBM.Models.ResourceStructure;
using BExIS.Rbm.Entities.Resource;
using System.ComponentModel.DataAnnotations;
using BExIS.Rbm.Entities.ResourceStructure;
using BExIS.Web.Shell.Areas.RBM.Models.Booking;
using BExIS.Rbm.Entities.ResourceConstraint;
using BExIS.Rbm.Entities.BookingManagementTime;

namespace BExIS.Web.Shell.Areas.RBM.Models.Resource
{
    public class ResourceModel
    {
        public long Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public int Quantity { get; set; }
        public string Color { get; set; }

        //public R.BookingTimeGranularity BookingTimeGranularity { get; set; }
        //BookingTimeGranularity
        public TimeDuration Duration { get; set; }
        public List<SystemDefinedUnit> TimeUnits { get; set; }

        public Status ResourceStatus { get; set; }
        public RS.ResourceStructure ResourceStructure { get; set; }
        public string ResourceStructureName { get; set; }
        public List<ResourceStructureAttributeValueModel> ResourceStructureAttributeValue { get; set; }

        //public R.ResourceClassifier ResourceClassifier { get; set; }

        public ResourceModel()
        {
            Name = "";
            Description = "";
            ResourceStatus = Status.created;
            ResourceStructure = new RS.ResourceStructure();
            ResourceStructureAttributeValue = new List<ResourceStructureAttributeValueModel>();
            //ResourceClassifier = new R.ResourceClassifier();
            TimeUnits = Enum.GetValues(typeof(SystemDefinedUnit)).Cast<SystemDefinedUnit>().ToList();
            Color = "";
        }

        public ResourceModel(R.SingleResource resource)
        {
            Id = resource.Id;
            Name = resource.Name;
            Description = resource.Description;
            ResourceStatus = resource.ResourceStatus;
            ResourceStructureName = resource.ResourceStructure.Name;
            ResourceStructure = resource.ResourceStructure;
            Color = resource.Color;

            TimeUnits = Enum.GetValues(typeof(SystemDefinedUnit)).Cast<SystemDefinedUnit>().ToList();

            //ResourceStructureAttributeValue
        }


        public ResourceModel Convert(R.SingleResource resource)
        {
            return new ResourceModel()
            {
                Id = resource.Id,
                Name = resource.Name,
                Description = resource.Description,
                ResourceStatus = resource.ResourceStatus,
                ResourceStructure = resource.ResourceStructure,
                Color = resource.Color,
                TimeUnits = Enum.GetValues(typeof(SystemDefinedUnit)).Cast<SystemDefinedUnit>().ToList()

            };
        }
    }

    public class EditResourceModel
    {
        #region Properties

        public long Id { get; set; }

        [Display(Name = "Name")]
        [StringLength(50, ErrorMessage = "The resource name must be {2} - {1} characters long.", MinimumLength = 3)]
        [Required]
        public string Name { get; set; }

        [Display(Name = "Description")]
        [StringLength(250, ErrorMessage = "The description must be less than {1} characters long.")]
        public string Description { get; set; }

        [Display(Name = "Quantity")]
        [Range(0, Int32.MaxValue, ErrorMessage = "Invalid Number")]
        public int Quantity { get; set; }

        [Required]
        public string Color { get; set; }

        public bool WithActivity { get; set; }

        //BookingTimeGranularity
        public TimeDuration Duration { get; set; }

        [Display(Name = "Time Unit")]
        public List<SystemDefinedUnit> TimeUnits { get; set; }

        public ResourceStructureModel ResourceStructure { get; set; }

        public ResourceModel Parent { get; set; }

        #endregion

        #region Filled Lists

        //available resources structures
        public List<ResourceStructureModel> ResourceStructures { get; set; }

        #endregion

        #region ResourceStructure

        public List<ResourceStructureAttributeValueModel> ResourceStructureAttributeValues { get; set; }
        public List<TextValueModel> TextValues { get; set; }
        public List<FileValueModel> FileValues { get; set; }

        #endregion

        #region Constraints

        public ResourceConstraintModel ResourceConstraintModel { get; set; }
        public List<ResourceConstraintModel> ResourceConstraints { get; set; }
        public List<DependencyConstraintModel> DependencyConstraints { get; set; }
        public List<BlockingConstraintModel> BlockingConstraints { get; set; }
        public List<QuantityConstraintModel> QuantityConstraints { get; set; }

        #endregion

        //Default Constructor
        public EditResourceModel()
        {
            ResourceStructures = new List<ResourceStructureModel>();

            ResourceStructureAttributeValues = new List<ResourceStructureAttributeValueModel>();
            TextValues = new List<Models.ResourceStructure.TextValueModel>();
            FileValues = new List<Models.ResourceStructure.FileValueModel>();

            ResourceStructureManager manager = new ResourceStructureManager();
            Parent = new ResourceModel();
            foreach (RS.ResourceStructure rs in manager.GetAllResourceStructures().ToList())
            {
                ResourceStructures.Add(new ResourceStructureModel(rs));
            }

            //BookingTimeGranularity
            Duration = new TimeDuration();
            Duration.Value = 1;
            TimeUnits = Enum.GetValues(typeof(SystemDefinedUnit)).Cast<SystemDefinedUnit>().ToList();


            ResourceConstraintModel = new ResourceConstraintModel();
            ResourceConstraints = new List<ResourceConstraintModel>();
            DependencyConstraints = new List<DependencyConstraintModel>();
            BlockingConstraints = new List<BlockingConstraintModel>();
            QuantityConstraints = new List<QuantityConstraintModel>();
        }

        public EditResourceModel(R.SingleResource resource, List<ResourceStructureAttributeValueModel> valuesModel, List<TextValueModel> textValues, List<FileValueModel> fileValues)
        {
            Id = resource.Id;
            Name = resource.Name;
            Description = resource.Description;
            Quantity = resource.Quantity;
            ResourceStructure = new ResourceStructureModel(resource.ResourceStructure);
            ResourceStructureAttributeValues = valuesModel;
            ResourceStructures = new List<ResourceStructureModel>();
            Color = resource.Color;
            WithActivity = resource.WithActivity;
            ResourceConstraints = new List<Resource.ResourceConstraintModel>();
            DependencyConstraints = new List<DependencyConstraintModel>();
            BlockingConstraints = new List<BlockingConstraintModel>();
            QuantityConstraints = new List<QuantityConstraintModel>();
            //TextValues = new List<TextValueModel>();
            //FileValues = new List<FileValueModel>();

            TextValues = textValues;
            FileValues = fileValues;

            //BookingTimeGranularity
            Duration = new TimeDuration();
            Duration = resource.Duration.Self;
            TimeUnits = Enum.GetValues(typeof(SystemDefinedUnit)).Cast<SystemDefinedUnit>().ToList();

            ResourceStructureManager manager = new ResourceStructureManager();

            foreach (RS.ResourceStructure rs in manager.GetAllResourceStructures().ToList())
            {
                ResourceStructures.Add(new ResourceStructureModel(rs));
            }

            //BookingTimeGranularity = resource.BookingTimeGranularity;

            ResourceConstraintModel = new ResourceConstraintModel();

            if (resource.ResourceConstraints != null)
            {
                foreach (ResourceConstraint c in resource.ResourceConstraints)
                {
                    if (c is DependencyConstraint)
                    {
                        DependencyConstraintModel dcModel = new DependencyConstraintModel((DependencyConstraint)c);
                        dcModel.Id = c.Id;
                        dcModel.Index = c.Index;
                        DependencyConstraints.Add(dcModel);
                        ResourceConstraints.Add(dcModel);
                    }

                    if (c is BlockingConstraint)
                    {
                        BlockingConstraintModel bcModel = new BlockingConstraintModel((BlockingConstraint)c);
                        bcModel.Id = c.Id;
                        bcModel.Index = c.Index;
                        BlockingConstraints.Add(bcModel);
                        ResourceConstraints.Add(bcModel);
                    }

                    if (c is QuantityConstraint)
                    {
                        QuantityConstraintModel qcModel = new QuantityConstraintModel((QuantityConstraint)c);
                        qcModel.Id = c.Id;
                        qcModel.Index = c.Index;
                        QuantityConstraints.Add(qcModel);
                        ResourceConstraints.Add(qcModel);
                    }
                }
                //Sort by Index 
                ResourceConstraints = ResourceConstraints.OrderBy(x => x.Index).ToList();
            }

        }
    }
    public class ShowResourceModel
    {
            public long Id { get; set; }
            public string Name { get; set; }
            public string Description { get; set; }
            public int Quantity { get; set; }
            public int Duration { get; set; }
            public string TimeUnit { get; set; }
            public string ResourceStructureName { get; set; }

            public List<ResourceStructureAttributeValueModel> ResourceStructureAttributeValues { get; set; }

            public ShowResourceModel(R.SingleResource resource, List<ResourceStructureAttributeValueModel> valuesModel)
            {
                Id = resource.Id;
                Name = resource.Name;
                Description = resource.Description;
                Quantity = resource.Quantity;
                Duration = resource.Duration.Value;
                TimeUnit = resource.Duration.TimeUnit.ToString();
                ResourceStructureName = resource.ResourceStructure.Name;
                ResourceStructureAttributeValues = valuesModel;
                //ResourceStructure = new ResourceStructureModel(resource.ResourceStructure);
            }

        }

        /// <summary>
        /// Model create a item for diplay a resource in the resource manger view.
        /// </summary>
        public class ResourceManagerModel
        {
            public long Id { get; set; }
            public string Name { get; set; }
            public string Description { get; set; }
            public string ResourceStructureName { get; set; }
            public string Quantity { get; set; }

            public bool InUse { get; set; }

            //define if user has edit access to the attribute
            public bool EditAccess { get; set; }

            //define if user has delete access to the attribute
            public bool DeleteAccess { get; set; }

            //Index of row, needed in DependencyConstraint to add a dependend resource
            public int Index { get; set; }

            public ResourceManagerModel(R.SingleResource resource)
            {
                Id = resource.Id;
                Name = resource.Name;
                Description = resource.Description;
                ResourceStructureName = resource.ResourceStructure.Name;
                if (resource.Quantity == 0)
                    Quantity = "no limitation";
                else
                    Quantity = resource.Quantity.ToString();

                InUse = false;
            }
        }

        /// <summary>
        /// Model gives you from a Resource their resource attribute (from resource structure) Ids and their values. Needed for Filtering Resource by Domain contrains
        /// </summary>
        public class ResourceAttributeValueModel
        {
            public List<long> AttributeIds { get; set; }
            public List<string> Values { get; set; }
            public R.SingleResource Resource { get; set; }

            public ResourceAttributeValueModel(R.SingleResource resource)
            {
                AttributeIds = new List<long>();
                //get only Id if attr has domain constraint
                foreach (ResourceAttributeUsage u in resource.ResourceStructure.ResourceAttributeUsages)
                {
                    if (u.IsFileDataType == false)
                    {
                        if (u.ResourceStructureAttribute.Constraints.Count > 0)
                        {
                            AttributeIds.Add(u.ResourceStructureAttribute.Id);
                        }
                    }
                }
                //AttributeIds = resource.ResourceStructure.ResourceAttributeUsages.Select(a => a.ResourceStructureAttribute.Id).ToList();

                Values = new List<string>();
                ResourceStructureAttributeManager rsaManager = new ResourceStructureAttributeManager();
                List<RS.ResourceAttributeValue> valueList = rsaManager.GetValuesByResource(resource);
                foreach (RS.ResourceAttributeValue v in valueList)
                {
                    if (v is TextValue)
                    {
                        TextValue tv = (TextValue)v;
                        Values.Add(tv.Value);
                    }
                    //Values = valueList.Select(a => a.Value).ToList();
                    Resource = resource;
                }
            }
        }
    }


