using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace ECommerce.Models
{
    public class Product
    {

        [Key]
        public int ProductId { get; set; }

        [Required(ErrorMessage = "The field {0} is required")]
        [Range(1, double.MaxValue, ErrorMessage = "You must select a {0}")]
        [Index("ProductId_CompanyId_Description_Index", 1, IsUnique = true)]
        [Index("ProductId_CompanyId_BarCode_Index", 1, IsUnique = true)]
        [Display(Name = "Company")]
        public int CompanyId { get; set; }

        [Required(ErrorMessage = "The field {0} is required")]
        [MaxLength(50, ErrorMessage = "The field {0} must be maximun {1} characters length")]
        [Display(Name = "Product")]
        [Index("ProductId_CompanyId_Description_Index", 2, IsUnique = true)]
        public string Description { get; set; }

        [Required(ErrorMessage = "The field {0} is required")]
        [MaxLength(13, ErrorMessage = "The field {0} must be maximun {1} characters length")]
        [Display(Name = "Bar Code")]
        [Index("ProductId_CompanyId_BarCode_Index", 2, IsUnique = true)]
        public string BarCode { get; set; }

        [Required(ErrorMessage = "The field {0} is required")]
        [Range(1, double.MaxValue, ErrorMessage = "You must select a {0}")]
        [Display(Name = "Category")]
        public int CategoryId { get; set; }

        [Required(ErrorMessage = "The field {0} is required")]
        [Range(1, double.MaxValue, ErrorMessage = "You must select a {0}")]
        [Display(Name = "Tax")]
        public int TaxId { get; set; }

        [Required(ErrorMessage = "The field {0} is required")]
        [DisplayFormat(DataFormatString = "{0:C2}", ApplyFormatInEditMode = false)]
        [Range(0, double.MaxValue, ErrorMessage = "You must select a {0} between {1} and {2}")]
        public decimal Price { get; set; }

        [DataType(DataType.ImageUrl)]
        public string Image { get; set; }

        [NotMapped]
        [Display(Name = "Image")]
        public HttpPostedFileBase ImageFile { get; set; }

        [DataType(DataType.MultilineText)]
        public string Remarks { get; set; }

        [DisplayFormat(DataFormatString = "{0:N2}", ApplyFormatInEditMode = false)]
        public double Stock { get { return Inventories == null ?0: Inventories.Sum(i => i.Stock); } }

        public virtual Company Company { get; set; }

        public virtual Category Category { get; set; }

        public virtual Tax Tax { get; set; }

        public virtual ICollection<Inventory> Inventories { get; set; }

        public virtual ICollection<OrderDetail> OrderDetails { get; set; }

        public virtual ICollection<OrderDetailTmp> OrderDetailTmps { get; set; }
    }
}