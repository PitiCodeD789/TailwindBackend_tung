using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Tailwind.Trader.Auction.Api.Models
{
    public static class Helper
    {
        public enum AuctionStatus
        {
            Open,
            Close
        }

        public enum PaidStatus
        {
            Wait,
            Paid
        }

        public enum ImageType
        {
            MainPicture,
            SidePicture
        }
    }
}
