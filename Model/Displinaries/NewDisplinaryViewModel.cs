using System;
using System.ComponentModel.DataAnnotations;

namespace HRCentral.Web.Models.Displinaries
{
    public class NewDisplinaryViewModel
    {
        // Crime Section
        [Required]
        public Guid? Employee { get; set; }
        [Required]
        [Display(Name = "IsGuilty")]
        public bool IsConvicted { get; set; }
        [Required]
        [Display(Name = "MisConduct Description")]
        public string CrimeDescription { get; set; }
        [Display(Name = "MisConduct Date")]
        [Required]
        public string ConvictionDate { get; set; }
        [Required]
        [Display(Name = "MisConduct Place")]
        public string ConvictionPlace { get; set; }
        [Required]
        [Display(Name = "MisConduct Punishment")]
        public string SentenceImposed { get; set; }
        // The End
    }
}
