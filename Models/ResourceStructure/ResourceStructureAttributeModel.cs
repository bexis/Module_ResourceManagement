using BExIS.Dlm.Entities.DataStructure;
using BExIS.Rbm.Entities.ResourceStructure;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using RS = BExIS.Rbm.Entities.ResourceStructure;

namespace BExIS.Web.Shell.Areas.RBM.Models.ResourceStructure
{
    public class ResourceStructureAttributeModel
    {
        [Display(Name = "ID")]
        //[Editable(false)]
        //[Required]
        public long AttributeId { get; set; }

        [Display(Name = "Name")]
        [StringLength(50, ErrorMessage = "The resource structure attribute name must be {2} - {1} characters long.", MinimumLength = 3)]
        //[Remote("ValidateResourceStructureName", "ResourceStructures")]
        [Required]
        public string AttributeName { get; set; }

        [Display(Name = "Description")]
        [StringLength(250, ErrorMessage = "The description must be less than {1} characters long.")]
        public string AttributeDescription { get; set; }

        public bool InUse { get; set; }

        //define if user has edit access to the attribute
        public bool EditAccess { get; set; }

        //define if user has delete access to the attribute
        public bool DeleteAccess { get; set; }

        public long rsID { get; set; }

        //List<Constraint> Constraints { get; set; }
        public DomainConstraintModel DomainConstraint { get; set; }


        public ResourceStructureAttributeModel()
        {
            AttributeId = 0;
            AttributeName = "";
            AttributeDescription = "";
            rsID = 0;
            InUse = false;
            EditAccess = false;
            DeleteAccess = false;

            //Constraints = new List<Constraint>();

            //
            DomainConstraint = new DomainConstraintModel();
            //DomainItem i = new DomainItem();
            //i.Key = "";
            //i.Value = "";
            //DomainConstraint.Items.Add(new DomainItemModel(i));
        }

        public ResourceStructureAttributeModel(ResourceStructureAttribute attribute)
        {
            AttributeId = attribute.Id;
            AttributeName = attribute.Name;
            AttributeDescription = attribute.Description;


            foreach (Constraint constraint in attribute.Constraints)
            {
                if (constraint is DomainConstraint)
                {
                    DomainConstraint dc = (DomainConstraint)constraint;
                    dc.Materialize();
                    DomainConstraint = new DomainConstraintModel(dc);
                }
            }

        }

        public static ResourceStructureAttributeModel Convert(RS.ResourceStructureAttribute attribute)
        {
            DomainConstraint dc = (DomainConstraint)attribute.Constraints.Where(p => p.GetType().Equals(typeof(DomainConstraint)));

            return new ResourceStructureAttributeModel()
            {
                AttributeId = attribute.Id,
                AttributeName = attribute.Name,
                AttributeDescription = attribute.Description,
                //Constraints = attribute.Constraints.ToList()
                DomainConstraint = new DomainConstraintModel(dc)
            };
        }
    }

  
    public class EditResourceStructureAttributeModel
    {
        public long Id { get; set; }


        [Display(Name = "Name")]
        [StringLength(50, ErrorMessage = "The resource structure attribute name must be {2} - {1} characters long.", MinimumLength = 3)]
        //[Remote("ValidateResourceStructureName", "ResourceStructures")]
        [Required]
        public string AttributeName { get; set; }

        [Display(Name = "Description")]
        [StringLength(250, ErrorMessage = "The description must be less than {1} characters long.")]
        public string AttributeDescription { get; set; }

        public long rsID { get; set; }

        public List<DomainItemModel> DomainItems { get; set; }

        public long ResourceStructureId { get; set; }


        public EditResourceStructureAttributeModel()
        {
            DomainItems = new List<DomainItemModel>();
        }
    }

  }