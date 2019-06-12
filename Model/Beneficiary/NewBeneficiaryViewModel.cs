using System;
using System.ComponentModel.DataAnnotations;

namespace HRCentral.Web.Models.Beneficiary
{
    public class NewBeneficiaryViewModel
    {
        [Display(Name = "Employee")]
        [Required(ErrorMessage = "Employee is required")]
        public Guid EmployeeID { get; set; }
        [Display(Name = "Beneficiary Names")]
        [RegularExpression(@"^[a-zA-Z\\-\\_\s]*$", ErrorMessage = "Name should be of this format 'Julius Muganji'.")]
        public string BeneficiaryName { get; set; }
        [Required]
        [Display(Name = "Beneficiary Contact")]
        public string BeneficiaryContact { get; set; }
        [Display(Name = "Relationship")]
       [Required(ErrorMessage = "Relationship is required")]
        public Guid? RelationshipId { get; set; }

    }
}
