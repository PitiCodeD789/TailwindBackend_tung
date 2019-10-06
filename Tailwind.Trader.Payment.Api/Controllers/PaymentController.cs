using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Omise;
using Omise.Models;
using Tailwind.Trader.Payment.Api.Command;
using Tailwind.Trader.Payment.Api.Infrastucture;
using Tailwind.Trader.Payment.Api.Models;

namespace Tailwind.Trader.Payment.Api.Controllers
{
    [Route("v1/api/[controller]")]
    [ApiController]
    public class PaymentController : ControllerBase
    {
        private readonly Client client = new Client("pkey_test_5gdhwbsev5jtm6aywj7", "skey_test_5gdhwbsf5de2b3pm9r0");
        private readonly string url = "http://192.168.1.40:30000/v1/api/auction/paidStatus";
        private readonly HttpClient httpClient = new HttpClient();

        private readonly PaymentContext _paymentContext;
        public PaymentController(PaymentContext paymentContext)
        {
            _paymentContext = paymentContext;
        }

        [HttpPost]
        public ActionResult Payment([FromBody]PaymentCommand paymentCommand)
        {
            if (!Identify(paymentCommand))
                return BadRequest();

            if (!CheckCard(paymentCommand.CardNumber))
                return BadRequest("CardNumber Invalid");

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

        private bool Identify(PaymentCommand paymentCommand)
        {
            throw new NotImplementedException();
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
                var result = httpClient.PostAsync(url, httpContent).Result;

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
                    Currency = "thb",
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

        private bool CheckCard(string cardNumber)
        {
            Regex visaRegex = new Regex(@"^4[0-9]{6,}$");
            Regex masterRegex = new Regex(@"^5[1-5][0-9]{5,}|222[1-9][0-9]{3,}|22[3-9][0-9]{4,}|2[3-6][0-9]{5,}|27[01][0-9]{4,}|2720[0-9]{3,}$");
            Regex jcbRegex = new Regex(@"^(3(?:088|096|112|158|337|5(?:2[89]|[3-8][0-9]))\d{12})$");

            if (visaRegex.IsMatch(cardNumber))
                return true;

            if (masterRegex.IsMatch(cardNumber))
                return true;

            if (jcbRegex.IsMatch(cardNumber))
                return true;

            return false;

        }

    }
}
