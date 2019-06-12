using System;
using System.ComponentModel.DataAnnotations;

namespace HRCentral.Web.Models.NextOfKins
{
    public class NextOfKinListViewModel
    {
       
        public Guid Id { get; set; }

       
        public Guid? EmployeeId { get; set; }

        [Display(Name = "Employee")]
        public string Employee { get; set; }

        //public string Address { get; set; }

        
        public string ContactInfo { get; set; }

        //[Display(Name = "Home Address")]
       // public string Residence { get; set; }

        [Display(Name = "Relationship")]
        public Guid? RelationshipId { get; set; }

        [Display(Name = "NextofKin Name")]
        public string Name { get; set; }

        public string RelationShip { get; set; }
    }
}