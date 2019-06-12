using System;
using System.ComponentModel.DataAnnotations;

namespace HRCentral.Web.Models.EmployeeLeave
{
    public class EmployeeLeaveDetailViewModel
    {
        public Guid Id { get; set; }

        [Required(ErrorMessage = "Employee is required.")]
        [Display(Name = "Employee")]
        public Guid? EmployeeId { get; set; }
        [Required(ErrorMessage = "LeaveType is required.")]
        [Display(Name = "Leave Type")]
        public Guid? LeaveTypeId { get; set; }
        [Required(ErrorMessage = "Leave StartDate is required.")]

        public string StartDate { get; set; }
        [Required(ErrorMessage = "Leave EndDate is required.")]
        public string EndDate { get; set; }
        [Required(ErrorMessage = "Days to be taken is required.")]

        public int DaystoTake { get; set; }
        [Required(ErrorMessage = "Replacement is required.")]
        [Display(Name = "Sitting In")]
        public Guid? ReplacementId { get; set; }
        public string RejectReason { get; set; }
    }
}
