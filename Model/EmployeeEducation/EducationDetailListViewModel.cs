using System;
using System.ComponentModel.DataAnnotations;

namespace HRCentral.Web.Models.EducationDetails
{
    public class EducationDetailListViewModel
    {
        public Guid Id { get; set; }

        [Display(Name = "Employee")]
        
        public Guid? EmployeeId { get; set; }

        [Display(Name = "Institution")]
        
        public string Institution { get; set; }

     
        public string QualificationTitle { get; set; }


        [Display(Name = "From")]

        public string Begin { get; set; }

        [Display(Name = "To")]

        public string End { get; set; }


  
        public bool Status { get; set; }


        [Display(Name = "Qualification")]

        public Guid? QualificationId { get; set; }


        public string Qualification { get; set; }

        public string Employee { get; set; }
    }
}