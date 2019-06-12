using System;
using System.ComponentModel.DataAnnotations;

namespace HRCentral.Web.Models.EmployeeLeave
{
    public class EmployeeLeaveListViewModel
    {
        public Guid? Id { get; set; }

     
        [Display(Name = "Employee")]
        public Guid? EmployeeId { get; set; }
    
        [Display(Name = "Leave Type")]
        public Guid? LeaveTypeId { get; set; }
        public string StartDate { get; set; }
        public string EndDate { get; set; }
        
        public string Status { get; set; }
        public int DaystoTake { get; set; }
       
        [Display(Name = "Sitting In")]
        public Guid? ReplacementId { get; set; }
        public string RejectReason { get; set; }
        public string Employee { get; set; }
    }
}
