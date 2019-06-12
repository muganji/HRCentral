using System;
using System.ComponentModel.DataAnnotations;

namespace HRCentral.Web.Models.LeaveDays
{
    public class LeaveDayDetailViewModel
    {

        public Guid Id { get; set; }

        [Required]
        public int? Days { get; set; }

        //[StringLength(25, MinimumLength = 3, ErrorMessage = "Bank name must be atleast 4 characters long.")]
        [Display(Name = "Leave Title")]
        [Required]
        public string Title { get; set; }
    }
}
