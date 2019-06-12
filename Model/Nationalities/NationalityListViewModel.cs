using System;
using System.ComponentModel.DataAnnotations;

namespace HRCentral.Web.Models.Nationalities
{
    public class NationalityListViewModel
    {
        public Guid Id { get; set; }

        [Display(Name = "Nationality")]
        public string Description { get; set; }
        public string DateAdded { get; set; }
        public string DateModified { get; set; }
        public string CreatedBy { get; set; }
    }
}