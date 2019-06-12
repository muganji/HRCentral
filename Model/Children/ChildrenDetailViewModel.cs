using System;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;


namespace HRCentral.Web.Models.Children
{
    public class ChildrenDetailViewModel
    {
        [Required]
        public Guid Id { get; set; }

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

        //[Required]
        //public IFormFile BirthCertificateImg { get; set; }

        //[Display(Name = "Face image")]
        //public string BirthCertificateImage { get; set; }
    }
}
