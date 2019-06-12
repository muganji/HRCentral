using System;
using System.ComponentModel.DataAnnotations;

namespace HRCentral.Web.Models.Passports
{
    public class PassportDetailViewModel
    {
        public Guid Id { get; set; }

        [Display(Name = "Employee")]
        [Required(ErrorMessage = "Employee Id is required")]
        public Guid? EmployeeId { get; set; }

        [RegularExpression(@"^[A-Z]\d+$", ErrorMessage = "Passport Number must contain only numbers. e.g B5655777")]
        [Display(Name = "Passport Number")]
        public string PassportNumber { get; set; }

        [Required]
        public string ExpiryDate { get; set; }
    }
}