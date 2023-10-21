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