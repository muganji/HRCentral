using System;
using System.ComponentModel.DataAnnotations;

namespace HRCentral.Web.Models.EmployeeParents
{
    public class EmployeeParentsDetailViewModel
    {
        [Required]
        public Guid Id { get; set; }

        [Required(ErrorMessage = "Employee is required.")]
        [Display(Name = "Employee")]
        public Guid? EmployeeId { get; set; }


        [Required(ErrorMessage = "Parents contact info is required.")]
        public string ContactInfo { get; set; }

        //[Required(ErrorMessage = "Parents residence is required.")]
        [Display(Name = "Home Address")]
        public string Residence { get; set; }


        [Required(ErrorMessage = "Parents name is required.")]
        [Display(Name = "Parents Name")]
        public string Name { get; set; }

        [Required(ErrorMessage = "Parent alive status is required.")]
        [Display(Name = "Alive Status")]
        public bool Alive { get; set; }

        [Required(ErrorMessage = "Residence District is required.")]
        [Display(Name = "Residence District")]
        public Guid? DistrictResidenceId { get; set; }
        [Required(ErrorMessage = "Nationality is required.")]
        [Display(Name = "Nationality")]
        public Guid? NationalityId { get; set; }
        [Required(ErrorMessage = "Home District is required.")]
        [Display(Name = "Home District")]
        public Guid? HomeDistrictId { get; set; }

    }
}