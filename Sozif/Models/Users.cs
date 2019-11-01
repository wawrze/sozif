using System.Collections.Generic;

namespace Sozif
{
    public partial class Users
    {
        public Users()
        {
            Invoices = new HashSet<Invoices>();
            Orders = new HashSet<Orders>();
        }

        public int UserId { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
        public string Firstname { get; set; }
        public string Lastname { get; set; }
        public int PermLevel { get; set; }

        public string UserData
        {
            get
            {
                return Username + "(" + Firstname + " " + Lastname + ")";
            }
        }

        public virtual UserPermissions PermLevelNavigation { get; set; }
        public virtual ICollection<Invoices> Invoices { get; set; }
        public virtual ICollection<Orders> Orders { get; set; }
    }
}
