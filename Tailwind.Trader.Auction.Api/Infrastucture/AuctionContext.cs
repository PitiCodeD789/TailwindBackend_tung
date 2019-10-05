using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Tailwind.Trader.Auction.Api.Models;

namespace Tailwind.Trader.Auction.Api.Infrastucture
{
    public class AuctionContext : DbContext
    {
        public DbSet<Detail> Details { get; set; }
        public DbSet<BidHistory> BidHistories { get; set; }
        public DbSet<ProductImagePath> ProductImagePaths { get; set; }
        public DbSet<Product> Products { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder builder)
        {
            builder.UseSqlServer(@"Server=git.dookdik.me;Database=TWTraderAuction;Trusted_Connection=True;user id=sa;password=Gg123456789;Integrated Security=false;");
        }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            builder.Entity<Product>().HasKey(x => x.Id);
            builder.Entity<Product>().Property(x => x.CreatedDateTme)
                .ValueGeneratedOnAddOrUpdate()
                .HasDefaultValueSql("GetUtcDate()");
            builder.Entity<Product>().Property(x => x.Expired).ValueGeneratedOnAddOrUpdate()
    .HasDefaultValueSql("GetUtcDate()");


            builder.Entity<ProductImagePath>().HasKey(x => x.Id);

            builder.Entity<BidHistory>().HasKey(x => x.Id);
            builder.Entity<BidHistory>().HasOne(x => x.Product).WithMany(x => x.BidHistories).HasForeignKey(x => x.ProductId);

            builder.Entity<BidHistory>().Property(x => x.CreatedDateTime)
                .ValueGeneratedOnAddOrUpdate()
                .HasDefaultValueSql("GetUtcDate()");


            builder.Entity<Detail>().HasKey(x => x.Id);

            base.OnModelCreating(builder);
        }
    }
}
