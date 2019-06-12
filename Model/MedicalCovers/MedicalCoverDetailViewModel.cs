using System;
using System.ComponentModel.DataAnnotations;

namespace HRCentral.Web.Models.MedicalCovers
{
    public class MedicalCoverDetailViewModel
    {
        [Required]
        public Guid Id { get; set; }

        [Required(ErrorMessage = "Medical cover type is required.")]
        [Display(Name = "Medical Title")]
        public string Title { get; set; }
    }
}