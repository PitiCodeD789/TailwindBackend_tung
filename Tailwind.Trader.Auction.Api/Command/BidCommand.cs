using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Tailwind.Trader.Auction.Api.Command
{
    public class BidCommand
    {
        public int BidId { get; set; }
        public int BidderId { get; set; }
        public string BidderName { get; set; }
        public int ProductId { get; set; }
        public decimal Price { get; set; }
        public DateTime CreatedDateTime { get; set; }

    }
}
