using System;
using System.ComponentModel.DataAnnotations;

namespace HRCentral.Web.Models.Children
{
    public class ChildrenListViewModel
    {

        public Guid Id { get; set; }

        [Display(Name = "Employee")]

        public Guid? EmployeeId { get; set; }

        [Display(Name = "Employee")]
        public string Employee { get; set; }

        [Display(Name = "Child Name")]
        public string Name { get; set; }

        public string OtherGender { get; set; }

        public string BirthOrder { get; set; }

        public string Gender { get; set; }

        [Display(Name = "BirthOrder")]
        
        public Guid? BirthOrderId { get; set; }

        [Display(Name = "Birth Date")]

        public string BirthDate { get; set; }
    }
}
