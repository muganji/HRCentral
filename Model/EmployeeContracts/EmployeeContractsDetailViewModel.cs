using System;
using System.ComponentModel.DataAnnotations;

namespace HRCentral.Web.Models.EmployeeContracts
{
    public class EmployeeContractsDetailViewModel
    {
        [Required]
        public Guid Id { get; set; }

        public float BasicPay { get; set; }

        [Required(ErrorMessage = "Employee is required.")]
        [Display(Name = "Employee")]
        public Guid? EmployeeId { get; set; }


        [Required(ErrorMessage = "Contract start date is required.")]
        [Display(Name = "Start Date")]
        public string StartDate { get; set; }

        [Required(ErrorMessage = "Contract termination date is required.")]
        [Display(Name = "End Date")]
        public string TerminationDate { get; set; }
        [Display(Name = "Status")]
        public string ContractStatus { get; set; }
        public string TerminationReason { get; set; }

        [Required(ErrorMessage = "Contract period is required.")]
        [Display(Name = "Contract Period")]
        public Guid? ContactID { get; set; }

        [Required(ErrorMessage = "WebAccess is required.")]
        public bool? WebAccess { get; set; }

        [Required(ErrorMessage = "Mail is required.")]
        public bool? Mail { get; set; }

        [Required(ErrorMessage = "Internet is required.")]
        public bool? Internet { get; set; }

        [Required(ErrorMessage = "DailUpAccess is required.")]
        public bool? DailUpAccess { get; set; }

        [Required(ErrorMessage = "Ability is required.")]
        public bool? Ability { get; set; }

        [Required(ErrorMessage = "Finance is required.")]
        public bool? Finance { get; set; }

        [Required(ErrorMessage = "CDRInterConnection is required.")]
        public bool? CDRInterConnection { get; set; }

        public string AirtimeAllocation { get; set; }

        [Required(ErrorMessage = "Medical cover is required.")]
        [Display(Name = "Medical Cover")]
        public Guid? MedicalCoverId { get; set; }
        public string Duration { get; set; }
    }
}