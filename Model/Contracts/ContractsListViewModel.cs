using System;
using System.ComponentModel.DataAnnotations;

namespace HRCentral.Web.Models.Contracts
{
    public class ContractsListViewModel
    {
        public Guid Id { get; set; }

        [Display(Name = "Contract Type")]
        public string Title { get; set; }

        [Display(Name = "ContractPeriod")]
        public int Months { get; set; }
        public string DateAdded { get; set; }
        public string DateModified { get; set; }
        public string CreatedBy { get; set; }
    }
}