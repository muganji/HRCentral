using System;
using System.ComponentModel.DataAnnotations;
namespace HRCentral.Web.Models.Email
{
    public class EmailListViewModel
    {
        
        public Guid Id { get; set; }
        [Display(Name = "Employee")]
        
        public Guid? EmployeeId { get; set; }

        [Display(Name = "Employee")]
        public string Employee { get; set; }

        [Display(Name = "Work Email")]
      
        public string EmailAddress { get; set; }
        public string DateAdded { get; set; }
        public string DateModified { get; set; }
        public string CreatedBy { get; set; }
    }
}
