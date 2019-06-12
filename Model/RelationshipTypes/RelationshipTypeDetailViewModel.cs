using System;
using System.ComponentModel.DataAnnotations;

namespace HRCentral.Web.Models.RelationshipTypes
{
    public class RelationshipTypeDetailViewModel
    {
        public Guid Id { get; set; }

        [Required(ErrorMessage = "Relationship title is required.")]
        [RegularExpression(@"^[a-zA-Z]*$", ErrorMessage = "Relationship title only  supports this format. e.g 'spouse'")]
        [StringLength(25, ErrorMessage = "Relationship title must be atmost 25 characters long.")]
        [Display(Name = "Relationship Name")]
        public string Title { get; set; }
    }
}