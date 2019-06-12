using System;
using System.ComponentModel.DataAnnotations;

namespace HRCentral.Web.Models.Visas
{
    public class VisaListViewModel
    {
        public Guid Id { get; set; }

        [Display(Name = "Employee Id")]
        public Guid? EmployeeId { get; set; }

        [Display(Name = "Visa Number")]
        public string VisaNumber { get; set; }

        [Display(Name = "Expiry Date")]
        public string ExpiryDate { get; set; }

        [Display(Name = "Employee")]
        public string Employee { get; set; }
    }
}