using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Tailwind.Trader.User.Api.Command
{
    public class EditAddressCommand
    {
        [Required]
        public int UserId { get; set; }

        [Required]
        public string Country { get; set; }

        [Required]
        public string Address { get; set; }

    }
}
