/*
Ren Bot is a discord bot with some silly features included.
Copyright (C) 2023 - queenoworld

This program is free software: you can redistribute it and/or modify
it under the terms of the GNU Affero General Public License as published by
the Free Software Foundation, either version 3 of the License, or
(at your option) any later version.

This program is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
GNU Affero General Public License for more details.

You should have received a copy of the GNU Affero General Public License
along with this program.  If not, see <https://www.gnu.org/licenses/>.
*/

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