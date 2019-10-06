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
        [RegularExpression(@"^[0-9]*$")]
        public int PayerId { get; set; }

        [Required]
        [RegularExpression(@"^[0-9]*$")]
        public int ProductId { get; set; }

        [Required]
        [RegularExpression(@"^[a-zA-Z \t]+$")]
        public string CardName { get; set; }

        [Required]
        [RegularExpression(@"^(\d{16})$")]
        public string CardNumber { get; set; }

        [Required]
        [RegularExpression(@"^([1-9]|1[0-2])$")]
        public int ExpireMonth { get; set; }

        [Required]
        [RegularExpression(@"^(\d{2})$")]
        public int ExpireYear { get; set; }

        [Required]
        [RegularExpression(@"^(\d{3})$")]
        public string Cvv { get; set; }

        [Required]
        [RegularExpression(@"^[0-9.]+$")]
        public long Amount { get; set; }
    }
}