﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Tailwind.Trader.Payment.Api.Command
{
    public class PaidStatusCommand
    {
        public int ProductId { get; set; }
        public int PayerId { get; set; }
    }
}
