using System.Collections.Generic;

namespace Sozif
{
    public partial class Addresses
    {
        public Addresses()
        {
            Orders = new HashSet<Orders>();
        }

        public int AddressId { get; set; }
        public string Street { get; set; }
        public string PostalCode { get; set; }
        public string City { get; set; }
        public bool IsMainAddress { get; set; }
        public int CustomerId { get; set; }

        public virtual Customers Customer { get; set; }
        public virtual ICollection<Orders> Orders { get; set; }
    }
}
