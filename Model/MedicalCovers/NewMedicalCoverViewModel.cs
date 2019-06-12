using System.ComponentModel.DataAnnotations;

namespace HRCentral.Web.Models.MedicalCovers
{
    public class NewMedicalCoverViewModel
    {
        [Required(ErrorMessage = "Medical cover type is required.")]
        [Display(Name = "Medical Title")]
        public string Title { get; set; }
    }
}