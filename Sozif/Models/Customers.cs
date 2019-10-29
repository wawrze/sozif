using System;
using System.Collections.Generic;

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

        public virtual ICollection<Addresses> Addresses { get; set; }
        public virtual ICollection<Invoices> Invoices { get; set; }
        public virtual ICollection<Orders> Orders { get; set; }
    }
}
