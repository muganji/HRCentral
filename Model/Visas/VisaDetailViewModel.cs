using System;
using System.ComponentModel.DataAnnotations;

namespace HRCentral.Web.Models.Visas
{
    public class VisaDetailViewModel
    {
        public Guid Id { get; set; }

        [Display(Name = "Employee")]
        [Required(ErrorMessage = "Employee Id is required")]
        public Guid? EmployeeId { get; set; }

        [Required]
        [Display(Name = "VisaNumber")]
        public string VisaNumber { get; set; }

        [Required]
        [Display(Name = "ExpiryDate")]
        public string ExpiryDate { get; set; }
    }
}