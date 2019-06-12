using System;
using System.ComponentModel.DataAnnotations;

namespace HRCentral.Web.Models.Languages
{
    public class LanguageListViewModel
    {
        public Guid Id { get; set; }

        [Display(Name = "Employee Id")]
        public Guid? EmployeeId { get; set; }

        [Display(Name = "Written")]
        public string WrittenProficiency { get; set; }

        [Display(Name = "Spoken")]
        public string SpeechProficiency { get; set; }

        [Display(Name = "Read")]
        public string ReadProficiency { get; set; }

        [Display(Name = "Employee")]
        public string EmployeeName { get; set; }
    }
}