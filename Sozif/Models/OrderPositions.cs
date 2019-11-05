using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Sozif
{
    [Table("order_positions")]
    public partial class OrderPositions
    {
        [Key, Column("order_id", Order = 0, TypeName = "INTEGER")]
        [ForeignKey("Orders")]
        public int OrderId { get; set; }

        [Key, Column("product_id", Order = 1, TypeName = "INTEGER")]
        [ForeignKey("Products")]
        public int ProductId { get; set; }

        [Column("count", TypeName = "INTEGER")]
        [Required]
        public int Count { get; set; }

        [Column("discount", TypeName = "INTEGER")]
        public int? Discount { get; set; }

        [NotMapped]
        public int FinalNetPrice
        {
            get
            {
                if (Discount == null) return Product.BaseNetPrice;
                int discount = (int)Discount;
                return Product.BaseNetPrice * (100 - discount) / 100;
            }
        }

        [NotMapped]
        public int FinalGrossPrice
        {
            get
            {
                if (Discount == null) return Product.BaseGrossPrice;
                int discount = (int)Discount;
                return Product.BaseGrossPrice * (100 - discount) / 100;
            }
        }

        [NotMapped]
        public int FinalNetValue
        {
            get
            {
                return FinalNetPrice * Count;
            }
        }

        [NotMapped]
        public int FinalGrossValue
        {
            get
            {
                return FinalGrossPrice * Count;
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

        public virtual Orders Order { get; set; }
        public virtual Products Product { get; set; }
    }
}
