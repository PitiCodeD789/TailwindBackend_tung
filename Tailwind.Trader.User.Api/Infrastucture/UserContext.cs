using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Tailwind.Trader.User.Api.Infrastucture
{
    public class UserContext : DbContext
    {
        public DbSet<Models.User> Users { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder builder)
        {
            builder.UseSqlServer(@"Server=git.dookdik.me;Database=TWTraderUser;Trusted_Connection=True;user id=sa;password=Gg123456789;Integrated Security=false;");
        }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            builder.Entity<Models.User>().HasKey(x => x.Id);

            builder.Entity<Models.User>().HasAlternateKey(x => x.Email);

            builder.Entity<Models.User>().Property(x => x.CreatedDateTime)
                .ValueGeneratedOnAddOrUpdate()
                .HasDefaultValueSql("GetUtcDate()");

        }
    }
}
