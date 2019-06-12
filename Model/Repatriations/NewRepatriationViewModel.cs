using System;
using System.ComponentModel.DataAnnotations;

namespace HRCentral.Web.Models.Repatriations
{
    public class NewRepatriationViewModel
    {
        [Display(Name = "Employee")]
        [Required(ErrorMessage = "Employee is required")]
        public Guid? EmployeeId { get; set; }

        [Required]
        [Display(Name = "Repatriation Name")]
        public string Name { get; set; }

        [Display(Name = "Relationship")]
        [Required(ErrorMessage = "Relationship is required")]
        public Guid? RelationshipId { get; set; }
    }
}
