using System;
using System.ComponentModel.DataAnnotations;

namespace HRCentral.Web.Models.Crimes
{
    public class CrimeDetailViewModel
    {

        [Required]
        public Guid Id { get; set; }
        [Required]
        public Guid? Employee { get; set; }
        [Required]
        public bool IsConvicted { get; set; }
        [Required]
        public string CrimeDescription { get; set; }
        [Display(Name = "Crime Date")]
        [Required]
        public string ConvictionDate { get; set; }
        [Required]
        public string ConvictionPlace { get; set; }
        [Required]
        public string SentenceImposed { get; set; }
    }
}
