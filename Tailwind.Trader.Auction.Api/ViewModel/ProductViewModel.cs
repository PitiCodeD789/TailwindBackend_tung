using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using static Tailwind.Trader.Auction.Api.Models.Helper;

namespace Tailwind.Trader.Auction.Api.ViewModel
{
    public class ProductViewModel
    {
        public int Id { get; set; }
        public decimal Price { get; set; }
        public string Name { get; set; }
        public DateTime CreatedDateTme { get; set; }
        public DateTime Expired { get; set; }
        public int HighestBidderId { get; set; }
        public string HighestBidderName { get; set; }
        public AuctionStatus AuctionStatus { get; set; }
        public PaidStatus PaidStatus { get; set; }
        public double ProductWeight { get; set; }
    }
}
