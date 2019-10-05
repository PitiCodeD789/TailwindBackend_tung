using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Firebase.Database;
using Firebase.Database.Query;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Tailwind.Trader.Auction.Api.Command;
using Tailwind.Trader.Auction.Api.Infrastucture;
using Tailwind.Trader.Auction.Api.Models;
using Tailwind.Trader.Auction.Api.ViewModel;
using TailwindBackend.Commond;

namespace Tailwind.Trader.Auction.Api.Controllers
{
    [Route("v1/api/[controller]")]
    [ApiController]
    public class AuctionController : ControllerBase
    {
        private readonly AuctionContext _auctionContext;
        private readonly CommondData _commondData;

        public AuctionController(AuctionContext auctionContext, CommondData commondData)
        {
            _auctionContext = auctionContext;
            _commondData = commondData;
        }

        //http://192.168.1.40:30000/v1/api/auction/products
        [HttpGet("Products")]
        public ActionResult GetProducts()
        {
            List<Product> result = _auctionContext.Products.Include(x => x.ProductImages).ToList();

            return Ok(result);
        }

        //http://192.168.1.40:30000/v1/api/auction/product/{id}
        [HttpGet("Product/{id}")]
        public ActionResult GetProductById(int id)
        {
            if (id == default)
                return BadRequest();
            var result = _auctionContext.Products.Include(x => x.ProductImages).Include(x => x.Details).FirstOrDefault(x => x.Id == id);
            if (result == null)
                return BadRequest();

            return Ok(result);
        }

        //http://192.168.1.40:30000/v1/api/auction/biddetail/{userId}
        [HttpGet("BidDetail/{userId}")]
        public ActionResult GetBidDetail(int userId)
        {
            if (userId == default)
                return BadRequest();
            var bidDetail = _auctionContext.Products.Include(x => x.BidHistories).Where(product => product.BidHistories.Any(bid => bid.BidderId == userId)).Select(x => new { x.BidHistories, x.ProductImages }).ToList();
            List<BidDetailViewModel> bidDetailViewModel = bidDetail.Select(s => new BidDetailViewModel()
            {
                BidHistories = s.BidHistories.LastOrDefault(x => x.BidderId == userId),
                ProductImages = s.ProductImages.FirstOrDefault(x => x.ImageType == Models.Helper.ImageType.MainPicture).ImagePath,
            }).ToList();

            return Ok(bidDetailViewModel);
        }

        //http://192.168.1.40:30000/v1/api/auction/currentbid/{userId}
        [HttpGet("CurrentBid/{userId}")]
        public ActionResult GetCurrentBid(int userId)
        {
            if (userId == default)
                return BadRequest();
            var bidDetail = _auctionContext.Products.Include(x => x.BidHistories)
                .Where(product => product.BidHistories.Any(bid => bid.BidderId == userId) && product.AuctionStatus == Helper.AuctionStatus.Open)
                .Select(x => new { x.BidHistories, x.ProductImages, x.Name, x.Price, x.HighestBidderName, x.Expired })
                .ToList();

            List<BidDetailViewModel> bidDetailViewModel = bidDetail.Select(s => new BidDetailViewModel()
            {
                BidHistories = s.BidHistories.LastOrDefault(x => x.BidderId == userId),
                ProductImages = s.ProductImages.FirstOrDefault(x => x.ImageType == Models.Helper.ImageType.MainPicture).ImagePath,
                ProductName = s.Name,
                HigherBidder = s.HighestBidderName,
                Price = s.Price,
                Expired = s.Expired
            }).ToList();

            return Ok(bidDetailViewModel);
        }

        //http://192.168.1.40:30000/v1/api/auction/bid
        [HttpPost("Bid")]
        public ActionResult Bid([FromBody]BidCommand bidCommand)
        {
            if (!ModelState.IsValid)
                return BadRequest();
            Product product = _auctionContext.Products.FirstOrDefault(x => x.Id == bidCommand.ProductId);
            if (product == null)
                return BadRequest();

            if (bidCommand.Price <= product.Price)
                return BadRequest("Amount error");

            product.HighestBidderId = bidCommand.BidderId;
            product.HighestBidderName = bidCommand.BidderName;
            product.Price = bidCommand.Price;

            bool isUpdate, isAdd;

            try
            {
                _auctionContext.Update(product);
                _auctionContext.SaveChanges();
                isUpdate = true;
                isAdd = CreateBidHistory(bidCommand);
                AddBid(product.Price, product.HighestBidderName, product.Id);
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

        private async Task AddBid(decimal price, string name, int productId)
        {
            var bidInfo = (await _commondData.Firebase.Child("Bid").OnceAsync<Bid>()).Where(a => a.Object.ProductId == productId).FirstOrDefault();

            if (bidInfo == null)
            {
                Bid newBid = new Bid()
                {
                    ProductId = productId,
                    Price = price,
                    Name = name
                };
                await _commondData.Firebase
                  .Child("Bid").PostAsync(newBid);
            }
            else
            {
                Bid bid = new Bid()
                {
                    ProductId = productId,
                    Price = price,
                    Name = name
                };
                await _commondData.Firebase.Child("Bid").Child(bidInfo.Key).PutAsync(bid);
            }
        }

        //http://192.168.1.40:30000/v1/api/auction/finishauction
        [HttpPost("FinishAuction")]
        public ActionResult FinishActioin([FromBody]FinishAuctionCommand finishAuctionCommand)
        {
            if (!ModelState.IsValid)
                return BadRequest();
            var auctionProduct = _auctionContext.Products.FirstOrDefault(x => x.Id == finishAuctionCommand.ProductId);
            if (auctionProduct == null)
                return BadRequest();

            auctionProduct.AuctionStatus = Models.Helper.AuctionStatus.Close;
            try
            {
                _auctionContext.Update(auctionProduct);
                _auctionContext.SaveChanges();

                if (!DeleteBid(finishAuctionCommand.ProductId).Result)
                    return BadRequest("Not found auction");
                else
                    return NoContent();
            }
            catch (Exception e)
            {
                return BadRequest();
                throw e;
            }
        }

        private async Task<bool> DeleteBid(int productId)
        {
            var bidInfo = (await _commondData.Firebase.Child("Bid").OnceAsync<Bid>()).Where(a => a.Object.ProductId == productId).FirstOrDefault();

            if (bidInfo == null)
            {
                return false;
            }
            else
            {
                await _commondData.Firebase.Child("Bid").Child(bidInfo.Key).DeleteAsync();

                return true;
            }
        }

        //http://192.168.1.40:30000/v1/api/auction/addproductdetail
        [HttpPost("AddProductDetail")]
        public ActionResult AddProductDetail([FromBody]ProductDetailCommand productDetailCommand)
        {
            if (!ModelState.IsValid)
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
            catch (Exception e)
            {
                return BadRequest();
                throw e;
            }
        }

        //http://192.168.1.40:30000/v1/api/auction/ProductDetails/{productId}
        [HttpGet("ProductDetails/{productId}")]
        public ActionResult GetProductDetailById(int productId)
        {
            if (productId == default)
                return BadRequest();
            var details = _auctionContext.Details.Where(x => x.Id == productId).ToList();
            if (details == null)
                return BadRequest();

            return Ok(details);
        }

        //http://192.168.1.40:30000/v1/api/auction/BidHistories/{productId}
        [HttpGet("BidHistories/{productId}")]
        public ActionResult GetBidHistoriesById(int productId)
        {
            if (productId == default)
                return BadRequest();
            var details = _auctionContext.BidHistories.Where(x => x.Id == productId).ToList();
            if (details == null)
                return BadRequest();

            return Ok(details);
        }

        private bool CreateBidHistory(BidCommand bidCommand)
        {
            BidHistory newBid = new BidHistory()
            {
                BidderId = bidCommand.BidderId,
                BidderName = bidCommand.BidderName,
                Price = bidCommand.Price,
                ProductId = bidCommand.ProductId
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
                throw e;
            }
        }

        //http://192.168.1.40:30000/v1/api/auction/paidStatus
        [HttpPost("PaidStatus")]
        public ActionResult ChangePaidStatus([FromBody]PaidStatusCommand paidStatusCommand)
        {
            if (!ModelState.IsValid)
                return BadRequest();
            var product = _auctionContext.Products.FirstOrDefault(x => x.Id == paidStatusCommand.ProductId);
            if (product == null)
                return BadRequest("Not found product");

            if (product.PaidStatus == Models.Helper.PaidStatus.Paid)
                return BadRequest("Already paid");

            if (product.HighestBidderId != paidStatusCommand.PayerId)
                return BadRequest("Wrong user");

            product.PaidStatus = Models.Helper.PaidStatus.Paid;

            try
            {
                _auctionContext.Update<Product>(product);
                _auctionContext.SaveChanges();

                return NoContent();
            }
            catch (Exception e)
            {
                return BadRequest(e);
            }
        }

        ////http://192.168.1.40:30000/v1/api/auction/product/new
        //[HttpPost("Product/new")]
        //public ActionResult CreateProduct([FromBody]NewProductCommand newProduct)
        //{
        //    Product product = new Product()
        //    {
        //        Name = newProduct.Name,
        //        ProductWeight = newProduct.ProductWeight,
        //        Expired = newProduct.Expired,
        //        Price = newProduct.Price
        //    };
        //    try
        //    {
        //        _auctionContext.Add<Product>(product);
        //        _auctionContext.SaveChanges();
        //        return NoContent();
        //    }
        //    catch (Exception e)
        //    {
        //        return BadRequest("Can't add product");
        //    }
        //}

        ////http://192.168.1.40:30000/v1/api/auction/product/image
        //[HttpPost("Product/image")]
        //public ActionResult CreateProduct([FromBody]ImagePathCommand images)
        //{
        //    ProductImagePath productImagePath = new ProductImagePath()
        //    {
        //        ProductId = images.ProductId,
        //    };
        //    try
        //    {
        //        _auctionContext.Add<Product>(product);
        //        _auctionContext.SaveChanges();
        //        return NoContent();
        //    }
        //    catch (Exception e)
        //    {
        //        return BadRequest("Can't add product");
        //    }
        //}
    }
}