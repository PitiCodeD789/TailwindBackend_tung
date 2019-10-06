using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace TailwindBackend.Validator
{
    public static class PaymentValidator
    {
        public static bool CheckCard(string cardNumber)
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