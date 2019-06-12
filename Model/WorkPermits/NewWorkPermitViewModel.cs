using System;
using System.ComponentModel.DataAnnotations;

namespace HRCentral.Web.Models.WorkPermits
{
    public class NewWorkPermitViewModel
    {
        [Display(Name = "Employee")]
        [Required(ErrorMessage = "Employee Id is required")]
        public Guid? EmployeeId { get; set; }

        [RegularExpression(@"^\d+$", ErrorMessage = "Work Permit Number must contain only numbers. e.g 475753583")]
        [Display(Name = "WorkPermit Number")]
        [Required]
        public string WorkPermitNumber { get; set; }

        [Display(Name = "Expiry Date")]
        [Required]
        public string ExpiryDate { get; set; }

        [Display(Name = "Employee")]
        public string Employee { get; internal set; }
    }
}