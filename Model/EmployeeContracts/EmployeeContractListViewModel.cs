using System;
using System.ComponentModel.DataAnnotations;

namespace HRCentral.Web.Models.EmployeeContracts
{
    public class EmployeeContractListViewModel
    {
        public Guid Id { get; set; }
        public float BasicPay { get; set; }

        [Display(Name = "Employee")]
        public Guid? EmployeeId { get; set; }

        [Display(Name = "Start Date")]
        public string StartDate { get; set; }

        [Display(Name = "End Date")]
        public string TerminationDate { get; set; }

        public string TerminationReason { get; set; }

        [Display(Name = "Contract Period")]
        public Guid? ContactID { get; set; }

        public bool? WebAccess { get; set; }
        public string ContractStatus { get; set; }
        public bool? Mail { get; set; }

        public bool? Internet { get; set; }

        public bool? DailUpAccess { get; set; }

        public bool? Ability { get; set; }

        public bool? Finance { get; set; }

        public bool? CDRInterConnection { get; set; }

        public string AirtimeAllocation { get; set; }

        [Display(Name = "Medical Cover")]
        public Guid? MedicalCoverId { get; set; }

        public string Employee { get; set; }
        public string Duration { get; set; }
    }
}