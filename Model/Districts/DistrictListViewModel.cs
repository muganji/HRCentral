using System;
using System.ComponentModel.DataAnnotations;

namespace HRCentral.Web.Models.Districts
{
    public class DistrictListViewModel
    {
        public Guid Id { get; set; }

        [Display(Name = "District")]
        public string Name { get; set; }
        public string DateAdded { get; set; }
        public string DateModified { get; set; }
        public string CreatedBy { get; set; }
    }
}