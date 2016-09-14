using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace ECommerce.Models
{
    public class CompanyCustomer
    {
        [Key]
        public int CompanyCustomerId { get; set; }

        public int CompanyId { get; set; }

        public int CustomerId { get; set; }

        public virtual Company Company { get; set; }

        public virtual Customer Customer { get; set; }
    }
}