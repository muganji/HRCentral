using System;
using System.ComponentModel.DataAnnotations;

namespace HRCentral.Web.Models.Crimes
{
    public class CrimeListViewModel
    {
        
        public Guid Id { get; set; }
      
        public Guid? EmployeeID { get; set; }
       
        public bool IsConvicted { get; set; }
      
        public string CrimeDescription { get; set; }
        
        public string ConvictionDate { get; set; }
       
        public string ConvictionPlace { get; set; }
       
        public string SentenceImposed { get; set; }

        public string Employee { get; set; }
    }
}
