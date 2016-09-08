using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ECommerce.Models
{
    public class Tax
    {
        [Key]
        public int TaxId { get; set; }

        [Required(ErrorMessage = "The field {0} is required")]
        [MaxLength(50, ErrorMessage = "The field {0} must be maximun {1} characters length")]
        [Display(Name = "Tax")]
        [Index("Tax_CompanyId_Description_Index", 2, IsUnique = true)]
        public string Description { get; set; }

        [Required(ErrorMessage = "The field {0} is required")]
        [DisplayFormat(DataFormatString = "{0:P2}", ApplyFormatInEditMode = false)]
        [Range(0, 1, ErrorMessage = "You must select a {0} between {1} and {2}")]
        public double Rate { get; set; }

        [Required(ErrorMessage = "The field {0} is required")]
        [Range(1, double.MaxValue, ErrorMessage = "You must select a {0}")]
        [Index("Tax_CompanyId_Description_Index", 1, IsUnique = true)]
        [Display(Name = "Company")]
        public int CompanyId { get; set; }

        public virtual Company Company { get; set; }

        public virtual ICollection<Product> Products { get; set; }
    }
}