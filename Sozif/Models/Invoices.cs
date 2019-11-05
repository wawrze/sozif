using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Sozif
{
    [Table("invoices")]
    public partial class Invoices
    {
        public Invoices()
        {
            InvoicePositions = new HashSet<InvoicePositions>();
            Orders = new HashSet<Orders>();
        }

        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Key, Column("invoice_id", TypeName = "INTEGER")]
        public int InvoiceId { get; set; }

        [Column("invoice_number", TypeName = "VARCHAR(15)")]
        [Required]
        [Index(IsUnique = true)]
        public string InvoiceNumber { get; set; }

        [Column("customer_name", TypeName = "VARCHAR(50)")]
        [Required]
        public string CustomerName { get; set; }

        [Column("customer_nip", TypeName = "NUMERIC(10)")]
        [Required]
        public decimal CustomerNip { get; set; }

        [Column("customer_address", TypeName = "VARCHAR(30)")]
        [Required]
        public string CustomerAddress { get; set; }

        [Column("customer_postal_code", TypeName = "VARCHAR(6)")]
        [Required]
        public string CustomerPostalCode { get; set; }

        [Column("customer_city", TypeName = "VARCHAR(20)")]
        [Required]
        public string CustomerCity { get; set; }

        [Column("invoice_date", TypeName = "DATE")]
        [Required]
        public DateTime InvoiceDate { get; set; }

        [Column("days_to_pay", TypeName = "INTEGER")]
        [Required]
        public int DaysToPay { get; set; }

        [Column("user_name", TypeName = "VARCHAR(31)")]
        [Required]
        public string UserName { get; set; }

        [Column("customer_id", TypeName = "INTEGER")]
        [ForeignKey("Customers")]
        public int? CustomerId { get; set; }

        [Column("user_id", TypeName = "INTEGER")]
        [ForeignKey("Users")]
        public int? UserId { get; set; }

        [NotMapped]
        public int PositionsCount
        {
            get
            {
                return InvoicePositions.Count;
            }
        }

        [NotMapped]
        public int NetValue
        {
            get
            {
                int value = 0;
                foreach (InvoicePositions orderPosition in this.InvoicePositions)
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
                foreach (InvoicePositions orderPosition in this.InvoicePositions)
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
        public string NipString
        {
            get
            {
                if (CustomerNip == 0) return "";
                string fourthPart = ((int)(CustomerNip % 100)).ToString();
                if (fourthPart.Length == 1) fourthPart = "0" + fourthPart;
                string thirdPart = (((int)(CustomerNip / 100)) % 100).ToString();
                if (thirdPart.Length == 1) thirdPart = "0" + thirdPart;
                string secondPart = (((int)(CustomerNip / 10000)) % 1000).ToString();
                if (secondPart.Length == 1) secondPart = "0" + secondPart;
                if (secondPart.Length == 2) secondPart = "0" + secondPart;
                string firstPart = (((int)(CustomerNip / 10000000)) % 1000).ToString();
                if (firstPart.Length == 1) firstPart = "0" + firstPart;
                if (firstPart.Length == 2) firstPart = "0" + firstPart;
                return firstPart + "-" + secondPart + "-" + thirdPart + "-" + fourthPart;
            }
        }

        public virtual Customers Customer { get; set; }
        public virtual Users User { get; set; }
        public virtual ICollection<InvoicePositions> InvoicePositions { get; set; }
        public virtual ICollection<Orders> Orders { get; set; }
    }
}
