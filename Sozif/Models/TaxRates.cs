using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Sozif
{
    [Table("tax_rates")]
    public partial class TaxRates
    {
        public TaxRates()
        {
            Products = new HashSet<Products>();
        }

        [Key, Column("tax_rate_id", TypeName = "INTEGER")]
        public int TaxRateId { get; set; }

        [Column("rate", TypeName = "INTEGER")]
        [Required]
        [Index(IsUnique = true)]
        public int Rate { get; set; }

        public virtual ICollection<Products> Products { get; set; }
    }
}
