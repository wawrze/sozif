using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace Sozif
{
    public partial class Customers
    {
        public Customers()
        {
            Addresses = new HashSet<Addresses>();
            Invoices = new HashSet<Invoices>();
            Orders = new HashSet<Orders>();
        }

        public int CustomerId { get; set; }
        public string CustomerName { get; set; }
        public decimal Nip { get; set; }
        public string ContactPerson { get; set; }
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
                string justNumber = "";
                foreach (char c in value)
                {
                    if (c != '-') justNumber += c;
                }
                Nip = Decimal.Parse(justNumber);
            }
        }
        public virtual ICollection<Addresses> Addresses { get; set; }
        public virtual ICollection<Invoices> Invoices { get; set; }
        public virtual ICollection<Orders> Orders { get; set; }
    }
}
