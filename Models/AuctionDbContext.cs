using Microsoft.EntityFrameworkCore;
using AuctionSystem.Models;

namespace AuctionSystem.Models
{
    public class AuctionDbContext : DbContext
    {
        public DbSet<Auction> Auctions { get; set; }
        public DbSet<Bid> Bids { get; set; }
        public DbSet<AutoBid> AutoBids { get; set; }
        public DbSet<User> Users { get; set; }

        public AuctionDbContext(DbContextOptions<AuctionDbContext> options) : base(options) { }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Auction>()
                .HasOne(a => a.Seller)
                .WithMany(u => u.Auctions)
                .HasForeignKey(a => a.SellerId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Bid>()
                .HasOne(b => b.Auction)
                .WithMany(a => a.Bids)
                .HasForeignKey(b => b.AuctionId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Bid>()
                .HasOne(b => b.User)
                .WithMany(u => u.Bids)
                .HasForeignKey(b => b.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<AutoBid>()
                .HasOne(ab => ab.Auction)
                .WithMany(a => a.AutoBids)
                .HasForeignKey(ab => ab.AuctionId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<AutoBid>()
                .HasOne(ab => ab.User)
                .WithMany(u => u.AutoBids)
                .HasForeignKey(ab => ab.UserId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}