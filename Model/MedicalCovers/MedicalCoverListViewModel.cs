using System;
using System.ComponentModel.DataAnnotations;

namespace HRCentral.Web.Models.MedicalCovers
{
    public class MedicalCoverListViewModel
    {
        public Guid Id { get; set; }

        [Display(Name = "Medical Title")]
        public string Title { get; set; }
        public string DateAdded { get; set; }
        public string DateModified { get; set; }
        public string CreatedBy { get; set; }
    }
}