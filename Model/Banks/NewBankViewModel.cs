using System.ComponentModel.DataAnnotations;

namespace HRCentral.Web.Models.Banks
{
    public class NewBankViewModel
    {
        [RegularExpression(@"^[a-zA-Z\\-\\_\s\&]*$", ErrorMessage = "Bank Name contains only  Characters and symbols. e.g DFCU")]
        [StringLength(25, MinimumLength = 3, ErrorMessage = "Bank name must be atleast 4 characters long.")]
        [Display(Name = "Bank Name")]
        [Required]
        public string Name { get; set; }
    }
}