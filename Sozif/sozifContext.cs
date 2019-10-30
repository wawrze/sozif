using Microsoft.EntityFrameworkCore;

namespace Sozif
{
    public partial class sozifContext : DbContext
    {
        public sozifContext()
        {
        }

        public sozifContext(DbContextOptions<sozifContext> options)
            : base(options)
        {
        }

        public virtual DbSet<Addresses> Addresses { get; set; }
        public virtual DbSet<Customers> Customers { get; set; }
        public virtual DbSet<InvoicePositions> InvoicePositions { get; set; }
        public virtual DbSet<Invoices> Invoices { get; set; }
        public virtual DbSet<OrderPositions> OrderPositions { get; set; }
        public virtual DbSet<Orders> Orders { get; set; }
        public virtual DbSet<Products> Products { get; set; }
        public virtual DbSet<TaxRates> TaxRates { get; set; }
        public virtual DbSet<UserPermissions> UserPermissions { get; set; }
        public virtual DbSet<Users> Users { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. See http://go.microsoft.com/fwlink/?LinkId=723263 for guidance on storing connection strings.
                optionsBuilder.UseSqlServer("Data Source=WAWRAHYPERBOOK;Initial Catalog=sozif;User Id=sa2; Password=sa2");
            }
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Addresses>(entity =>
            {
                entity.HasKey(e => e.AddressId)
                    .HasName("PK__addresse__CAA247C8BC9165F9");

                entity.ToTable("addresses");

                entity.Property(e => e.AddressId).HasColumnName("address_id");

                entity.Property(e => e.City)
                    .IsRequired()
                    .HasColumnName("city")
                    .HasMaxLength(20)
                    .IsUnicode(false);

                entity.Property(e => e.CustomerId).HasColumnName("customer_id");

                entity.Property(e => e.IsMainAddress).HasColumnName("is_main_address");

                entity.Property(e => e.PostalCode)
                    .IsRequired()
                    .HasColumnName("postal_code")
                    .HasMaxLength(6)
                    .IsUnicode(false);

                entity.Property(e => e.Street)
                    .IsRequired()
                    .HasColumnName("street")
                    .HasMaxLength(30)
                    .IsUnicode(false);

                entity.HasOne(d => d.Customer)
                    .WithMany(p => p.Addresses)
                    .HasForeignKey(d => d.CustomerId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK__addresses__custo__6B79F03D");
            });

            modelBuilder.Entity<Customers>(entity =>
            {
                entity.HasKey(e => e.CustomerId)
                    .HasName("PK__customer__CD65CB859DEF53B2");

                entity.ToTable("customers");

                entity.HasIndex(e => new { e.CustomerName, e.Nip })
                    .HasName("UQ__customer__D67037C536B8456E")
                    .IsUnique();

                entity.Property(e => e.CustomerId).HasColumnName("customer_id");

                entity.Property(e => e.ContactPerson)
                    .HasColumnName("contact_person")
                    .HasMaxLength(30)
                    .IsUnicode(false);

                entity.Property(e => e.CustomerName)
                    .IsRequired()
                    .HasColumnName("customer_name")
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.Nip)
                    .HasColumnName("nip")
                    .HasColumnType("numeric(10, 0)");

                entity.Property(e => e.PhoneNumber)
                    .HasColumnName("phone_number")
                    .HasColumnType("numeric(9, 0)");
            });

            modelBuilder.Entity<InvoicePositions>(entity =>
            {
                entity.HasKey(e => e.InvoicePositionId)
                    .HasName("PK__invoice___AA106E5E67AE6E5D");

                entity.ToTable("invoice_positions");

                entity.Property(e => e.InvoicePositionId).HasColumnName("invoice_position_id");

                entity.Property(e => e.Discount).HasColumnName("discount");

                entity.Property(e => e.InvoiceId).HasColumnName("invoice_id");

                entity.Property(e => e.ProductCount).HasColumnName("product_count");

                entity.Property(e => e.ProductName)
                    .IsRequired()
                    .HasColumnName("product_name")
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.ProductNetPrice).HasColumnName("product_net_price");

                entity.Property(e => e.ProductTaxRate).HasColumnName("product_tax_rate");

                entity.HasOne(d => d.Invoice)
                    .WithMany(p => p.InvoicePositions)
                    .HasForeignKey(d => d.InvoiceId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK__invoice_p__invoi__731B1205");
            });

            modelBuilder.Entity<Invoices>(entity =>
            {
                entity.HasKey(e => e.InvoiceId)
                    .HasName("PK__invoices__F58DFD49209E8028");

                entity.ToTable("invoices");

                entity.HasIndex(e => e.InvoiceNumber)
                    .HasName("UQ__invoices__8081A63A2A60A6A8")
                    .IsUnique();

                entity.Property(e => e.InvoiceId).HasColumnName("invoice_id");

                entity.Property(e => e.CustomerAddress)
                    .IsRequired()
                    .HasColumnName("customer_address")
                    .HasMaxLength(30)
                    .IsUnicode(false);

                entity.Property(e => e.CustomerCity)
                    .IsRequired()
                    .HasColumnName("customer_city")
                    .HasMaxLength(20)
                    .IsUnicode(false);

                entity.Property(e => e.CustomerId).HasColumnName("customer_id");

                entity.Property(e => e.CustomerName)
                    .IsRequired()
                    .HasColumnName("customer_name")
                    .HasMaxLength(30)
                    .IsUnicode(false);

                entity.Property(e => e.CustomerNip)
                    .HasColumnName("customer_nip")
                    .HasColumnType("numeric(10, 0)");

                entity.Property(e => e.CustomerPostalCode)
                    .IsRequired()
                    .HasColumnName("customer_postal_code")
                    .HasMaxLength(6)
                    .IsUnicode(false);

                entity.Property(e => e.DaysToPay).HasColumnName("days_to_pay");

                entity.Property(e => e.InvoiceDate)
                    .HasColumnName("invoice_date")
                    .HasColumnType("date");

                entity.Property(e => e.InvoiceNumber)
                    .IsRequired()
                    .HasColumnName("invoice_number")
                    .HasMaxLength(15)
                    .IsUnicode(false);

                entity.Property(e => e.UserId).HasColumnName("user_id");

                entity.Property(e => e.UserName)
                    .IsRequired()
                    .HasColumnName("user_name")
                    .HasMaxLength(31)
                    .IsUnicode(false);

                entity.HasOne(d => d.Customer)
                    .WithMany(p => p.Invoices)
                    .HasForeignKey(d => d.CustomerId)
                    .HasConstraintName("FK__invoices__custom__6F4A8121");

                entity.HasOne(d => d.User)
                    .WithMany(p => p.Invoices)
                    .HasForeignKey(d => d.UserId)
                    .HasConstraintName("FK__invoices__user_i__703EA55A");
            });

            modelBuilder.Entity<OrderPositions>(entity =>
            {
                entity.HasKey(e => new { e.OrderId, e.ProductId })
                    .HasName("PK__order_po__022945F60E36CFDC");

                entity.ToTable("order_positions");

                entity.Property(e => e.OrderId).HasColumnName("order_id");

                entity.Property(e => e.ProductId).HasColumnName("product_id");

                entity.Property(e => e.Count).HasColumnName("count");

                entity.Property(e => e.Discount).HasColumnName("discount");

                entity.HasOne(d => d.Order)
                    .WithMany(p => p.OrderPositions)
                    .HasForeignKey(d => d.OrderId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK__order_pos__order__7CA47C3F");

                entity.HasOne(d => d.Product)
                    .WithMany(p => p.OrderPositions)
                    .HasForeignKey(d => d.ProductId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK__order_pos__produ__7D98A078");
            });

            modelBuilder.Entity<Orders>(entity =>
            {
                entity.HasKey(e => e.OrderId)
                    .HasName("PK__orders__4659622968E2E783");

                entity.ToTable("orders");

                entity.HasIndex(e => e.OrderNumber)
                    .HasName("UQ__orders__730E34DF0C504B51")
                    .IsUnique();

                entity.Property(e => e.OrderId).HasColumnName("order_id");

                entity.Property(e => e.AddressId).HasColumnName("address_id");

                entity.Property(e => e.CustomerId).HasColumnName("customer_id");

                entity.Property(e => e.InvoiceId).HasColumnName("invoice_id");

                entity.Property(e => e.OrderDate)
                    .HasColumnName("order_date")
                    .HasColumnType("date");

                entity.Property(e => e.OrderNumber)
                    .IsRequired()
                    .HasColumnName("order_number")
                    .HasMaxLength(15)
                    .IsUnicode(false);

                entity.Property(e => e.RealisationDate)
                    .HasColumnName("realisation_date")
                    .HasColumnType("date");

                entity.Property(e => e.UserId).HasColumnName("user_id");

                entity.HasOne(d => d.Address)
                    .WithMany(p => p.Orders)
                    .HasForeignKey(d => d.AddressId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK__orders__address___79C80F94");

                entity.HasOne(d => d.Customer)
                    .WithMany(p => p.Orders)
                    .HasForeignKey(d => d.CustomerId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK__orders__customer__78D3EB5B");

                entity.HasOne(d => d.Invoice)
                    .WithMany(p => p.Orders)
                    .HasForeignKey(d => d.InvoiceId)
                    .HasConstraintName("FK__orders__invoice___76EBA2E9");

                entity.HasOne(d => d.User)
                    .WithMany(p => p.Orders)
                    .HasForeignKey(d => d.UserId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK__orders__user_id__77DFC722");
            });

            modelBuilder.Entity<Products>(entity =>
            {
                entity.HasKey(e => e.ProductId)
                    .HasName("PK__products__47027DF54CC2256F");

                entity.ToTable("products");

                entity.HasIndex(e => e.ProductName)
                    .HasName("UQ__products__2B5A6A5F450D18F4")
                    .IsUnique();

                entity.Property(e => e.ProductId).HasColumnName("product_id");

                entity.Property(e => e.BaseNetPrice).HasColumnName("base_net_price");

                entity.Property(e => e.ProductName)
                    .IsRequired()
                    .HasColumnName("product_name")
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.TaxRateId).HasColumnName("tax_rate_id");

                entity.HasOne(d => d.TaxRate)
                    .WithMany(p => p.Products)
                    .HasForeignKey(d => d.TaxRateId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK__products__tax_ra__65C116E7");
            });

            modelBuilder.Entity<TaxRates>(entity =>
            {
                entity.HasKey(e => e.TaxRateId)
                    .HasName("PK__tax_rate__4B78B3334F950950");

                entity.ToTable("tax_rates");

                entity.HasIndex(e => e.Rate)
                    .HasName("UQ__tax_rate__CAA90D6FBF5693EB")
                    .IsUnique();

                entity.Property(e => e.TaxRateId).HasColumnName("tax_rate_id");

                entity.Property(e => e.Rate).HasColumnName("rate");
            });

            modelBuilder.Entity<UserPermissions>(entity =>
            {
                entity.HasKey(e => e.PermLevel)
                    .HasName("PK__user_per__81E18404EEC757E9");

                entity.ToTable("user_permissions");

                entity.HasIndex(e => e.PermName)
                    .HasName("UQ__user_per__104765357E019F03")
                    .IsUnique();

                entity.Property(e => e.PermLevel)
                    .HasColumnName("perm_level")
                    .ValueGeneratedNever();

                entity.Property(e => e.EditCustomers).HasColumnName("edit_customers");

                entity.Property(e => e.EditInvoices).HasColumnName("edit_invoices");

                entity.Property(e => e.EditOrders).HasColumnName("edit_orders");

                entity.Property(e => e.EditProducts).HasColumnName("edit_products");

                entity.Property(e => e.EditUsers).HasColumnName("edit_users");

                entity.Property(e => e.PermName)
                    .IsRequired()
                    .HasColumnName("perm_name")
                    .HasMaxLength(20)
                    .IsUnicode(false);
            });

            modelBuilder.Entity<Users>(entity =>
            {
                entity.HasKey(e => e.UserId)
                    .HasName("PK__users__B9BE370F351A65C5");

                entity.ToTable("users");

                entity.HasIndex(e => e.Username)
                    .HasName("UQ__users__F3DBC572E9229083")
                    .IsUnique();

                entity.Property(e => e.UserId).HasColumnName("user_id");

                entity.Property(e => e.Firstname)
                    .IsRequired()
                    .HasColumnName("firstname")
                    .HasMaxLength(10)
                    .IsUnicode(false);

                entity.Property(e => e.Lastname)
                    .IsRequired()
                    .HasColumnName("lastname")
                    .HasMaxLength(20)
                    .IsUnicode(false);

                entity.Property(e => e.Password)
                    .IsRequired()
                    .HasColumnName("password")
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.PermLevel).HasColumnName("perm_level");

                entity.Property(e => e.Username)
                    .IsRequired()
                    .HasColumnName("username")
                    .HasMaxLength(10)
                    .IsUnicode(false);

                entity.HasOne(d => d.PermLevelNavigation)
                    .WithMany(p => p.Users)
                    .HasForeignKey(d => d.PermLevel)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK__users__perm_leve__5F141958");
            });

            OnModelCreatingPartial(modelBuilder);
        }

        partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
    }
}
