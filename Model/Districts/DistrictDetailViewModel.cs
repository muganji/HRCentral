using System;
using System.ComponentModel.DataAnnotations;

namespace HRCentral.Web.Models.Districts
{
    public class DistrictDetailViewModel
    {
        [Required]
        public Guid Id { get; set; }

        //[RegularExpression(@"^[a-zA-Z]*$", ErrorMessage = "District Name must contains Characters.")]
        [Display(Name = "District")]
        //[StringLength(25, MinimumLength = 4, ErrorMessage = "District name must be atleast 4 characters long.")]
        [Required]
        public string Name { get; set; }
    }
}