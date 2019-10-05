using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Tailwind.Trader.Auction.Api.Models;

namespace Tailwind.Trader.Auction.Api.ViewModel
{
    public class BidDetailViewModel
    {
        public List<ProductImagePath> ProductImages { get; set; }
        public BidHistory BidHistories { get; set; }

    }
}
