using System.ComponentModel.DataAnnotations;

namespace HRCentral.Web.Models.Nationalities
{
    public class NewNationalityViewModel
    {
        //[RegularExpression(@"^[a-zA-Z]*$", ErrorMessage = "Nationality must contain Characters only.")]
        [Display(Name = "Nationality")]
        [Required]
        //[StringLength(25, MinimumLength = 3, ErrorMessage = "Nationality must be atleast 3 characters long.")]
        public string Description { get; set; }
    }
}