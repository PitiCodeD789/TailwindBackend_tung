using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Tailwind.Trader.Auction.Api.Models
{
    public class BidHistory
    {
        public int Id { get; set; }
        public int ProductId { get; set; }
        public int BidderId { get; set; }
        public decimal Price { get; set; }
        public DateTime CreatedDateTime { get; set; }

        [JsonIgnore]
        public Product Product { get; set; }

    }
}
