using System;
using System.Collections.Generic;

namespace Sozif
{
    public partial class TaxRates
    {
        public TaxRates()
        {
            Products = new HashSet<Products>();
        }

        public int TaxRateId { get; set; }
        public int Rate { get; set; }

        public virtual ICollection<Products> Products { get; set; }
    }
}
