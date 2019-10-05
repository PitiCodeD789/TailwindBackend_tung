using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using static Tailwind.Trader.Auction.Api.Models.Helper;

namespace Tailwind.Trader.Auction.Api.Models
{
    public class Product
    {
        public int Id { get; set; }
        public decimal Price { get; set; }
        public string Name { get; set; }
        public DateTime CreatedDateTme { get; set; }
        public DateTime Expired { get; set; }
        public int HighestBidder { get; set; }
        public AuctionStatus AuctionStatus { get; set; }
        public PaidStatus PaidStatus { get; set; }

        public List<ProductImagePath> ProductImages { get; set; }
        public List<Detail> Details { get; set; }
        
        public ICollection<BidHistory> BidHistories { get; set; }
    }
}
