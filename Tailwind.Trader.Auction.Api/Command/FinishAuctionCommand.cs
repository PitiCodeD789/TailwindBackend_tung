﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Tailwind.Trader.Auction.Api.Command
{
    public class FinishAuctionCommand
    {
        [Required]
        [RegularExpression("^[0-9]*$")]
        public int ProductId { get; set; }
    }
}