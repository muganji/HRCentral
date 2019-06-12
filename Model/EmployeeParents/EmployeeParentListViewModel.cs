using System;
using System.ComponentModel.DataAnnotations;

namespace HRCentral.Web.Models.EmployeeParents
{
    public class EmployeeParentListViewModel
    {
        public Guid Id { get; set; }

        [Display(Name = "Employee")]
        public string Employee { get; set; }

        [Display(Name = "Home Address")]
        public string Address { get; set; }

        public string ContactInfo { get; set; }

        [Display(Name = "Home District")]
        public string HomeDistrict { get; set; }

        [Display(Name = "Nationality")]
        public string Nationality { get; set; }

        [Display(Name = "Residence District")]
        public string DistrictResidence { get; set; }

        [Display(Name = "Parents Name")]
        public string Name { get; set; }

        [Display(Name = "Alive Status")]
        public bool Alive { get; set; }
    }
}