using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Tailwind.Trader.Auction.Api.Command
{
    public class BidCommand
    {
        [Required]
        [RegularExpression("^[0-9]*$")]
        public int BidderId { get; set; }

        [Required]
        [RegularExpression("^[a-zA-Z]*$")]
        public string BidderName { get; set; }

        [Required]
        [RegularExpression("^[0-9]*$")]
        public int ProductId { get; set; }

        [Required]
        [RegularExpression(@"^\d+\.\d{0,2}$")]
        [Range(0, 9999999999999999.99)]
        public decimal Price { get; set; }
    }
}