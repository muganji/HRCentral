using System;
using System.ComponentModel.DataAnnotations;

namespace HRCentral.Web.Models.RelationshipTypes
{
    public class RelationshipTypeListViewModel
    {
        public Guid Id { get; set; }

        [Display(Name = "Relationship Name")]
        public string Title { get; set; }
        public string DateAdded { get; set; }
        public string DateModified { get; set; }
        public string CreatedBy { get; set; }
    }
}