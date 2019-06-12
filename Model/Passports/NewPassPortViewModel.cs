using System;
using System.ComponentModel.DataAnnotations;

namespace HRCentral.Web.Models.Passports
{
    public class NewPassPortViewModel
    {
        [Display(Name = "Employee")]
        [Required(ErrorMessage = "Employee Id is required")]
        public Guid? EmployeeId { get; set; }

        [RegularExpression(@"^[A-Z]\d+$", ErrorMessage = "Passport Number must be of this format. e.g B5655777")]
        [Display(Name = "Passport Number")]
        public string PassportNumber { get; set; }

        [Display(Name = "Expiry Date")]
        [Required]
        public string ExpiryDate { get; set; }
    }
}