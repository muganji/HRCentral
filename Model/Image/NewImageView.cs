using System;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace HRCentral.Web.Models.Image
{
    public class NewImageView
    {
        [Required]
        public IFormFile Img { get; set; }

        [Required]
        public Guid EmployeeID { get; set; }
    }
}
