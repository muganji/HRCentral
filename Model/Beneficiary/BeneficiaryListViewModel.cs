using System;
using System.ComponentModel.DataAnnotations;

namespace HRCentral.Web.Models.Beneficiary
{
    public class BeneficiaryListViewModel
    {
   
        public Guid Id { get; set; }

        [Display(Name = "Employee")]

        public Guid? EmployeeId { get; set; }


        [Display(Name = "Employee")]
        public string Employee { get; set; }

        [Display(Name = "Beneficiary Name")]
        public string BeneficiaryName { get; set; }
      
        [Display(Name = "Beneficiary Contact")]
        public string BeneficiaryContact { get; set; }
        [Display(Name = "Relationship")]
      
        public Guid? RelationshipId { get; set; }

        public string RelationShip { get; set; }
    }
}
