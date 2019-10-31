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

        public int PositionsCount
        {
            get
            {
                return InvoicePositions.Count;
            }
        }

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

        public int TaxValue
        {
            get
            {
                return GrossValue - NetValue;
            }
        }

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
