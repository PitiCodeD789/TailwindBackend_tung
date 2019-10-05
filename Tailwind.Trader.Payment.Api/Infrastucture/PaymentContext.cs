using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Tailwind.Trader.Payment.Api.Models;

namespace Tailwind.Trader.Payment.Api.Infrastucture
{

    public class PaymentContext : DbContext
    {
        public DbSet<PaymentTransaction> Payments { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder builder)
        {
            builder.UseSqlServer(@"Server=git.dookdik.me;Database=TWTraderPayment;Trusted_Connection=True;user id=sa;password=Gg123456789;Integrated Security=false;");
        }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            builder.Entity<PaymentTransaction>().HasKey(x => x.Id);

            builder.Entity<PaymentTransaction>()
                .Property(x => x.CreatedDateTime)
                .ValueGeneratedOnAddOrUpdate()
                .HasDefaultValueSql("GetUtcDate()");

            base.OnModelCreating(builder);
        }
    }
}
