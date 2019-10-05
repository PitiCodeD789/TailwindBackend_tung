using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using static Tailwind.Trader.Auction.Api.Models.Helper;

namespace Tailwind.Trader.Auction.Api.Models
{
    public class ProductImagePath
    {
        public int Id { get; set; }
        public int ProductId { get; set; }
        public ImageType ImageType { get; set; }
        public string ImagePath { get; set; }
    }
}
