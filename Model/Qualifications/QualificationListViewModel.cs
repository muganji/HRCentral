using System;
using System.ComponentModel.DataAnnotations;

namespace HRCentral.Web.Models.Qualifications
{
    public class QualificationListViewModel
    {
        public Guid Id { get; set; }

        [Display(Name = "Qualification")]
        public string Title { get; set; }
        public string DateAdded { get; set; }
        public string DateModified { get; set; }
        public string CreatedBy { get; set; }
    }
}