using System;
using System.ComponentModel.DataAnnotations;

namespace HRCentral.Web.Models.Repatriations
{
    public class RepatriationListViewModel
    {

        public Guid? Id { get; set; }

        [Display(Name = "Employee")]
        public Guid? EmployeeId { get; set; }


        [Display(Name = "Repatriation Name")]
        public string Name { get; set; }

        [Display(Name = "Relationship")]

        public Guid? RelationshipId { get; set; }

        [Display(Name = "Employee")]
        public string Employee { get; set; }

        public string Relationship { get; set; }
    }
}
