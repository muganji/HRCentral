using System;
using System.ComponentModel.DataAnnotations;

namespace HRCentral.Web.Models.Banks
{
    public class BankListViewModel
    {
        public Guid Id { get; set; }

        [Display(Name = "Bank")]
        public string Name { get; set; }

        public string DateAdded { get; set; }
        public string DateModified { get; set; }
        public string CreatedBy { get; set; }
    }
}