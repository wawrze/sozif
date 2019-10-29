using System;
using System.Collections.Generic;

namespace Sozif
{
    public partial class InvoicePositions
    {
        public int InvoicePositionId { get; set; }
        public string ProductName { get; set; }
        public int ProductCount { get; set; }
        public int ProductNetPrice { get; set; }
        public int ProductTaxRate { get; set; }
        public int? Discount { get; set; }
        public int InvoiceId { get; set; }

        public virtual Invoices Invoice { get; set; }
    }
}
