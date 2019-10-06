using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Tailwind.Trader.Auction.Api.Command
{
    public class ImagePathCommand
    {
        public int ProductId { get; set; }
        public string ImagePath { get; set; }
    }
}
