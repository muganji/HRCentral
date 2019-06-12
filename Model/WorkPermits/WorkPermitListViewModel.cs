using System;
using System.ComponentModel.DataAnnotations;

namespace HRCentral.Web.Models.WorkPermits
{
    public class WorkPermitListViewModel
    {
        public Guid Id { get; set; }

        [Display(Name = "Employee Id")]
        public Guid? EmployeeId { get; set; }

        [Display(Name = "Work Permit Number")]
        public string WorkPermitNumber { get; set; }

        [Display(Name = "Expiry Date")]
        public string ExpiryDate { get; set; }

        [Display(Name = "Employee")]
        public string Employee { get; set; }
    }
}