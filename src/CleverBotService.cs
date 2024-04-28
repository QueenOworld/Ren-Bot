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

using System;
using System.Linq;
using System.Net.Http;
using System.Security.Cryptography;
using System.Threading.Tasks;
using System.Web;

namespace RenBot
{
    public class CleverBot
    {
        HttpClient client;
        private static readonly string ascii = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789@*_+-./";

        private string cookies = null;

        public CleverBot()
        {
            client = new HttpClient();
            client.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/102.0.0.0 Safari/537.36");
        }

        public async Task<string> SendCleverbotMessage(string stimulus, params string[] context)
        {
            var _context = (string[])context.Clone();

            if (cookies == null)
            {
                var res = await client.GetAsync("https://www.cleverbot.com/extras/conversation-social-min.js");
                cookies = res.Headers.GetValues("set-cookie").FirstOrDefault();
                res.Dispose();
            }

            var payload = $"stimulus={(Escape(stimulus).Contains("%u") ? Escape(Escape(stimulus).Replace("%u", "|")) : Escape(stimulus))}&";

            Array.Reverse(_context);

            for (var i = 0; i < _context.Length; i++)
            {
                payload += $"vText{i + 2}={(Escape(_context[i]).Contains("%u") ? Escape(Escape(_context[i]).Replace("%u", "|")) : Escape(_context[i]))}&";
            }

            payload += "cb_settings_scripting=no&islearning=1&icognoid=wsf&icognocheck=";

            payload += Hash(payload.Substring(7, 26));

            var req = new HttpRequestMessage(HttpMethod.Post, "https://www.cleverbot.com/webservicemin?uc=UseOfficialCleverbotAPI");
            req.Content = new StringContent(payload);
            req.Headers.Add("Cookie", cookies);

            var response = await client.SendAsync(req);

            var text = await response.Content.ReadAsStringAsync();

            req.Content.Dispose();
            req.Dispose();
            response.Dispose();

            return text.Split("\r")[0];
        }

        private string Hash(string input)
        {
            using MD5 md5 = MD5.Create();
            var inputBytes = System.Text.Encoding.ASCII.GetBytes(input);
            var hashBytes = md5.ComputeHash(inputBytes);

            var ret = "";

            for (int i = 0; i < hashBytes.Length; i++)
            {
                ret += hashBytes[i].ToString("x2");
            }

            return ret;
        }

        private string Escape(string str)
        {
            var ret = "";

            for (var i = 0; i < str.Length; i++)
            {
                var c = str[i];

                if (ascii.Contains(c))
                {
                    ret += c;
                }
                else if (c >= 256)
                {
                    ret += $"%u{Convert.ToUInt16(c):X4}";
                }
                else
                {
                    ret += $"%{Convert.ToUInt16('ä'):X2}";
                }
            }

            return ret;
        }
    }
}
