using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Sozif
{
    [Table("customers")]
    public partial class Customers
    {
        public Customers()
        {
            Addresses = new HashSet<Addresses>();
            Invoices = new HashSet<Invoices>();
            Orders = new HashSet<Orders>();
        }
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Key, Column("customer_id", TypeName = "INTEGER")]
        public int CustomerId { get; set; }

        [Column("customer_name", TypeName = "VARCHAR(50)")]
        [Required]
        [Index(IsUnique = true)]
        public string CustomerName { get; set; }

        [Column("nip", TypeName = "NUMERIC(10)")]
        [Required]
        [Index(IsUnique = true)]
        public decimal Nip { get; set; }

        [Column("contact_person", TypeName = "VARCHAR(30)")]
        public string ContactPerson { get; set; }

        [Column("phone_number", TypeName = "NUMERIC(9)")]
        public decimal? PhoneNumber { get; set; }

        [NotMapped]
        public string PhoneNumberString
        {
            get
            {
                if (PhoneNumber == null) return "";
                string thirdPart = ((int)(PhoneNumber % 1000)).ToString();
                if (thirdPart.Length == 1) thirdPart = "0" + thirdPart;
                if (thirdPart.Length == 2) thirdPart = "0" + thirdPart;
                string secondPart = (((int)(PhoneNumber / 1000)) % 1000).ToString();
                if (secondPart.Length == 1) secondPart = "0" + secondPart;
                if (secondPart.Length == 2) secondPart = "0" + secondPart;
                string firstPart = (((int)(PhoneNumber / 1000000)) % 1000).ToString();
                if (firstPart.Length == 1) firstPart = "0" + firstPart;
                if (firstPart.Length == 2) firstPart = "0" + firstPart;
                return firstPart + "-" + secondPart + "-" + thirdPart;
            }

            set
            {
                if (value == null) return;
                string justNumber = "";
                foreach (char c in value)
                {
                    if (c != '-') justNumber += c;
                }
                PhoneNumber = Decimal.Parse(justNumber);
            }
        }

        [NotMapped]
        public string NipString
        {
            get
            {
                if (Nip == 0) return "";
                string fourthPart = ((int)(Nip % 100)).ToString();
                if (fourthPart.Length == 1) fourthPart = "0" + fourthPart;
                string thirdPart = (((int)(Nip / 100)) % 100).ToString();
                if (thirdPart.Length == 1) thirdPart = "0" + thirdPart;
                string secondPart = (((int)(Nip / 10000)) % 1000).ToString();
                if (secondPart.Length == 1) secondPart = "0" + secondPart;
                if (secondPart.Length == 2) secondPart = "0" + secondPart;
                string firstPart = (((int)(Nip / 10000000)) % 1000).ToString();
                if (firstPart.Length == 1) firstPart = "0" + firstPart;
                if (firstPart.Length == 2) firstPart = "0" + firstPart;
                return firstPart + "-" + secondPart + "-" + thirdPart + "-" + fourthPart;
            }

            set
            {
                if (value == null) return;
                string justNumber = "";
                foreach (char c in value)
                {
                    if (c != '-') justNumber += c;
                }
                Nip = Decimal.Parse(justNumber);
            }
        }

        [NotMapped]
        public string Street { get; set; }

        [NotMapped]
        public string PostalCode { get; set; }

        [NotMapped]
        public string City { get; set; }

        public override string ToString()
        {
            return CustomerName + " (" + NipString + ")";
        }

        public virtual ICollection<Addresses> Addresses { get; set; }
        public virtual ICollection<Invoices> Invoices { get; set; }
        public virtual ICollection<Orders> Orders { get; set; }
    }
}
