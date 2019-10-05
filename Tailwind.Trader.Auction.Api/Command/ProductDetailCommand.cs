using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Tailwind.Trader.Auction.Api.Command
{
    public class ProductDetailCommand
    {
        public int ProductId { get; set; }
        public string Topic { get; set; }
        public string TopicDetail { get; set; }
    }
}
