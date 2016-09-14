using System.ComponentModel.DataAnnotations;

namespace ECommerce.Models
{
    public class AddProductView
    {
        [Required(ErrorMessage = "The field {0} is required")]
        [Range(1, double.MaxValue, ErrorMessage = "You must select a {0}")]
        [Display(Name = "Product", Prompt = "[Select a product...]")]
        public int ProductId { get; set; }

        [Required(ErrorMessage = "The field {0} is required")]
        [DisplayFormat(DataFormatString = "{0:C2}", ApplyFormatInEditMode = false)]
        [Range(0, double.MaxValue, ErrorMessage = "You must enter greather than {1} values in {0}")]

        public double Quantity { get; set; }

    }
}