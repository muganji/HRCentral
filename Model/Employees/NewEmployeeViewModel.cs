using System;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace HRCentral.Web.Models.Employees
{
    public class NewEmployeeViewModel
    {
        [RegularExpression(@"^\d+$", ErrorMessage = "Personal File Number must be only numbers.")]
        [Display(Name = "PF Number")]
        [Required]
        [StringLength(25, MinimumLength = 4, ErrorMessage = "PF Number must be atleast 4 characters long.")]
        public string PFNumber { get; set; }
        [Required]
        [Display(Name = "Title")]
        public string Salutation { get; set; }
        [Display(Name = "Given Name")]
        [Required]
        [StringLength(25, MinimumLength = 3, ErrorMessage = "Given name must be atleast 3 characters long.")]
        [RegularExpression(@"^[a-zA-Z\\-\\_\s\&]*$", ErrorMessage = "FirstName must be only Characters.")]
        public string FirstName { get; set; }
        [RegularExpression(@"^[a-zA-Z\\-\\_\s\&]*$", ErrorMessage = "Maiden Name must be only Characters.")]
        [StringLength(25, MinimumLength = 3, ErrorMessage = "Maiden Name must be atleast 3 characters long.")]
        public string MiddleName { get; set; }
        [Display(Name = "Maiden Name")]
        [RegularExpression(@"^[a-zA-Z\\-\\_\s\&]*$", ErrorMessage = "Middle Name must be only Characters.")]
        [StringLength(25, MinimumLength = 3, ErrorMessage = "Middle Name must be atleast 3 characters long.")]
        public string MaidenName { get; set; }
        [Display(Name = "SurName")]
        [Required]
        [RegularExpression(@"^[a-zA-Z\\-\\_\s\&]*$", ErrorMessage = "Family name must be only Characters.")]
        [StringLength(25, MinimumLength = 3, ErrorMessage = "Family name must be atleast 3 characters long.")]
        public string LastName { get; set; }
        [Display(Name = "Gender")]
        [Required(ErrorMessage = "Gender is required")]
        public string Gender { get; set; }
        [Display(Name = "Nationality")]
        [Required(ErrorMessage = "Nationality is required")]
        public Guid? NationalityId { get; set; }
        [Display(Name = "Religion")]
        [Required(ErrorMessage = "Religion is required")]
        public string Religion { get; set; }
        [Display(Name = "Marital Status")]
        [Required(ErrorMessage = "Marital Status is required")]
        public string MaritalStatus { get; set; }
        //[RegularExpression(@"^[a-zA-Z\\-\\_\s]*$")]
        [Required(ErrorMessage = "Home Address is required")]
        public string Address { get; set; }
        [Required]
        [RegularExpression(@"^[a-zA-Z\\-\\_\s\&]*$", ErrorMessage = "Job title must be only Characters.")]
        [StringLength(256, ErrorMessage = "Job title must be atmost 256 characters long.")]
        public string JobTitle { get; set; }
        [Required]
        [Display(Name ="Name")]
        [RegularExpression(@"^[a-zA-Z\\-\\_\s]*$", ErrorMessage = "Contact person name must be only Characters.")]
        [StringLength(256, ErrorMessage = "Contact person name must be atmost 256 characters long.")]
        public string ContactPerson { get; set; }
        [Required]
        [Display(Name ="Phone")]
        [RegularExpression(@"^07[0-9]{8}$", ErrorMessage = "Phone number must be of a standard format e.g +256712126547.")]
        public string ContactPersonTelephone { get; set; }

        [Display(Name = "TIN Number")]
        [RegularExpression(@"^\d{10}$", ErrorMessage = "TIN Number must be strictly 10 digits")]
        public string TINNumber { get; set; }

        [Display(Name = "NSSF Number")]
        [RegularExpression(@"^\d{13}$", ErrorMessage = "NSSF Number must be strictly 13 digits")]
        public string NSSFNumber { get; set; }
        [Display(Name = "NIN Number")]
        [RegularExpression(@"^C[a-zA-Z]\d{7}[a-zA-Z\d]*$", ErrorMessage = "NIN Number must be of a standard format.")]
        [StringLength(14, ErrorMessage = "NIN must be atmost 14 characters long.")]
        public string NINNumber { get; set; }
        public string SpouseName { get; set; }
        [Display(Name = "Spouse Address")]
        public string SpouseContactAddress { get; set; }
        [Display(Name = "Spouse BirthDate")]
        public string SpouseDateOfBirth { get; set; }
        public string SpouseTelephone { get; set; }
        [Required]
        public bool IsActive { get; set; }
        [Display(Name = "BirthDate")]
        [Required]
        public string DateOfBirth { get; set; }
        [Display(Name = "Join Date")]
        [Required]
        public string DateOfJoin { get; set; }
        [Display(Name = "Birth District")]
        [Required(ErrorMessage = "Birth District is required")]
        public Guid? DistrictBirthId { get; set; }
        [Display(Name = "Line Manager")]
        //[Required(ErrorMessage ="Line Manager is required")]
        public Guid? SupervisorId { get; set; }
        [Display(Name = "Residence District")]
        [Required(ErrorMessage = "Residence District is required")]
        public Guid? DistrictResidenceId { get; set; }
        [Required]
        [Display(Name = "Department")]
        public Guid? DepartmentId { get; set; }
        [Required]
        [Display(Name = "Bank Name")]
        public Guid? BankId { get; set; }
        [Required]
        [Display(Name = "Branch")]
        [StringLength(25, MinimumLength = 3, ErrorMessage = "Branch name must be atleast 3 characters long.")]
        public string BankBranch { get; set; }
        [Required]
        [RegularExpression(@"^\d+$", ErrorMessage = "Account Number contains only numbers.")]
        [Display(Name = "Bank Account")]
        public string BankAccountNumber { get; set; }
       // [Display(Name = "Beneficiary Names")]
       // [RegularExpression(@"^[a-zA-Z\\-\\_\s]*$", ErrorMessage = "Name should be of this format 'Julius Muganji'.")]
       // public string BeneficiaryName { get; set; }
        //[Display(Name = "Beneficiary Address")]
        //public string BeneficiaryContact { get; set; }
        //[Display(Name = "Relationship")]
        //public Guid? RelationshipId { get; set; }
        [Display(Name = "Phone Number")]
        [Required]
        [RegularExpression(@"^071[0-9]{7}$", ErrorMessage = "Utl Phone number must be of a standard format e.g 0712126547.")]
        public string PersonalMobileNumber { get; set; }
        [Display(Name = "Work PhoneNumber")]
        [RegularExpression(@"^0414333[0-9]{3}$", ErrorMessage = "Work phone number must be of a standard format e.g 0414333259.")]
        public string WorkMobileNumber { get; set; }
        [Display(Name = "Alternative Number")]
        //[Required]
        [RegularExpression(@"^07[0-9]{8}$", ErrorMessage = "Secondary phone number must contain 10 numbers and of a format 07xxxxxxxx.")]
        public string AlternativeMobileNumber { get; set; }
        public string OtherReligion { get; set; }
        public string MaritalStatusOthers { get; set; }
        [Required]
        public IFormFile Img { get; set; }




    }
}