using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Sozif
{
    [Table("user_permissions")]
    public partial class UserPermissions
    {
        public UserPermissions()
        {
            Users = new HashSet<Users>();
        }

        [Key, Column("perm_level", TypeName = "INTEGER")]
        public int PermLevel { get; set; }

        [Column("perm_name", TypeName = "VARCHAR(20)")]
        [Required]
        [Index(IsUnique = true)]
        public string PermName { get; set; }

        [Column("edit_users", TypeName = "BIT")]
        [Required]
        public bool EditUsers { get; set; }

        [Column("edit_products", TypeName = "BIT")]
        [Required]
        public bool EditProducts { get; set; }

        [Column("edit_customers", TypeName = "BIT")]
        [Required]
        public bool EditCustomers { get; set; }

        [Column("edit_orders", TypeName = "BIT")]
        [Required]
        public bool EditOrders { get; set; }

        [Column("edit_invoices", TypeName = "BIT")]
        [Required]
        public bool EditInvoices { get; set; }

        public int UsersWithPermLevel
        {
            get
            {
                return Users.Count;
            }
        }

        public virtual ICollection<Users> Users { get; set; }
    }
}
