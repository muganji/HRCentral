using System.ComponentModel.DataAnnotations;

namespace HRCentral.Web.Models.Districts
{
    public class NewDistrictViewModel
    {
        //[RegularExpression(@"^[a-zA-Z]*$", ErrorMessage = "District name contains only Characters.")]
        [Display(Name = "District Name")]
        //[StringLength(25, MinimumLength = 4, ErrorMessage = "District name must be atleast 4 characters long.")]
        [Required]
        public string Name { get; set; }
    }
}