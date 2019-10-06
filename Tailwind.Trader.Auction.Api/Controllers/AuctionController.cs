using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Firebase.Database;
using Firebase.Database.Query;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
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
        private readonly IConfiguration _configuration;
        private FirebaseClient Firebase;

        public AuctionController(AuctionContext auctionContext, IConfiguration configuration)
        {
            _auctionContext = auctionContext;
            _configuration = configuration;
            Firebase = new FirebaseClient(
            _configuration["Firebase:AuctionBaseUrl"],
            new FirebaseOptions
            {
                AuthTokenAsyncFactory =
            () => Task.FromResult(_configuration["Firebase:AuthKey"])
            });
        }

        //http://192.168.1.40:30000/v1/api/auction/products
        [HttpGet("Products")]
        public ActionResult GetProducts()
        {
            //Check Product Expired
            CheckExpireProduct();

            List<Product> result = _auctionContext.Products.Include(x => x.ProductImages)
                .Where(x => x.AuctionStatus == Helper.AuctionStatus.Open)
                .ToList();

            return Ok(result);
        }

        //http://192.168.1.40:30000/v1/api/auction/product/{id}
        [HttpGet("Product/{id}")]
        public ActionResult GetProductById(int id)
        {
            //Check Product Expired
            CheckExpireProduct();

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
            //Check Product Expired
            CheckExpireProduct();

            var product = _auctionContext.Products.Where(x => x.HighestBidderId == userId && x.AuctionStatus == Helper.AuctionStatus.Close);
            if (product == null)
                return BadRequest();

            return Ok(product);
        }

        //http://192.168.1.40:30000/v1/api/auction/auctioned/{userId}
        [HttpGet("auctioned/{userId}")]
        public ActionResult GetAuctionedItems(int userId)
        {
            //Check Product Expired
            CheckExpireProduct();

            var item = _auctionContext.Products
                .Include(x => x.ProductImages)
                .Where(x => x.HighestBidderId == userId && x.AuctionStatus == Models.Helper.AuctionStatus.Close)
                .ToList();

            if (item == null)
                return Ok();

            //ต้องมีรูป 0
            List<AuctionedViewModel> model = item.Select(s => new AuctionedViewModel()
            {
                Id = s.Id,
                Price = s.Price,
                Name = s.Name,
                HighestBidderId = s.HighestBidderId,
                HighestBidderName = s.HighestBidderName,
                ProductWeight = s.ProductWeight,
                ProductImages = s.ProductImages?.FirstOrDefault(x => x.ImageType == Models.Helper.ImageType.MainPicture).ImagePath,
                CreatedDateTime = s.CreatedDateTme,
                Expired = s.Expired,
                AuctionStatus = s.AuctionStatus,
                PaidStatus = s.PaidStatus
            }).ToList();

            return Ok(model);
        }

        //http://192.168.1.40:30000/v1/api/auction/currentbid/{userId}
        [HttpGet("CurrentBid/{userId}")]
        public ActionResult GetCurrentBid(int userId)
        {
            //Check Product Expired
            CheckExpireProduct();

            if (userId == default)
                return BadRequest();

            var bidDetail = _auctionContext.Products.Include(x => x.BidHistories)
                .Where(product => product.AuctionStatus == Helper.AuctionStatus.Open)
                .Select(x => new { x.BidHistories, x.ProductImages, x.Name, x.Price, x.HighestBidderName, x.PaidStatus , x.Expired })
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
            //Check Product Expired
            CheckExpireProduct();

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
            var bidInfo = (await Firebase.Child(_configuration["Firebase:BidTable"]).OnceAsync<Bid>()).Where(a => a.Object.ProductId == productId).FirstOrDefault();

            if (bidInfo == null)
            {
                Bid newBid = new Bid()
                {
                    ProductId = productId,
                    Price = price,
                    Name = name
                };
                await Firebase
                  .Child(_configuration["Firebase:BidTable"]).PostAsync(newBid);
            }
            else
            {
                Bid bid = new Bid()
                {
                    ProductId = productId,
                    Price = price,
                    Name = name
                };
                await Firebase.Child(_configuration["Firebase:BidTable"]).Child(bidInfo.Key).PutAsync(bid);
            }
        }

        //http://192.168.1.40:30000/v1/api/auction/finishauction
        [HttpPost("FinishAuction")]
        public ActionResult FinishActioin([FromBody]FinishAuctionCommand finishAuctionCommand)
        {
            //Check Product Expired
            CheckExpireProduct();

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
            //Check Product Expired
            CheckExpireProduct();

            var bidInfo = (await Firebase.Child(_configuration["Firebase:BidTable"]).OnceAsync<Bid>()).Where(a => a.Object.ProductId == productId).FirstOrDefault();

            if (bidInfo == null)
            {
                return false;
            }
            else
            {
                await Firebase.Child(_configuration["Firebase:BidTable"]).Child(bidInfo.Key).DeleteAsync();

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
            //Check Product Expired
            CheckExpireProduct();

            if (productId == default)
                return BadRequest();
            var details = _auctionContext.Details.Where(x => x.ProductId == productId).ToList();
            if (details == null)
                return BadRequest();

            return Ok(details);
        }

        //http://192.168.1.40:30000/v1/api/auction/BidHistories/{productId}
        [HttpGet("BidHistories/{productId}")]
        public ActionResult GetBidHistoriesById(int productId)
        {
            //Check Product Expired
            CheckExpireProduct();

            if (productId == default)
                return BadRequest();
            var details = _auctionContext.BidHistories.Where(x => x.ProductId == productId).ToList();
            if (details == null)
                return BadRequest();

            return Ok(details);
        }

        private bool CreateBidHistory(BidCommand bidCommand)
        {
            //Check Product Expired
            CheckExpireProduct();

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
            //Check Product Expired
            CheckExpireProduct();

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

        //http://192.168.1.40:30000/v1/api/auction/bid/check
        [HttpPost("bid/check")]
        public ActionResult CheckBid([FromBody]CheckBidCommand checkBidCommand)
        {
            //Check Product Expired
            CheckExpireProduct();

            var product = _auctionContext.Products.FirstOrDefault(x => x.Id == checkBidCommand.ProductId);

            if (product == null)
                return BadRequest();

            if (product.PaidStatus == Helper.PaidStatus.Paid)
                return BadRequest("Already Paid");

            if (product.HighestBidderId != checkBidCommand.UserId)
                return BadRequest("Wrong User");

            if (checkBidCommand.Amount < product.Price)
                return BadRequest("Wrong Amount");

            return Ok();

        }

        //http://192.168.1.40:30000/v1/api/auction/product/new
        [HttpPost("Product/new")]
        public ActionResult CreateProduct([FromBody]NewProductCommand newProduct)
        {
            Product product = new Product()
            {
                Name = newProduct.Name,
                ProductWeight = newProduct.ProductWeight,
                Expired = newProduct.Expired,
                Price = newProduct.Price
            };
            try
            {
                _auctionContext.Add<Product>(product);
                _auctionContext.SaveChanges();
                return NoContent();
            }
            catch (Exception e)
            {
                return BadRequest("Can't add product");
            }
        }

        //http://192.168.1.40:30000/v1/api/auction/product/image
        [HttpPost("Product/image")]
        public ActionResult CreateProductImage([FromBody]ImagePathCommand images)
        {
            var productImages = _auctionContext.ProductImagePaths.Where(x => x.ProductId == images.ProductId)
                .ToList();

            ProductImagePath productImagePath = new ProductImagePath();
            productImagePath.ProductId = images.ProductId;
            productImagePath.ImagePath = images.ImagePath;
            productImagePath.ImageType = (productImages.Count == 0) ? Helper.ImageType.MainPicture : Helper.ImageType.SidePicture;

            try
            {
                _auctionContext.Add<ProductImagePath>(productImagePath);
                _auctionContext.SaveChanges();

                return NoContent();
            }
            catch (Exception e)
            {
                return BadRequest();
            }

        }

        //http://192.168.1.40:30000/v1/api/auction/TestCheckExpired
        [HttpGet("TestCheckExpired")]
        public ActionResult TestCheckExpired()
        {
            CheckExpireProduct();
            return Ok();
        }

        private async Task CheckExpireProduct()
        {
            List<Product> expiredItems = _auctionContext.Products.Where(x => x.Expired < DateTime.UtcNow).ToList();
            if(expiredItems.Count != 0)
            {
                foreach (Product item in expiredItems)


                {
                    item.AuctionStatus = Helper.AuctionStatus.Close;
                    try
                    {
                        _auctionContext.Update<Product>(item);
                        _auctionContext.SaveChanges();
                    }
                    catch (Exception e)                    
                    {
                        
                    }
                }
            }
        }
    }
}
