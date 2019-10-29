using System;
using System.Collections.Generic;

namespace Sozif
{
    public partial class OrderPositions
    {
        public int OrderId { get; set; }
        public int ProductId { get; set; }
        public int Count { get; set; }
        public int? Discount { get; set; }

        public virtual Orders Order { get; set; }
        public virtual Products Product { get; set; }
    }
}
