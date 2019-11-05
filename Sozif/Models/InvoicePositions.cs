using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Sozif
{
    [Table("invoice_positions")]
    public partial class InvoicePositions
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Key, Column("invoice_position_id", TypeName = "INTEGER")]
        public int InvoicePositionId { get; set; }

        [Column("product_name", TypeName = "VARCHAR(50)")]
        [Required]
        public string ProductName { get; set; }

        [Column("product_count", TypeName = "INTEGER")]
        [Required]
        public int ProductCount { get; set; }

        [Column("product_net_price", TypeName = "INTEGER")]
        [Required]
        public int ProductNetPrice { get; set; }

        [Column("product_tax_rate", TypeName = "INTEGER")]
        [Required]
        public int ProductTaxRate { get; set; }

        [Column("discount", TypeName = "INTEGER")]
        public int? Discount { get; set; }

        [Column("invoice_id", TypeName = "INTEGER")]
        [Required]
        [ForeignKey("Invoices")]
        public int InvoiceId { get; set; }

        [NotMapped]
        public int FinalNetPrice
        {
            get
            {
                if (Discount == null) return ProductNetPrice;
                int discount = (int)Discount;
                return ProductNetPrice * (100 - discount) / 100;
            }
        }

        [NotMapped]
        public int FinalGrossPrice
        {
            get
            {
                if (Discount == null) return ProductNetPrice * (100 + ProductTaxRate) / 100;
                int discount = (int)Discount;
                return ProductNetPrice * (100 + ProductTaxRate) * (100 - discount) / 10000;
            }
        }

        [NotMapped]
        public int FinalNetValue
        {
            get
            {
                return FinalNetPrice * ProductCount;
            }
        }

        [NotMapped]
        public int FinalGrossValue
        {
            get
            {
                return FinalGrossPrice * ProductCount;
            }
        }

        [NotMapped]
        public int FinalTaxValue
        {
            get
            {
                return FinalGrossValue - FinalNetValue;
            }
        }

        public virtual Invoices Invoice { get; set; }
    }
}
