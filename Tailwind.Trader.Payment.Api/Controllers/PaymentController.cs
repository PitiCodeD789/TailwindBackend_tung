using System;
using System.Collections.Generic;
using System.Linq;
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
            Charge result = Pay(paymentCommand).Result;

            if (result.Status.ToString() == "Successful")
            {
                CreatePaymentTransaction(result, paymentCommand.PayerId);
                return NoContent();
            }

            return BadRequest();
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
