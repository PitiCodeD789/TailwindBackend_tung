using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Tailwind.Trader.Auction.Api.Command
{
    public class ChangeExpireCommand
    {
        public int ProductId { get; set; }
        public int Minute { get; set; }
    }
}
