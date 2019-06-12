using System;
using System.ComponentModel.DataAnnotations;

namespace HRCentral.Web.Models.Email
{
    public class NewEmailViewModel
    {
        [Display(Name = "Employee")]
        [Required(ErrorMessage = "Employee is required")]
        public Guid? EmployeeId { get; set; }

        [Display(Name = "Work Email")]
        [Required]
        public string EmailAddress { get; set; }
    }
}
