﻿using System;
using System.ComponentModel.DataAnnotations;

namespace HRCentral.Web.Models.Departments
{
    public class DepartmentDetailViewModel
    {
        [Required]
        public Guid Id { get; set; }

        [Required(ErrorMessage = "Department title is required.")]
        //[RegularExpression(@"^[a-zA-Z\\-\\_\s\&]*$")]
        //[StringLength(25, ErrorMessage = "Department title must be atmost 25 characters long.")]
        [Display(Name = "Department Name")]
        public string Title { get; set; }
    }
}