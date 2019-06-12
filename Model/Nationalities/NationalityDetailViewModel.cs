using System;
using System.ComponentModel.DataAnnotations;

namespace HRCentral.Web.Models.Nationalities
{
    public class NationalityDetailViewModel
    {
        public Guid Id { get; set; }

        //[RegularExpression(@"^[a-zA-Z]*$", ErrorMessage = "Nationality must contain characters only.")]
        [Required]
        //[StringLength(25, MinimumLength = 3, ErrorMessage = "Nationality must be atleast 3 characters long.")]
        public string Description { get; set; }
    }
}