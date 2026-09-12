using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using ApnaRent.Models;

namespace ApnaRent.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options) { }

        public DbSet<Item> Items { get; set; }
        public DbSet<Category> Categories { get; set; }
        public DbSet<Booking> Bookings { get; set; }
        public DbSet<EmailOtp> EmailOtps { get; set; }
        public DbSet<Cart> Carts { get; set; }
        public DbSet<Notification> Notifications { get; set; }
        public DbSet<Message> Messages { get; set; }
        public DbSet<SavedItem> SavedItems { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            builder.Entity<Item>().Property(i => i.PricePerDay).HasPrecision(18, 2);
            builder.Entity<Booking>().Property(b => b.TotalPrice).HasPrecision(18, 2);

            builder.Entity<Booking>()
                .HasOne(b => b.Item).WithMany()
                .HasForeignKey(b => b.ItemId).OnDelete(DeleteBehavior.Restrict);

            builder.Entity<Booking>()
                .HasOne(b => b.User).WithMany()
                .HasForeignKey(b => b.UserId).OnDelete(DeleteBehavior.Restrict);

            builder.Entity<Item>()
                .HasOne(i => i.Owner).WithMany()
                .HasForeignKey(i => i.OwnerId).OnDelete(DeleteBehavior.Restrict);

            builder.Entity<Message>()
                .HasOne(m => m.Item).WithMany()
                .HasForeignKey(m => m.ItemId).OnDelete(DeleteBehavior.Cascade);
        }
    }
}