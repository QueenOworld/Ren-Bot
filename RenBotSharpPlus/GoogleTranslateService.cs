/*
Ren Bot is a discord bot with some silly features included.
Copyright (C) 2023 - kingoworld

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

namespace RenBotSharp
{
    public class GoogleTranslateService
    {
        HttpClient client;
        public GoogleTranslateService()
        {
            client = new HttpClient();

            client.DefaultRequestHeaders.Add("User-Agent", "AndroidTranslate/5.3.0.RC02.130475354-53000263 5.1 phone TRANSLATE_OPM5_TEST_1");

            client.DefaultRequestHeaders.TryAddWithoutValidation("Content-Type", "application/x-www-form-urlencoded;charset=utf-8");
        }
        public async Task<string> Translate(string text, string from = "auto", string to = "en")
        {
            var data = new Dictionary<string, string>
            {
                {"sl",HttpUtility.UrlEncode(from)},
                {"tl",HttpUtility.UrlEncode(to)},
                {"q", text}
            };

            HttpResponseMessage response = await client.PostAsync($"https://translate.google.com/translate_a/single?client=at&dt=t&dt=rm&dj=1", new FormUrlEncodedContent(data));

            JObject translations = JObject.Parse(await response.Content.ReadAsStringAsync());

            JToken result = translations["sentences"].FirstOrDefault();

            return HttpUtility.UrlDecode(result["trans"].ToString());
        }
    }
}