using System.ComponentModel.DataAnnotations;


namespace HRCentral.Web.Models.BirthOrder
{
    public class NewBirtOrderViewModel
    {
        [RegularExpression(@"^[a-zA-Z\\-\\_\s]*$", ErrorMessage = "Birth Order contains only  Characters. e.g First born")]
        [Display(Name = "Birth Order")]
        [Required]
        public string Name { get; set; }
    }
}
