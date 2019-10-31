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

        public int FinalNetPrice
        {
            get
            {
                if (Discount == null) return ProductNetPrice;
                int discount = (int)Discount;
                return ProductNetPrice * (100 - discount) / 100;
            }
        }

        public int FinalGrossPrice
        {
            get
            {
                if (Discount == null) return ProductNetPrice * (100 + ProductTaxRate) / 100;
                int discount = (int)Discount;
                return ProductNetPrice * (100 + ProductTaxRate) * (100 - discount) / 10000;
            }
        }

        public int FinalNetValue
        {
            get
            {
                return FinalNetPrice * ProductCount;
            }
        }

        public int FinalGrossValue
        {
            get
            {
                return FinalGrossPrice * ProductCount;
            }
        }

        public int FinalTaxValue
        {
            get
            {
                return FinalGrossValue - FinalNetValue;
            }
        }

        public virtual Invoices Invoice { get; set; }
    }
}
