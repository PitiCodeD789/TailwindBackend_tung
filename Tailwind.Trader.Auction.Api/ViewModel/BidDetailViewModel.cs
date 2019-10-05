using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Tailwind.Trader.Auction.Api.Models;

namespace Tailwind.Trader.Auction.Api.ViewModel
{
    public class BidDetailViewModel
    {
        public string ProductName { get; set; }
        public string HigherBidder { get; set; }
        public decimal Price { get; set; }
        public DateTime Expired { get; set; }
        public List<ProductImagePath> ProductImages { get; set; }
        public BidHistory BidHistories { get; set; }
    }
}
