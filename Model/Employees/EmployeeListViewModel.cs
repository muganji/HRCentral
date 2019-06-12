using System;
using System.ComponentModel.DataAnnotations;

namespace HRCentral.Web.Models.Employees
{
    public class EmployeeListViewModel
    {
        
        public Guid Id { get; set; }

       public string BankName { get; set; }
        [Display(Name = "PF Number")]
       
        public string PersonnelFileNumber { get; set; }

       
        [Display(Name = "Title")]
        public string Salutation { get; set; }

        [Display(Name = "Givenname")]
       
        public string FirstName { get; set; }

        
        public string MiddleName { get; set; }

        [Display(Name = "Maidenname")]
        
        public string MaidenName { get; set; }

        [Display(Name = "Family Name")]
        
        public string LastName { get; set; }

        [Display(Name = "BirthDate")]

        public string DateOfBirth { get; set; }

        [Display(Name = "Gender")]

        public string Gender { get; set; }

        [Display(Name = "Nationality")]

        public Guid? NationalityId { get; set; }

        [Display(Name = "Religion")]

        public Guid? ReligionId { get; set; }

        [Display(Name = "Marital Status")]

        public Guid? MaritalStatusId { get; set; }

        [Display(Name = "Home Address")]

        public string HomeAddress { get; set; }



        [Display(Name = "NIN Number")]
        //[Required]
        public string NationalIDNumber { get; set; }

        //[Required]
        [Display(Name = "TIN Number")]
       
        public string TINNumber { get; set; }

        //[Required]
        [Display(Name = "NSSF Number")]
        
        public string NSSFNumber { get; set; }

        [Display(Name = "Birth District")]

        public Guid? DistrictBirthId { get; set; }

        [Display(Name = "Residence District")]
       
        public Guid? DistrictResidenceId { get; set; }

        [Display(Name = "Line Manager")]
        
        public Guid? SupervisorId { get; set; }


        
        public string JobTitle { get; set; }


        [Display(Name = "Name")]
       
        public string ContactPerson { get; set; }

        [Required]
        
        public string ContactPersonPhone { get; set; }

        [Display(Name = "Spouse Names")]
        
        public string SpouseName { get; set; }
        
        [Display(Name = "Spouse Address")]
        public string ContactAddress { get; set; }
        [Display(Name = "Birth Date")]
        public string SpouseDateOfBirth { get; set; }
        [Display(Name = "Phone Numbers")]
        
        public string PhoneNumbers { get; set; }

        public bool IsActive { get; set; }

     
        [Display(Name = "Department")]
        public Guid? DepartmentId { get; set; }

  
        [Display(Name = "Bank Name")]
        public Guid? BankId { get; set; }

        [Display(Name = "Branch")]
        
        public string Branch { get; set; }
        
        [Display(Name = "Bank Account")]
        public string AccountNumber { get; set; }

       // [Display(Name = "Beneficiary Names")]
      
        //public string Name { get; set; }
        //[Display(Name = "Beneficiary Address")]
        //public string Contact { get; set; }
        //[Display(Name = "Relationship")]
        //public Guid? RelationshipId { get; set; }


    

        [Display(Name = "Phone Number")]

        
        public string PersonalMobileNumber { get; set; }
        [Display(Name = "Work PhoneNumber")]
        
        public string WorkMobileNumber { get; set; }
        [Display(Name = "Alternative Number")]
        
        
        public string AlternativeMobileNumber { get; set; }
     
        public string District { get; set; }
        public string Nationality { get; set; }
        public string Department { get; set; }
        [Display(Name = "Join Date")]
        public string DateOfJoin { get; set; }
    }
}