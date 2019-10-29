using System;
using System.Collections.Generic;

namespace Sozif
{
    public partial class Invoices
    {
        public Invoices()
        {
            InvoicePositions = new HashSet<InvoicePositions>();
            Orders = new HashSet<Orders>();
        }

        public int InvoiceId { get; set; }
        public string InvoiceNumber { get; set; }
        public string CustomerName { get; set; }
        public decimal CustomerNip { get; set; }
        public string CustomerAddress { get; set; }
        public string CustomerPostalCode { get; set; }
        public string CustomerCity { get; set; }
        public DateTime InvoiceDate { get; set; }
        public int DaysToPay { get; set; }
        public string UserName { get; set; }
        public int? CustomerId { get; set; }
        public int? UserId { get; set; }

        public virtual Customers Customer { get; set; }
        public virtual Users User { get; set; }
        public virtual ICollection<InvoicePositions> InvoicePositions { get; set; }
        public virtual ICollection<Orders> Orders { get; set; }
    }
}
