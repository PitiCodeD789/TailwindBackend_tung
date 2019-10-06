using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using Omise;
using Omise.Models;
using Tailwind.Trader.Payment.Api.Command;
using Tailwind.Trader.Payment.Api.Infrastucture;
using Tailwind.Trader.Payment.Api.Models;
using TailwindBackend.Validator;

namespace Tailwind.Trader.Payment.Api.Controllers
{
    [Route("v1/api/[controller]")]
    [ApiController]
    public class PaymentController : ControllerBase
    {
        private readonly Client client;
        private readonly string paidStatusUrl;
        private readonly string checkBidUrl;
        private readonly HttpClient httpClient;
        private readonly PaymentContext _paymentContext;
        private readonly IConfiguration _configuration;

        public PaymentController(PaymentContext paymentContext, IConfiguration configuration)
        {
            _paymentContext = paymentContext;
            _configuration = configuration;
            httpClient = new HttpClient();
            client = new Client(_configuration["Payment:PKey"], _configuration["Payment:SKey"]);
            paidStatusUrl = _configuration["Payment:ChangePaymentStatusUrl"];
            checkBidUrl = _configuration["Payment:CheckBidUrl"];
        }

        [HttpPost]
        public ActionResult Payment([FromBody]PaymentCommand paymentCommand)
        {
            if (!ModelState.IsValid)
                return BadRequest("Input InValid");

            if (!CheckBid(paymentCommand))
                return BadRequest();

            if (!PaymentValidator.CheckCard(paymentCommand.CardNumber))
                return BadRequest("CardNumber Invalid");

            if (!PaymentValidator.CheckCard(paymentCommand.CardNumber))
                return BadRequest("Card number invalid");

            Charge result = Pay(paymentCommand).Result;

            if (result.Status.ToString() == "Successful")
            {
                CreatePaymentTransaction(result, paymentCommand.PayerId, paymentCommand.ProductId);
                bool isChange = ChangePaymentStatus(paymentCommand.PayerId, paymentCommand.ProductId);

                if (isChange)
                    return NoContent();

                return BadRequest("Can't Change PaymentStatus");
            }
            return BadRequest();
        }

        private bool CheckBid(PaymentCommand paymentCommand)
        {
            IdentifyCommand identifyCommand = new IdentifyCommand()
            {
                UserId = paymentCommand.PayerId,
                ProductId = paymentCommand.ProductId,
                Amount = paymentCommand.Amount
            };
            try
            {
                var jsonString = JsonConvert.SerializeObject(identifyCommand);
                HttpContent httpContent = new StringContent(jsonString, Encoding.UTF8, "application/json");
                var result = httpClient.PostAsync(checkBidUrl, httpContent).Result;

                return true;
            }
            catch (Exception e)
            {
                return false;
            }
        }

        private bool ChangePaymentStatus(int payerId, int productId)
        {
            PaidStatusCommand paidStatusCommand = new PaidStatusCommand()
            {
                PayerId = payerId,
                ProductId = productId,
            };
            try
            {
                var jsonString = JsonConvert.SerializeObject(paidStatusCommand);
                HttpContent httpContent = new StringContent(jsonString, Encoding.UTF8, "application/json");
                var result = httpClient.PostAsync(paidStatusUrl, httpContent).Result;

                if (result.IsSuccessStatusCode)
                    return true;

                return false;
            }
            catch (Exception e)
            {
                return false;
            }
        }

        private async Task<Charge> Pay(PaymentCommand paymentCommand)
        {
            try
            {
                Token token = await client.Tokens.Create(new CreateTokenRequest
                {
                    Name = paymentCommand.CardName,
                    Number = paymentCommand.CardNumber,
                    ExpirationMonth = paymentCommand.ExpireMonth,
                    ExpirationYear = paymentCommand.ExpireYear,
                    SecurityCode = paymentCommand.Cvv
                });

                var charge = await client.Charges.Create(new CreateChargeRequest
                {
                    Amount = paymentCommand.Amount,
                    Currency = _configuration["Payment:MainCurrency"],
                    Card = token.Id
                });

                return charge;
            }
            catch (OmiseException e)
            {
                throw e;
            }
        }

        private void CreatePaymentTransaction(Charge charge, int payerId, int productId)
        {
            Models.PaymentTransaction newTransaction = new Models.PaymentTransaction()
            {
                PayerId = payerId,
                ProductId = productId,
                Amount = (charge.Amount / 100),
                PaymentReference = charge.Id,
                PaymentTime = charge.Created
            };

            try
            {
                _paymentContext.Add<PaymentTransaction>(newTransaction);
                _paymentContext.SaveChanges();
            }
            catch (Exception e)
            {
                throw e;
            }
        }
    }
}
