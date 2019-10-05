using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Tailwind.Trader.Payment.Api.Command
{
    public class PaymentCommand
    {
        [Required]
        public int PayerId { get; set; }

        [Required]
        public string CardName { get; set; }

        [Required]
        public string CardNumber { get; set; }

        [Required]
        public int ExpireMonth { get; set; }

        [Required]
        public int ExpireYear { get; set; }

        [Required]
        public string Cvv { get; set; }

        [Required]
        public long Amount { get; set; }
    }
}
