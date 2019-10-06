using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Tailwind.Trader.Auction.Api.ViewModel
{
    public class AuctionedViewModel
    {
        public int Id { get; set; }
        public decimal Price { get; set; }
        public string Name { get; set; }
        public DateTime CreatedDateTime { get; set; }
        public DateTime Expired { get; set; }
        public int HighestBidderId { get; set; }
        public string HighestBidderName { get; set; }
        public Models.Helper.AuctionStatus AuctionStatus { get; set; }
        public Models.Helper.PaidStatus PaidStatus { get; set; }
        public double ProductWeight { get; set; }
        public string ProductImages { get; set; }

    }
}
