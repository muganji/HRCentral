using System.ComponentModel.DataAnnotations;

namespace HRCentral.Web.Models.Contracts
{
    public class NewContractsViewModel
    {
        
        [Display(Name = "Duration Name")]
        [Required(ErrorMessage = "Contract Title is required")]
        public string Title { get; set; }

        
        [Required(ErrorMessage = "Contract Months is required.")]
        [Display(Name = "Contract Period")]
        public int Months { get; set; }
    }
}