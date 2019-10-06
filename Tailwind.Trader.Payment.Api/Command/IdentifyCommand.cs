using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Tailwind.Trader.Payment.Api.Command
{
    public class IdentifyCommand
    {
        public int ProductId { get; set; }
        public int UserId { get; set; }
        public decimal Amount { get; set; }
    }
}
