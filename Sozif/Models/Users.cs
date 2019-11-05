using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Sozif
{
    [Table("users")]
    public partial class Users
    {
        public Users()
        {
            Invoices = new HashSet<Invoices>();
            Orders = new HashSet<Orders>();
        }

        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Column ("user_id")]
        public int UserId { get; set; }

        [Column("username", TypeName = "varchar(10)")]
        [Required]
        [Index(IsUnique = true)]
        public string Username { get; set; }

        [Column("password", TypeName = "varchar(100)")]
        [Required]
        public string Password { get; set; }

        [Column("firstname", TypeName = "varchar(10)")]
        [Required]
        public string Firstname { get; set; }

        [Column("lastname", TypeName = "varchar(20)")]
        [Required]
        public string Lastname { get; set; }

        [Column("perm_level")]
        [ForeignKey("UserPermissions")]
        [Required]
        public int PermLevel { get; set; }
        
        [NotMapped]
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
