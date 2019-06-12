using System;
using System.ComponentModel.DataAnnotations;

namespace HRCentral.Web.Models.Contracts
{
    public class ContractsDetailViewModel
    {
        [Required]
        public Guid Id { get; set; }

        //[RegularExpression(@"^[0-9{2}\\-\\_\s][a-zA-Z]*$", ErrorMessage = "Contract title only  supports this format. e.g '02 months'")]
        //[StringLength(25, MinimumLength = 9, ErrorMessage = "Contract title must be atleast 4 characters long.")]
        [Display(Name = "Duration Name")]
        [Required(ErrorMessage = "Contract Title is required")]
        public string Title { get; set; }

        //[RegularExpression(@"^[0-9{2}$", ErrorMessage = "Contract month only  supports this format. e.g '02'")]
        [Required(ErrorMessage = "Contract Months is required.")]
        [Display(Name = "Contract Period")]
        public int Months { get; set; }
    }
}