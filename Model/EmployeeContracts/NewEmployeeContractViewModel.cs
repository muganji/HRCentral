using System;
using System.ComponentModel.DataAnnotations;

namespace HRCentral.Web.Models.EmployeeContracts
{
    public class NewEmployeeContractViewModel
    {
        public float BasicPay { get; set; }

        [Required(ErrorMessage = "Employee is required.")]
        [Display(Name = "Employee")]
        public Guid? EmployeeId { get; set; }

        [Required(ErrorMessage = "Contract start date is required.")]
        [Display(Name = "Start Date")]
        public string StartDate { get; set; }

        //[Required(ErrorMessage = "Contract termination date is required.")]
        //[Display(Name = "End Date")]
        //public string TerminationDate { get; set; }

        public string TerminationReason { get; set; }

        [Required(ErrorMessage = "Contract period is required.")]
        [Display(Name = "Contract Period")]
        public Guid? ContactID { get; set; }

        [Required(ErrorMessage = "WebAccess is required.")]
        [Display(Name = "HasWebAccess")]
        public bool? WebAccess { get; set; }

        [Required(ErrorMessage = "Mail is required.")]
        [Display(Name = "HasMail")]
        public bool? Mail { get; set; }

        [Required(ErrorMessage = "Internet is required.")]
        [Display(Name = "HasInternet")]
        public bool? Internet { get; set; }

        [Required(ErrorMessage = "DailUpAccess is required.")]
        [Display(Name = "HasDailUp")]
        public bool? DailUpAccess { get; set; }

        [Required(ErrorMessage = "Ability is required.")]
        [Display(Name = "HasAbility")]
        public bool? Ability { get; set; }

        [Required(ErrorMessage = "Finance is required.")]
        [Display(Name = "IsFinance")]
        public bool? Finance { get; set; }
        [Required(ErrorMessage = "Contract Status.")]
        [Display(Name = "Status")]
        public string ContractStatus { get; set; }

        [Required(ErrorMessage = "CDRInterConnection is required.")]
        [Display(Name = "HasCDR")]
        public bool? CDRInterConnection { get; set; }

        public string AirtimeAllocation { get; set; }

        [Required(ErrorMessage = "Medical cover is required.")]
        [Display(Name = "Medical Cover")]
        public Guid? MedicalCoverId { get; set; }
        public string Duration { get; set; }
    }
}