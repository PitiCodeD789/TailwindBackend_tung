using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Tailwind.Trader.Auction.Api.ViewModel
{
    public class Bid
    {
        public int ProductId { get; set; }
        public string Name { get; set; }
        public decimal Price { get; set; }
    }
}
