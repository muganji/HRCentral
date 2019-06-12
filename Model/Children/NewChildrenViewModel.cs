using System;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace HRCentral.Web.Models.Children
{
    public class NewChildrenViewModel
    {
        [Display(Name = "Employee")]
        [Required(ErrorMessage = "Employee is required")]
        public Guid? EmployeeId { get; set; }

        [Required]
        [Display(Name = "Child Name")]
        public string Name { get; set; }

        [Required]
        public string Gender { get; set; }


        [Display(Name = "BirthOrder")]
        [Required(ErrorMessage = "BirthOrder is required")]
        public Guid? BirthOrderId { get; set; }

        [Display(Name = "Birth Date")]
        [Required]
        public string BirthDate { get; set; }


       // [Required]
        //public IFormFile BirthImg { get; set; }

    }
}
