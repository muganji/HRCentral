using System;
using System.ComponentModel.DataAnnotations;

namespace HRCentral.Web.Models.NextOfKins
{
    public class NextOfKinDetailViewModel
    {
        [Required]
        public Guid Id { get; set; }

        [Required(ErrorMessage = "Employee is required.")]
        [Display(Name = "Employee")]
        public Guid? EmployeeId { get; set; }

        //public string Address { get; set; }

        [Required(ErrorMessage = "Next of kin contact info is required.")]
        public string ContactInfo { get; set; }

     
        //public string Residence { get; set; }

        [Required(ErrorMessage = "Relationship is required.")]
        [Display(Name = "Relationship")]
        public Guid? RelationshipId { get; set; }

        [Required(ErrorMessage = "Next of kin name is required.")]
        [Display(Name = "NextofKin Name")]
        public string Name { get; set; }
    }
}