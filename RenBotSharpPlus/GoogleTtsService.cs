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

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Web;

namespace RenBotSharp
{
    public static class GoogleTextToSpeech
    {
        public class Option
        {
            public Option()
            {
                Lang = "en-AU";
                Slow = false;
                Host = "https://translate.google.com";
            }
            public string Lang { get; set; }
            public bool Slow { get; set; }
            public string Host { get; set; }
        }
        public static string GetAudioUrl(string text, Option options = null)
        {
            options ??= new Option
            {
                Lang = "en",
                Slow = false,
                Host = "https://translate.google.com"
            };

            if (text.Length > 200)
            {
                throw new Exception($"text length ({text.Length}) should be less than 200 characters :3");
            }

            var query = $"ie=UTF-8&q={Uri.EscapeDataString(text)}&tl={Uri.EscapeDataString(options.Lang)}&total=1&idx=0&textlen={text.Length}&client=tw-ob&prev=input&ttsspeed={(options.Slow ? 0.24 : 1)}";
            var url = $"{options.Host}/translate_tts?{query}";

            return url;
        }
    }
}