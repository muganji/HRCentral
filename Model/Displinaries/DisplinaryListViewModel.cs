using System;

namespace HRCentral.Web.Models.Displinaries
{
    public class DisplinaryListViewModel
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
