using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Sozif
{
    [Table("orders")]
    public partial class Orders
    {
        public Orders()
        {
            OrderPositions = new HashSet<OrderPositions>();
        }

        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Key, Column("order_id", TypeName = "INTEGER")]
        public int OrderId { get; set; }

        [Column("order_number", TypeName = "VARCHAR(15)")]
        [Required]
        [Index(IsUnique = true)]
        public string OrderNumber { get; set; }

        [Column("order_date", TypeName = "DATE")]
        [Required]
        public DateTime OrderDate { get; set; }

        [Column("realisation_date", TypeName = "DATE")]
        public DateTime? RealisationDate { get; set; }

        [Column("invoice_id", TypeName = "INTEGER")]
        [ForeignKey("Invoices")]
        public int? InvoiceId { get; set; }

        [Column("user_id", TypeName = "INTEGER")]
        [ForeignKey("Users")]
        [Required]
        public int UserId { get; set; }

        [Column("customer_id", TypeName = "INTEGER")]
        [ForeignKey("Customers")]
        [Required]
        public int CustomerId { get; set; }

        [Column("address_id", TypeName = "INTEGER")]
        [ForeignKey("Addresses")]
        [Required]
        public int AddressId { get; set; }

        [NotMapped]
        public int PositionsCount
        {
            get
            {
                return OrderPositions.Count;
            }
        }

        [NotMapped]
        public int NetValue
        {
            get
            {
                int value = 0;
                foreach (OrderPositions orderPosition in this.OrderPositions)
                {
                    value += orderPosition.FinalNetValue;
                }
                return value;
            }
        }

        [NotMapped]
        public int GrossValue
        {
            get
            {
                int value = 0;
                foreach (OrderPositions orderPosition in this.OrderPositions)
                {
                    value += orderPosition.FinalGrossValue;
                }
                return value;
            }
        }

        [NotMapped]
        public int TaxValue
        {
            get
            {
                return GrossValue - NetValue;
            }
        }

        [NotMapped]
        public string UserName
        {
            get
            {
                return User.Firstname + " " + User.Lastname;
            }
        }

        public virtual Addresses Address { get; set; }
        public virtual Customers Customer { get; set; }
        public virtual Invoices Invoice { get; set; }
        public virtual Users User { get; set; }
        public virtual ICollection<OrderPositions> OrderPositions { get; set; }
    }
}
