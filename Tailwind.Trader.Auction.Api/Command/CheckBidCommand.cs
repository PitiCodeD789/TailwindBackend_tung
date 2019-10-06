using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Tailwind.Trader.Auction.Api.Command
{
    public class CheckBidCommand
    {
        public int ProductId { get; set; }
        public int UserId { get; set; }
        public decimal Amount { get; set; }
    }
}
