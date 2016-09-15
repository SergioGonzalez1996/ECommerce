using System.ComponentModel.DataAnnotations;

namespace ECommerce.Models
{
    public class CompanySupplier
    {
        [Key]
        public int CompanySupplierId { get; set; }

        public int CompanyId { get; set; }

        public int SupplierId { get; set; }

        public virtual Company Company { get; set; }

        public virtual Supplier Supplier { get; set; }
    }
}