using System;
using System.Collections.Generic;

namespace Sozif
{
    public partial class Orders
    {
        public Orders()
        {
            OrderPositions = new HashSet<OrderPositions>();
        }

        public int OrderId { get; set; }
        public string OrderNumber { get; set; }
        public DateTime OrderDate { get; set; }
        public DateTime? RealisationDate { get; set; }
        public int? InvoiceId { get; set; }
        public int UserId { get; set; }
        public int CustomerId { get; set; }
        public int AddressId { get; set; }

        public virtual Addresses Address { get; set; }
        public virtual Customers Customer { get; set; }
        public virtual Invoices Invoice { get; set; }
        public virtual Users User { get; set; }
        public virtual ICollection<OrderPositions> OrderPositions { get; set; }
    }
}
