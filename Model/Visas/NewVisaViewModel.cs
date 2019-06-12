using System;
using System.ComponentModel.DataAnnotations;

namespace HRCentral.Web.Models.Visas
{
    public class NewVisaViewModel
    {
        [Display(Name = "Employee")]
        [Required(ErrorMessage = "Employee Id is required")]
        public Guid? EmployeeId { get; set; }

        [RegularExpression(@"^\d+$", ErrorMessage = "Visa Number must contain only numbers. e.g 2201012121")]
        [Display(Name = "Visa Number")]
        [Required]
        public string VisaNumber { get; set; }

        [Display(Name = "Expiry Date")]
        [Required]
        public string ExpiryDate { get; set; }

        [Display(Name = "Employee")]
        public string Employee { get; internal set; }
    }
}