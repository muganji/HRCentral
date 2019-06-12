using System;
using System.ComponentModel.DataAnnotations;

namespace HRCentral.Web.Models.Passports
{
    public class PassPortListViewModel
    {
        public Guid Id { get; set; }

        [Display(Name = "Employee Id")]
        public Guid? EmployeeId { get; set; }

        [Display(Name = "Passport Number")]
        public string PassportNumber { get; set; }

        [Display(Name = "Expiry Date")]
        public string ExpiryDate { get; set; }

        [Display(Name = "Employee")]
        public string Employee { get; set; }
    }
}