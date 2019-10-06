using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Tailwind.Trader.Auction.Api.Command
{
    public class NewProductCommand
    {
        public string Name { get; set; }
        public double ProductWeight { get; set; }
        public decimal Price { get; set; }
        public DateTime Expired { get; set; }

    }
}
