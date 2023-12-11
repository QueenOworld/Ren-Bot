﻿/*
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

using DSharpPlus.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RenBotSharp
{
    internal static class Settings
    {
        public static Dictionary<ulong, bool> TalkyServers = new Dictionary<ulong, bool>();
        public static HttpClient client = new HttpClient();
        public static List<DiscordColor> Rainbow = new List<DiscordColor>() { DiscordColor.Red, DiscordColor.Orange, DiscordColor.Yellow, DiscordColor.Green, DiscordColor.Blue, DiscordColor.Purple, DiscordColor.Magenta };
        public static string LastWord = string.Empty;
        public static string CurrentLanguage = File.ReadAllText($"{Environment.CurrentDirectory}/CurrentLanguage.Ren");
        public static Dictionary<ulong, DiscordMessage?> LastDeletedMessage = new Dictionary<ulong, DiscordMessage?>();
        public static Dictionary<string, string> LanguageDictionary = new Dictionary<string, string>()
        {
            { "arabic", "ar-XA" },
            { "aasque", "eu-ES" },
            { "bengali", "bn-IN" },
            { "bulgarian", "bg-BG" },
            { "catalan", "ca-ES" },
            { "chinese", "yue-HK" },
            { "czech", "cs-CZ" },
            { "danish", "da-DK" },
            { "dutch (belgium)", "nl-BE" },
            { "dutch (netherlands)", "nl-NL" },
            { "english (australia)", "en-AU" },
            { "english (india)", "en-IN" },
            { "english (uk)", "en-GB" },
            { "english (us)", "en-US" },
            { "filipino", "fil-PH" },
            { "finnish", "fi-FI" },
            { "french (canada)", "fr-CA" },
            { "french (france)", "fr-FR" },
            { "galician", "gl-ES" },
            { "german", "de-DE" },
            { "greek", "el-GR" },
            { "gujarati", "gu-IN" },
            { "hebrew", "he-IL" },
            { "hindi", "hi-IN" },
            { "hungarian", "hu-HU" },
            { "icelandic", "is-IS" },
            { "indonesian", "id-ID" },
            { "italian", "it-IT" },
            { "japanese", "ja-JP" },
            { "kannada", "kn-IN" },
            { "korean", "ko-KR" },
            { "latvian", "lv-LV" },
            { "lithuanian", "lt-LT" },
            { "malay", "ms-MY" },
            { "malayalam", "ml-IN" },
            { "mandarin chinese", "cmn-CN" },
            { "marathi", "mr-IN" },
            { "norwegian", "nb-NO" },
            { "polish", "pl-PL" },
            { "portuguese (brazil)", "pt-BR" },
            { "portuguese (portugal)", "pt-PT" },
            { "punjabi", "pa-IN" },
            { "romanian", "ro-RO" },
            { "russian", "ru-RU" },
            { "serbian", "sr-RS" },
            { "spanish (spain)", "es-ES" },
            { "spanish (us)", "es-US" },
            { "swedish", "sv-SE" },
            { "tamil", "ta-IN" },
            { "telugu", "te-IN" },
            { "thai", "th-TH" },
            { "turkish", "tr-TR" },
            { "ukrainian", "uk-UA" },
            { "vietnamese", "vi-VN" }
        };
        public static string[] Statuses =
        {
            "Clowning around",
            "Bootleg Ren",
            "I'm the real Ren",
            "Ren is cool",
            "Destiny 2",
            "Did you say Saint 14?",
            "You're not bad, you're just the opposite of good",
            "Just beat my 92nd Vault of Glass run",
            "Simping for Saint 14",
            "*Uncomprehendable screaming*",
            "Robo Ren",
            "I am Ren except no",
            "Rivals of Aether",
            "Maybe",
            ":skull:",
            "W",
            "Ambussing",
            "Geometry Dash",
            "Destiny 2 reference???",
            "Vibing",
            "12 minutes remain",
            "Goofy ah",
            "LAMO",
            "I farded",
            "huh? huh? huh?",
            "That was a big slip-up",
            "You need jesus",
            "Oracle callouts",
            "Left right middle",
            "Damage phase now",
            "I'm in mars",
            "I got relic",
            "I\'m boutta blow",
            "Deepstone Crypt",
            "Bloons TD 6",
            "Balls",
            "Deez Nutz",
            "Renning",
            "Um actually",
            "I rember",
            "Beep boop",
            "I like it in the face",
            "Terraria",
            "tModLoader",
            "Amogus",
            "Hating Canada",
            "Gayming",
            "rule34.xxx",
            "Not browsing nsfw",
            "Ranked",
            "Casual",
            "Duos",
            "in ${Mem}\'s house.",
            "with ${Mem}",
            "I killed ${Mem}",
            "LMFAO",
            "Augh",
            "Ren Bot Better Bot",
            "Is this a fear tactic?",
            "${Mem}",
            "${Mem} is gay",
            "Whar??",
            "Peepee poopoo",
            "Bloodbath",
            "TOP 1 EXTREME DEMON",
            "ELDEN RING",
            "DARK SOULS III",
            "Dead Cells",
            "Renber",
            "This cuh",
            "BRUHHH",
            "WHAT THE HELLLLL",
            "Only in Ohio",
            "I'm such a scorpio",
            "GRR GRR GRR",
            "Sex",
            "Fucking ${Mem}",
            "Reincarnation Level 8",
            "Cock",
            "On crack",
            "There aint no way",
            "Reproducing",
            "192.168.0.1",
            "Yuor'e*",
            "I have your kids",
            "IS THAT A REFERENCE???",
            "The knives in my back >:(",
            "kms",
            "${Mem} moment",
            "HOLY SHIT",
            "Oh wow",
            "WHAT DA HELLLLL OMAGAHD",
            "I can't take this anymore",
            "Hey emo boy",
            "Hey hey, hey emo boy",
            "According to all known laws of aviation",
            "Fortnite",
            "Popped a well",
            "Mmmmm, men",
            "MEN",
            "Ugh, women",
            ";-;",
            ":(",
            ":)",
            ":3",
            "owo",
            "uwu",
            "Guh",
            "Cuh",
            "Free bobux",
            "AMBATUKAM  AHHHHHH",
            "But why?",
            "???",
            "WHAT",
            "HOW",
            "Aint no way",
            "Sleeping",
            "Walmart jumpscare",
            "Meow :3",
            "THE FOG IS COMING",
            "WHAT THE FUCK IS A KILOMETER",
            "Say Gex",
            "Browsing e621",
            "50 Gasoline",
            "Microwaving a spoon",
            "My body is a MACHINE",
            "ඞ",
            "The Old Tale is real!!!!!!",
            "Can we get much higher",
            "So high",
            "Gay balls",
            "Balls",
            "Praise the sun",
            "NIHIL!!!!",
            "BEAR WITNESS",
            "Ashen One",
            "Nameless King <3",
            "<3",
            "Gamign",
            "Deadname twitter :)",
            "Nuh uh",
            "Heavy TF2",
            "You can't say the n word",
            "Those Basketball People",
            "Oh you're live on Twitch?",
            "Team Fortress 2",
            "Shoutout to Non-Binaries",
            "Shoutout to Transgenders",
            "Shoutout to Femboys",
            "Shoutout to Tomboys",
            "Shoutout to Women",
            "Shoutout to Men",
            "Meow mrrp mew :3",
            ":3 :3 :3 :3 :3 :3"
        };
    }
}
