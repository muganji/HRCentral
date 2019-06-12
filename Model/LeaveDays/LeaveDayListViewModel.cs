using System;
using System.ComponentModel.DataAnnotations;

namespace HRCentral.Web.Models.LeaveDays
{
    public class LeaveDayListViewModel
    {
        public Guid Id { get; set; }


        public int? Days { get; set; }

        //[StringLength(25, MinimumLength = 3, ErrorMessage = "Bank name must be atleast 4 characters long.")]
        [Display(Name = "Leave Title")]
 
        public string Title { get; set; }
        public string DateAdded { get; set; }
        public string DateModified { get; set; }
        public string CreatedBy { get; set; }
    }
}
