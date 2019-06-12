using System;
using System.ComponentModel.DataAnnotations;

namespace HRCentral.Web.Models.BirthOrder
{
    public class BirthOrderListViewModel
    {
        public Guid Id { get; set; }

        [Display(Name = "Birth Order")]
        public string Name { get; set; }
        public string DateAdded { get; set; }
        public string DateModified { get; set; }
        public string CreatedBy { get; set; }
    }
}
