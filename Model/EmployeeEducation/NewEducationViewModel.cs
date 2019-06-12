using System;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace HRCentral.Web.Models.EducationDetails
{
    public class NewEducationViewModel
    {
        [Display(Name = "Employee")]
        [Required(ErrorMessage = "Employee Id is required")]
        public Guid? EmployeeId { get; set; }

        [Display(Name = "Awarding School")]
        [Required(ErrorMessage = "Institution is required")]
        [RegularExpression(@"^[a-zA-Z\\-\\_\s]*$", ErrorMessage = "Institution contains only Characters.")]
        public string Institution { get; set; }

        [Display(Name = "From")]
        [Required(ErrorMessage = "Begin date is required")]
        public string Begin { get; set; }

        [Display(Name = "To")]
        
        [Required(ErrorMessage = "End date is required")]
        public string End { get; set; }

        
        [Required]
        [Display(Name = "IsCompleted")]
        public bool Status { get; set; }

        [Required]
        [Display(Name = "Title")]
        public string QualificationTitle { get; set; }

        [Display(Name = "Qualification")]
        [Required(ErrorMessage = "Qualification is required")]
        public Guid? QualificationId { get; set; }

        //[Required]
        //public IFormFile DocumentImg { get; set; }


    }
}