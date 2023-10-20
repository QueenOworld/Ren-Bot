using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Data;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Web;
using System.Security.Cryptography;
using DecimalMath;

namespace RenBotSharp
{
    public static class BankService
    {
        public static decimal GetCurrentValue()
        {
            DateTime currentTime = DateTime.UtcNow;
            long unixTime = ((DateTimeOffset)currentTime).ToUnixTimeSeconds();

            return DecimalEx.Sin(unixTime / 25400 + DecimalEx.Cos(unixTime/14640)) + 0.1m;
        }
        public static decimal CalculateAmountToSteal(decimal balance) 
        {
            return DecimalEx.Log2(balance) * (balance/100);
        }
        public static bool SuccessfulSteal(decimal balance)
        {
            decimal chance = DecimalEx.Log2(balance) * (balance / 100000);
            if (chance > 90)
            {
                chance = 90;
            }
            else if (chance < 1)
            {
                chance = 1;
            }
            return RandomNumberGenerator.GetInt32(0, 101) < chance;
        }
        public static decimal StealChance(decimal balance)
        {
            decimal chance = DecimalEx.Log2(balance) * (balance / 100000);
            if (chance > 90)
            {
                chance = 90;
            }
            else if (chance < 1)
            {
                chance = 1;
            }
            return chance;
        }
    }
}