using System;
using System.ComponentModel.DataAnnotations;

namespace HRCentral.Web.Models.EmploymentRecords
{
    public class EmploymentRecordListViewModel
    {
 
        public Guid? Id { get; set; }

        [Display(Name = "Employee")]

        public Guid? EmployeeID { get; set; }

        public string CompanyName { get; set; }

        public string Duration { get; set; }
        [Display(Name = "Employee")]
        public string Employee { get; set; }

        public string PositionHeld { get; set; }
    }
}
