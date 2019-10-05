using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Tailwind.Trader.Payment.Api.Models
{
    public class PaymentTransaction
    {
        public int Id { get; set; }
        public int PayerId { get; set; }
        public DateTime PaymentTime { get; set; }
        public string PaymentReference { get; set; }
        public decimal Amount { get; set; }
        public DateTime CreatedDateTime { get; set; }
    }
}
