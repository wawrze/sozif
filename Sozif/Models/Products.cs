using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Sozif
{
    [Table("products")]
    public partial class Products
    {
        public Products()
        {
            OrderPositions = new HashSet<OrderPositions>();
        }

        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Key, Column("product_id", TypeName = "INTEGER")]
        public int ProductId { get; set; }

        [Column("product_name", TypeName = "VARCHAR(50)")]
        [Required]
        [Index(IsUnique = true)]
        public string ProductName { get; set; }

        [Column("product_name", TypeName = "INTEGER")]
        [Required]
        public int BaseNetPrice { get; set; }

        [Column("tax_rate_id", TypeName = "INTEGER")]
        [ForeignKey("TaxRates")]
        public int TaxRateId { get; set; }

        [NotMapped]
        public int BaseGrossPrice
        {
            get
            {
                if (TaxRate == null) return 0;
                return BaseNetPrice * (100 + TaxRate.Rate) / 100;
            }
        }

        public virtual TaxRates TaxRate { get; set; }
        public virtual ICollection<OrderPositions> OrderPositions { get; set; }
    }
}
