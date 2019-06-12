using System;
using System.ComponentModel.DataAnnotations;

namespace HRCentral.Web.Models.EmploymentRecords
{
    public class EmploymentRecordDetailViewModel
    {
        [Required]
        public Guid Id { get; set; }

        [Display(Name = "Employee")]
        [Required]
        public Guid? EmployeeID { get; set; }

        [Required]
        public string CompanyName { get; set; }

        [Required]
        public string Duration { get; set; }

        [Required]
        public string PositionHeld { get; set; }
    }
}
