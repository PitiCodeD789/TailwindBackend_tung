using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
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

        private readonly PaymentContext _paymentContext;
        public PaymentController(PaymentContext paymentContext)
        {
            _paymentContext = paymentContext;
        }

        [HttpPost]
        public ActionResult Payment([FromBody]PaymentCommand paymentCommand)
        {
            if (!CheckCard(paymentCommand.CardNumber))
                return BadRequest("CardNumber Invalid");

            Charge result = Pay(paymentCommand).Result;

            if (result.Status.ToString() == "Successful")
            {
                CreatePaymentTransaction(result, paymentCommand.PayerId);
                return NoContent();
            }

            return BadRequest();
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
                    Currency = "usd",
                    Card = token.Id
                });

                return charge;
            }
            catch (OmiseException e)
            {
                throw e;
            }

        }

        private async void CreatePaymentTransaction(Charge charge, int payerId)
        {
            Models.PaymentTransaction newTransaction = new Models.PaymentTransaction()
            {
                PayerId = payerId,
                Amount = charge.Amount,
                PaymentReference = charge.Id,
                PaymentTime = charge.Created,
            };

            try
            {
                _paymentContext.Add<PaymentTransaction>(newTransaction);
                await _paymentContext.SaveChangesAsync();
            }
            catch (Exception e)
            {
                throw e;
            }
        }
    }
}
