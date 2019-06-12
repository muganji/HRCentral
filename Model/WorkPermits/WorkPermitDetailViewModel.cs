using System;
using System.ComponentModel.DataAnnotations;

namespace HRCentral.Web.Models.WorkPermits
{
    public class WorkPermitDetailViewModel
    {
        public Guid Id { get; set; }

        [Display(Name = "Employee")]
        [Required(ErrorMessage = "Employee Id is required")]
        public Guid? EmployeeId { get; set; }

        [Required]
        [Display(Name = "PermitNumber")]
        public string WorkPermitNumber { get; set; }

        [Required]
        [Display(Name = "ExpiryDate")]
        public string ExpiryDate { get; set; }
    }
}