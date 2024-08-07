﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;
using System.Web;
using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.SlashCommands;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using static System.Net.WebRequestMethods;

namespace RenBot
{
    public class BasicCommands : ApplicationCommandModule
    {
        private readonly GoogleTranslate _googleTranslate;
        private readonly Markov _markov;
        public BasicCommands(GoogleTranslate googleTranslate, Markov markov)
        {
            _googleTranslate = googleTranslate;
            _markov = markov;
        }
        [SlashCommand("ping", "Pong")]
        private async Task Ping(InteractionContext ctx)
        {
            await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder().WithContent("Pong!"));
        }
        [SlashCommand("r8gay", "Rate how gay something is")]
        private async Task R8Gay(InteractionContext ctx, [Option("to-rate", "thing to rate")] string toRate)
        {
            int percentage = Math.Min(Math.Abs(HashCode.Combine(toRate, 69420) % 101), 100);
            DiscordEmbedBuilder embed = new DiscordEmbedBuilder()
            {
                Title = $"{toRate}",
                Description = DiscordEmoji.FromName(ctx.Client, ":straight_ruler:") + "**" + new string(':', percentage) + new string('.', 100 - percentage) + "**" + DiscordEmoji.FromName(ctx.Client, ":rainbow_flag:"),
                Footer = new DiscordEmbedBuilder.EmbedFooter() { Text = $"{percentage}% gay" }
            };
            await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder().AddEmbed(embed));
        }
        [SlashCommand("r8anything", "Rate anything about anything")]
        private async Task R8Anything(InteractionContext ctx, [Option("to-rate", "thing to rate")] string toRate, [Option("quality", "what about it to rate")] string quality)
        {
            int percentage = Math.Min(Math.Abs(HashCode.Combine(toRate, quality) % 101), 100);
            DiscordEmbedBuilder embed = new DiscordEmbedBuilder()
            {
                Title = $"{toRate}",
                Description = DiscordEmoji.FromName(ctx.Client, ":zero:") + "**" + new string(':', percentage) + new string('.', 100 - percentage) + "**" + DiscordEmoji.FromName(ctx.Client, ":100:"),
                Footer = new DiscordEmbedBuilder.EmbedFooter() { Text = $"{percentage}% {quality}" }
            };
            await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder().AddEmbed(embed));
        }
        [SlashCommand("ship", "Ship 2 people or things :3")]
        private async Task Ship(InteractionContext ctx, [Option("thing1", "thing to ship")] string thing1, [Option("thing2", "other thing to ship")] string thing2)
        {
            int percentage = Math.Min(Math.Abs(HashCode.Combine(thing1, thing2) % 101), 100);
            DiscordEmbedBuilder embed = new DiscordEmbedBuilder()
            {
                Title = $"{thing1} x {thing2}",
                Description = DiscordEmoji.FromName(ctx.Client, ":broken_heart:") + "**" + new string(':', percentage) + new string('.', 100 - percentage) + "**" + DiscordEmoji.FromName(ctx.Client, ":heart:"),
                Footer = new DiscordEmbedBuilder.EmbedFooter() { Text = $"I rate this ship {percentage}/100" }
            };
            await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder().AddEmbed(embed));
        }
        [SlashCommand("allowtalking", "Let ren bot speak")]
        private async Task AllowTalking(InteractionContext ctx, [Option("cantalk", "whether he can talk or not")] bool talky)
        {
            JObject json = JObject.Parse(System.IO.File.ReadAllText("../config.json"));

            json["servers"][ctx.Guild.Id.ToString()]["talky"] = talky;

            System.IO.File.WriteAllText("../config.json", json.ToString());

            if (talky)
            {
                await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder().WithContent("I can talk now!"));
            }
            else
            {
                await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder().WithContent("I can't talk anymore :("));
            }
        }
        [SlashCommand("whois", "Gives basic info on a user")]
        private async Task WhoIs(InteractionContext ctx, [Option("user", "user to get info on")] DiscordUser user)
        {
            try
            {
                DiscordEmbedBuilder embed = new DiscordEmbedBuilder()
                {
                    Title = $"{user.Username}",
                    Description = user.Id.ToString(),
                    Footer = new DiscordEmbedBuilder.EmbedFooter() { Text = "" },
                    ImageUrl = user.AvatarUrl,
                    Thumbnail = new DiscordEmbedBuilder.EmbedThumbnail() { Url = user.BannerUrl },
                    Url = user.AvatarUrl
                };

                embed.AddField("Account Created", user.CreationTimestamp.ToString("d"));

                try
                {
                    embed.AddField("Current Status", user.Presence.Status.ToString());
                }
                catch
                {
                    embed.AddField("Current Status", "Couldn't Get Status");
                }

                try
                {
                    embed.AddField("Current Activity", user.Presence.Activities.FirstOrDefault().ActivityType + " " + user.Presence.Activities.FirstOrDefault().Name.ToString());
                }
                catch
                {
                    embed.AddField("Current Activity", "No Activity");
                }

                try
                {
                    embed.AddField("Custom Status", user.Presence.Activities.FirstOrDefault().CustomStatus.Name.ToString());
                }
                catch
                {
                    embed.AddField("Custom Status", "No Custom Status");
                }

                await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder().AddEmbed(embed));
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
        }
        [SlashCommand("define", "Define a word, leave empty for a random word")]
        private async Task Define(InteractionContext ctx, [Option("word", "word to define, leave empty for random")] string word = "")
        {
            HttpClient client = new HttpClient();

            JObject wordJson = JObject.Parse(System.IO.File.ReadAllText("../config.json"));

            DataSet words = JsonConvert.DeserializeObject<DataSet>(await client.GetStringAsync($"https://api.urbandictionary.com/v0/{((string.IsNullOrEmpty(word)) ? "random" : $"define?page=1&term={word}")}"));

            DataTable dataTable = words.Tables["list"];

            DataRow result = dataTable.Rows[0];

            wordJson["word"] = result["word"].ToString();

            System.IO.File.WriteAllText("../config.json", wordJson.ToString());

            if (string.IsNullOrEmpty(word))
            {
                words = JsonConvert.DeserializeObject<DataSet>(await client.GetStringAsync($"https://api.urbandictionary.com/v0/define?page=1&term={result["word"]}"));

                dataTable = words.Tables["list"];

                result = dataTable.Rows[0];
            }

            DiscordEmbedBuilder embed = new DiscordEmbedBuilder()
            {
                Title = result["word"].ToString(),
                Footer = new DiscordEmbedBuilder.EmbedFooter() { Text = $"Page 1/{dataTable.Rows.Count}  •  {DateTime.Now.ToString("t")}" },
                Url = result["permalink"].ToString(),
                Color = DiscordColor.Red
            };

            try
            {
                embed.AddField("Definition", Regex.Replace(result["definition"].ToString(), @"\[(.*?)\]", m => $"{m.Value}(<https://www.urbandictionary.com/define.php?term={HttpUtility.UrlEncode(m.Value.Replace("[", string.Empty).Replace("]", string.Empty))}>)"));

                embed.AddField("Example", Regex.Replace(result["example"].ToString(), @"\[(.*?)\]", m => $"{m.Value}(<https://www.urbandictionary.com/define.php?term={HttpUtility.UrlEncode(m.Value.Replace("[", string.Empty).Replace("]", string.Empty))}>)"));

                embed.AddField("Author", result["author"].ToString());

                embed.AddField("Votes", $"Upvotes: {result["thumbs_up"]}\nDownvotes: {result["thumbs_down"]}");
            }
            catch
            {
                embed.AddField("Error :(", "Too much text");
            }

            var left = new DiscordButtonComponent(ButtonStyle.Primary, "left_arrow_define", "←", true);

            var right = new DiscordButtonComponent(ButtonStyle.Primary, "right_arrow_define", "→", (dataTable.Rows.Count == 1));

            await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder().AddEmbed(embed).AddComponents(new DiscordComponent[]
            {
                left, right
            }));

            client.Dispose();
        }
        [SlashCommand("translate", "Google translate in Ren Bot (real)")]
        private async Task Translate(InteractionContext ctx, [Option("text", "text to translate")] string text, [Option("from", "language to translate from")] string from = "auto", [Option("to", "language to translate to")] string to = "en")
        {
            try
            {
                await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder().WithContent($"{text} from {from} to {to}:\n{HttpUtility.UrlDecode(await _googleTranslate.Translate(text, from, to))}"));
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            };
        }
        [SlashCommand("expose", "Shows the contents of the last deleted message")]
        private async Task Expose(InteractionContext ctx)
        {
            JObject json = JObject.Parse(System.IO.File.ReadAllText("../config.json"));

            DiscordEmbedBuilder embed = new DiscordEmbedBuilder()
            {
                Title = json["servers"][ctx.Guild.Id.ToString()]["deleted_message"]["username"].ToString(),
                Description = json["servers"][ctx.Guild.Id.ToString()]["deleted_message"]["content"].ToString(),
                ImageUrl = (json["servers"][ctx.Guild.Id.ToString()]["deleted_message"]["urls"].ToObject<List<string>>().Count > 0) ? json["servers"][ctx.Guild.Id.ToString()]["deleted_message"]["urls"].ToObject<List<string>>()[0] : string.Empty,
                Footer = new DiscordEmbedBuilder.EmbedFooter() { IconUrl = json["servers"][ctx.Guild.Id.ToString()]["deleted_message"]["avatar"].ToString(), Text = $"Sent at {json["servers"][ctx.Guild.Id.ToString()]["deleted_message"]["timestamp"].ToString()}" }
            };

            if (json["servers"][ctx.Guild.Id.ToString()]["deleted_message"]["urls"].ToObject<List<string>>().Count > 0)
            {
                for (int i = 0; i < json["servers"][ctx.Guild.Id.ToString()]["deleted_message"]["urls"].ToObject<List<string>>().Count; i++)
                {
                    embed.AddField(json["servers"][ctx.Guild.Id.ToString()]["deleted_message"]["filenames"].ToObject<List<string>>()[i], json["servers"][ctx.Guild.Id.ToString()]["deleted_message"]["urls"].ToObject<List<string>>()[i], true);
                }
            }

            await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder().AddEmbed(embed));
        }
        [SlashCommand("generatecolor", "Generates a random color, along with its hex code")]
        private async Task GenerateColor(InteractionContext ctx)
        {
            string color = String.Format("#{0:X6}", RandomNumberGenerator.GetInt32(0, 0x1000000));

            DiscordEmbedBuilder embed = new DiscordEmbedBuilder()
            {
                Title = color,
                Color = new DiscordColor(color)
            };

            var newColor = new DiscordButtonComponent(ButtonStyle.Secondary, "generate_new_color", "New", false);

            await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder().AddEmbed(embed).AddComponents(new DiscordComponent[] { newColor }));
        }
        [SlashCommand("rolldie", "Rolls dice")]
        private async Task RollDie(InteractionContext ctx, [Option("type_of_dice", "Type of die to roll")] DiceEnum die = DiceEnum.D6, [Option("amount", "how many dice to roll")] long amount = 1)
        {
            string color = String.Format("#{0:X6}", RandomNumberGenerator.GetInt32(0, 0x1000000));

            int[] numbers = new int[amount].Select(x => RandomNumberGenerator.GetInt32(1, (int)die + 1)).ToArray();

            DiscordEmbedBuilder embed = new DiscordEmbedBuilder()
            {
                Title = (amount == 1) ? $"Rolling a {die.GetName()}" : $"Rolling {amount} {die.GetName()}s",
                Description = String.Join(" + ", numbers) + $"\n{DiscordEmoji.FromName(ctx.Client, ":game_die:")} {numbers.Sum()}",
                Color = new DiscordColor(color),
            };

            await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder().AddEmbed(embed));
        }
        public enum DiceEnum
        {
            [ChoiceName("D4")]
            D4 = 4,
            [ChoiceName("D6")]
            D6 = 6,
            [ChoiceName("D10")]
            D10 = 10,
            [ChoiceName("D12")]
            D12 = 12,
            [ChoiceName("D20")]
            D20 = 20,
            [ChoiceName("D100")]
            D100 = 100
        }
        [SlashCommand("drunk", "Drunkens your text")]
        private async Task Drunk(InteractionContext ctx, [Option("text", "Text to drunken")] string text)
        {
            IList<string> strings = text.Split(' ');

            int n = strings.Count;
            while (n > 1)
            {
                var box = new byte[1];
                do RandomNumberGenerator.Fill(box);
                while (!(box[0] < n * (Byte.MaxValue / n)));
                var k = (box[0] % n);
                n--;
                var value = strings[k];
                strings[k] = strings[n];
                strings[n] = value;
            }

            await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder().WithContent(string.Join(' ', strings)));
        }
        [SlashCommand("markov", "Generates a sentence using a markov chain fueled by YOU!")]
        public async Task GenerateMarkov(InteractionContext ctx, [Option("input", "initial input paramter (default: blank)")] string input = "")
        {
            await ctx.DeferAsync().ConfigureAwait(false);
            if (string.IsNullOrEmpty(input))
            {
                await ctx.FollowUpAsync(new DiscordFollowupMessageBuilder().WithContent(_markov.Query()));
            }
            else
            {
                await ctx.FollowUpAsync(new DiscordFollowupMessageBuilder().WithContent($"initial input: {input}\n---\n" + _markov.Query(input)));
            }
        }
        [SlashCommand("generaterandomnumber", "Generates a random number within a range")]
        private async Task GenRandomNumber(InteractionContext ctx, [Option("lower-bound", "minimum of idfk")] double lower, [Option("upper-bound", "maximum of idfk")] double upper)
        {
            double result = BitConverter.ToDouble(RandomNumberGenerator.GetBytes(8), 0);

            result = (((result - lower) % (upper - lower)) + (upper - lower)) % (upper - lower) + lower;

            await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder().WithContent(result.ToString()));
        }
        [SlashCommand("sex", "hehehe sex hehehehehehe funny :333")]
        private async Task Sex(InteractionContext ctx, [Option("user", "User to sex")] DiscordUser user)
        {
            if (user.Id == ctx.User.Id)
            {
                await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder().WithContent($"*{ctx.User.Username} sexes themselves...*"));
            }
            else
            {
                await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder().WithContent($"*{ctx.User.Username} sexes {user.Username} professionally*"));
            }
        }
        [SlashCommand("uptime", "Tells you how long Ren Bot has been running since last startup")]
        private async Task Uptime(InteractionContext ctx)
        {
            JObject json = JObject.Parse(System.IO.File.ReadAllText("../config.json"));

            DateTime currentTime = DateTime.UtcNow;
            long CurrentTime = ((DateTimeOffset)currentTime).ToUnixTimeSeconds();

            long StartupTime = long.Parse(json["startup"].ToString());

            await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder().WithContent($"I've been running for **{CurrentTime - StartupTime}** seconds without crashing."));
        }
    }
}
