using System;
using System.ComponentModel.DataAnnotations;

namespace HRCentral.Web.Models.NextOfKins
{
    public class NewNextOfKinViewModel
    {
        [Required(ErrorMessage = "Employee is required.")]
        [Display(Name = "Employee")]
        public Guid? EmployeeId { get; set; }

        //public string Address { get; set; }

        [Required(ErrorMessage = "Next of kin contact info is required.")]
        public string ContactInfo { get; set; }

        //[Required(ErrorMessage = "Next of Kin residence is required.")]
        //[Display(Name = "Home Address")]
        //public string Residence { get; set; }

        [Required(ErrorMessage = "Relationship is required.")]
        [Display(Name = "Relationship")]
        public Guid? RelationshipId { get; set; }

        [Required(ErrorMessage = "Next of kin name is required.")]
        [Display(Name = "NextofKin Name")]
        [RegularExpression(@"^[a-zA-Z\\-\\_\s]*$", ErrorMessage = "Name contains only Characters.")]
        [StringLength(100, ErrorMessage = "Name must be atmost 100 characters long.")]
        public string Name { get; set; }
    }
}