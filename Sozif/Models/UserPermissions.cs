using System.Collections.Generic;

namespace Sozif
{
    public partial class UserPermissions
    {
        public UserPermissions()
        {
            Users = new HashSet<Users>();
        }

        public int PermLevel { get; set; }
        public string PermName { get; set; }
        public bool EditUsers { get; set; }
        public bool EditProducts { get; set; }
        public bool EditCustomers { get; set; }
        public bool EditOrders { get; set; }
        public bool EditInvoices { get; set; }

        public virtual ICollection<Users> Users { get; set; }
    }
}
