namespace Sozif
{
    public partial class OrderPositions
    {
        public int OrderId { get; set; }
        public int ProductId { get; set; }
        public int Count { get; set; }
        public int? Discount { get; set; }

        public int FinalNetPrice
        {
            get
            {
                if (Discount == null) return Product.BaseNetPrice;
                int discount = (int)Discount;
                return Product.BaseNetPrice * (100 - discount) / 100;
            }
        }

        public int FinalGrossPrice
        {
            get
            {
                if (Discount == null) return Product.BaseGrossPrice;
                int discount = (int)Discount;
                return Product.BaseGrossPrice * (100 - discount) / 100;
            }
        }

        public int FinalNetValue
        {
            get
            {
                return FinalNetPrice * Count;
            }
        }

        public int FinalGrossValue
        {
            get
            {
                return FinalGrossPrice * Count;
            }
        }

        public int FinalTaxValue
        {
            get
            {
                return FinalGrossValue - FinalNetValue;
            }
        }

        public virtual Orders Order { get; set; }
        public virtual Products Product { get; set; }
    }
}
