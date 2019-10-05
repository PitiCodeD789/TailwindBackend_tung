using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Tailwind.Trader.Auction.Api.Command;
using Tailwind.Trader.Auction.Api.Infrastucture;
using Tailwind.Trader.Auction.Api.Models;
using Tailwind.Trader.Auction.Api.ViewModel;

namespace Tailwind.Trader.Auction.Api.Controllers
{
    [Route("v1/api/[controller]")]
    [ApiController]
    public class AuctionController : ControllerBase
    {
        private readonly AuctionContext _auctionContext;
        public AuctionController(AuctionContext auctionContext)
        {
            _auctionContext = auctionContext;
        }

        //http://192.168.1.40:30000/v1/api/auction/products
        [HttpGet("Products")]
        public ActionResult GetProducts()
        {
            List<Product> result = _auctionContext.Products.ToList();
            return Ok(result);
        }

        //http://192.168.1.40:30000/v1/api/auction/product/{id}
        [HttpGet("Product/{id}")]
        public ActionResult GetProductById(int id)
        {
            var result = _auctionContext.Products.FirstOrDefault(x => x.Id == id);
            if (result == null)
                return BadRequest();

            return Ok(result);
        }

        //http://192.168.1.40:30000/v1/api/auction/biddetail/{userId}
        [HttpGet("BidDetail/{userId}")]
        public ActionResult GetBidDetail(int userId)
        {
            var bidDetail = _auctionContext.Products.Include(x => x.BidHistories).Where(product => product.BidHistories.Any(bid => bid.BidderId == userId)).Select(x => new { x.BidHistories, x.ProductImages}).ToList();
            List<BidDetailViewModel> bidDetailViewModel = bidDetail.Select(s => new BidDetailViewModel()
            {
                BidHistories = s.BidHistories.LastOrDefault(x => x.BidderId == userId),
                ProductImages = s.ProductImages
            }).ToList();       
   
            return Ok(bidDetailViewModel);
        }

        //http://192.168.1.40:30000/v1/api/auction/currentbid/{userId}
        [HttpGet("CurrentBid/{userId}")]
        public ActionResult GetCurrentBid(int userId)
        {
            var bidDetail = _auctionContext.Products.Include(x => x.BidHistories).Where(x => x.AuctionStatus == Helper.AuctionStatus.Open).Select(x => new { x.BidHistories, x.ProductImages, x.HighestBidder }).ToList();
            List<BidDetailViewModel> bidDetailViewModel = bidDetail.Select(s => new BidDetailViewModel()
            {
                BidHistories = s.BidHistories.LastOrDefault(x => x.BidderId == userId),
                ProductImages = s.ProductImages
            }).ToList();

            return Ok(bidDetailViewModel);
        }

        //http://192.168.1.40:30000/v1/api/auction/bid
        [HttpPost("Bid")]
        public ActionResult Bid([FromBody]BidCommand bidCommand)
        {
            Product a = _auctionContext.Products.FirstOrDefault(x => x.Id == bidCommand.ProductId);
            if (a == null)
                return BadRequest();

            a.HighestBidder = bidCommand.BidderId;
            a.Price = bidCommand.Price;

            bool isUpdate, isAdd;

            try
            {
                _auctionContext.Update(a);
                _auctionContext.SaveChanges();
                isUpdate = true;
                isAdd = CreateBidHistory(bidCommand);
            }
            catch (Exception e)
            {
                return BadRequest(e);
            }

            if (isUpdate && isAdd)
                return NoContent();
            else
                return BadRequest();
        }

        //http://192.168.1.40:30000/v1/api/auction/finishauction
        [HttpPost("FinishAuction")]
        public ActionResult FinishActioin([FromBody]FinishAuctionCommand finishAuctionCommand)
        {
            var auctionProduct = _auctionContext.Products.FirstOrDefault(x => x.Id == finishAuctionCommand.ProductId);
            if (auctionProduct == null)
                return BadRequest();

            auctionProduct.AuctionStatus = Helper.AuctionStatus.Close;
            try
            {
                _auctionContext.Update(auctionProduct);
                _auctionContext.SaveChanges();

                return NoContent();
            }
            catch (Exception e)
            {
                return BadRequest();
            }
        }

        //http://192.168.1.40:30000/v1/api/auction/addproductdetail
        [HttpPost("AddProductDetail")]
        public ActionResult AddProductDetail([FromBody]ProductDetailCommand productDetailCommand)
        {
            if (productDetailCommand == null)
                return BadRequest();

            Detail newDetail = new Detail()
            {
                ProductId = productDetailCommand.ProductId,
                Topic = productDetailCommand.Topic,
                TopicDetail = productDetailCommand.TopicDetail
            };
            try
            {
                _auctionContext.Add<Detail>(newDetail);
                _auctionContext.SaveChanges();

                return NoContent();
            }
            catch (Exception ex)
            {
                return BadRequest();
            }
        }

        //http://192.168.1.40:30000/v1/api/auction/ProductDetails/{productId}
        [HttpGet("ProductDetails/{productId}")]
        public ActionResult GetProductDetailById(int productId)
        {
            var details = _auctionContext.Details.Where(x => x.Id == productId).ToList();
            if (details == null)
                return BadRequest();

            return Ok(details);
        }

        private bool CreateBidHistory(BidCommand bidCommand)
        {
            BidHistory newBid = new BidHistory()
            {
                BidderId = bidCommand.BidderId,
                Price = bidCommand.Price,
                ProductId = bidCommand.ProductId,
                CreatedDateTime = bidCommand.CreatedDateTime
            };
            try
            {
                _auctionContext.Add<BidHistory>(newBid);
                _auctionContext.SaveChanges();
                return true;
            }
            catch (Exception e)
            {
                return false;
            }
        }
        
    }
}