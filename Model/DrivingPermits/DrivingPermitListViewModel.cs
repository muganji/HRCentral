using System;
using System.ComponentModel.DataAnnotations;

namespace HRCentral.Web.Models.DrivingPermits
{
    public class DrivingPermitListViewModel
    {
        public Guid Id { get; set; }

        [Display(Name = "Employee")]
        public Guid? EmployeeId { get; set; }

        [Display(Name = "Permit Number")]
        public string DrivingPermitNumber { get; set; }

        [Display(Name = "Expiry Date")]
        public string ExpiryDate { get; set; }

        [Display(Name = "Employee")]
        public string Employee { get; set; }
    }
}