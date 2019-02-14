using Microsoft.AspNet.Identity.EntityFramework;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity;
using System.Data.Entity.Infrastructure.Annotations;

namespace WebAspNetIdentity.IdentityExtensions
{
    public class ExtendendUserDbContext : IdentityDbContext<ExtendedUser>
    {
        public DbSet<AddressModel> Addresses { get; set; }
        public ExtendendUserDbContext(string nameOrConnectionString) : base(nameOrConnectionString)
        {
        }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            var address = modelBuilder.Entity<AddressModel>();
            address.ToTable("AspNetUserAddresses");
            address.HasKey(a => a.Id);

            var user = modelBuilder.Entity<ExtendedUser>();
            user.Property(u => u.FullName).IsRequired().HasMaxLength(256)
                .HasColumnAnnotation("Index", new IndexAnnotation(new IndexAttribute("FullNameIndex")));
            user.HasMany(u => u.Addresses).WithRequired().HasForeignKey(a => a.UserId);
        }
    }
}
