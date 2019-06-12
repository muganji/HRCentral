using System;
using System.ComponentModel.DataAnnotations;

namespace HRCentral.Web.Models.Languages
{
    public class LanguageDetailViewModel
    {
        [Required]
        public Guid Id { get; set; }

        [Display(Name = "Employee")]
        [Required(ErrorMessage = "Employee is required")]
        public Guid? EmployeeId { get; set; }

        [RegularExpression(@"^[a-zA-Z]*$", ErrorMessage = "Written proficiency must contontainin characters only.")]
        [Display(Name = "Written")]
        [Required]
        [StringLength(25, ErrorMessage = "Language must be atleast 6 characters long.")]
        public string WrittenProficiency { get; set; }

        [RegularExpression(@"^[a-zA-Z]*$", ErrorMessage = "Speech proficiency must contain characters only.")]
        [Display(Name = "Speech")]
        [Required]
        [StringLength(25, ErrorMessage = "Language must be atleast 6 characters long.")]
        public string SpeechProficiency { get; set; }

        [RegularExpression(@"^[a-zA-Z]*$", ErrorMessage = "ReadProficiency must contain characters only.")]
        [Display(Name = "Read")]
        [Required]
        [StringLength(25, ErrorMessage = "Language must be atleast 6 characters long.")]
        public string ReadProficiency { get; set; }
    }
}