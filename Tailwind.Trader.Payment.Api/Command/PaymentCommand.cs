using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Tailwind.Trader.Payment.Api.Command
{
    public class PaymentCommand
    {
        public int PayerId { get; set; }
        public string CardName { get; set; }
        public string CardNumber { get; set; }
        public int ExpireMonth { get; set; }
        public int ExpireYear { get; set; }
        public string Cvv { get; set; }
        public long Amount { get; set; }
    }
}
