using System;
using System.ComponentModel.DataAnnotations;

namespace HRCentral.Web.Models.Departments
{
    public class DepartmentListViewModel
    {
        public Guid Id { get; set; }
        [Display(Name ="Department Name")]
        public string Title { get; set; }
        public string DateAdded { get; set; }
        public string DateModified { get; set; }
        public string CreatedBy { get; set; }
    }
}
