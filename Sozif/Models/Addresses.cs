using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Sozif
{
    [Table("addresses")]
    public partial class Addresses
    {
        public Addresses()
        {
            Orders = new HashSet<Orders>();
        }
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Key, Column("address_id", TypeName = "INTEGER")]
        public int AddressId { get; set; }

        [Column("street", TypeName = "VARCHAR(30)")]
        [Required]
        public string Street { get; set; }

        [Column("postal_code", TypeName = "VARCHAR(6)")]
        [Required]
        public string PostalCode { get; set; }

        [Column("city", TypeName = "VARCHAR(20)")]
        [Required]
        public string City { get; set; }

        [Column("is_main_address", TypeName = "BIT")]
        [Required]
        public bool IsMainAddress { get; set; }

        [Column("customer_id", TypeName = "INTEGER")]
        [ForeignKey("Customers")]
        [Required]
        public int CustomerId { get; set; }

        [NotMapped]
        public string FullAddress
        {
            get
            {
                return Street + ", " + PostalCode + " " + City;
            }
        }

        public virtual Customers Customer { get; set; }
        public virtual ICollection<Orders> Orders { get; set; }
    }
}
