using Sozif.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

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

        public int PositionsCount
        {
            get
            {
                return OrderPositions.Count;
            }
        }

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

        public int TaxValue
        {
            get
            {
                return GrossValue - NetValue;
            }
        }

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
