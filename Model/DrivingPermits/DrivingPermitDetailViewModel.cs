using System;
using System.ComponentModel.DataAnnotations;

namespace HRCentral.Web.Models.DrivingPermits
{
    public class DrivingPermitDetailViewModel
    {
        [Required]
        public Guid Id { get; set; }

        [Display(Name = "Employee")]
        [Required(ErrorMessage = "Employee Id is required")]
        public Guid? EmployeeId { get; set; }

        [RegularExpression(@"^\d+\/\d{1}\/\d{1}$", ErrorMessage = "Driving Permit Number must contain only numbers. e.g 34567867/7/9")]
        [Display(Name = "Driving Permit")]
        [StringLength(15, MinimumLength = 10, ErrorMessage = "Driving Permit Number must be atleast 10 characters long.")]
        [Required]
        public string DrivingPermitNumber { get; set; }

        [Required]
        [Display(Name = "ExpiryDate")]
        public string ExpiryDate { get; set; }
    }
}