using System;
using System.Collections.Generic;

namespace Sozif
{
    public partial class Products
    {
        public Products()
        {
            OrderPositions = new HashSet<OrderPositions>();
        }

        public int ProductId { get; set; }
        public string ProductName { get; set; }
        public int BaseNetPrice { get; set; }
        public int TaxRateId { get; set; }

        public int BaseGrossPrice
        {
            get
            {
                return BaseNetPrice * (100 + TaxRate.Rate) / 100;
            }
        }

        public virtual TaxRates TaxRate { get; set; }
        public virtual ICollection<OrderPositions> OrderPositions { get; set; }
    }
}
