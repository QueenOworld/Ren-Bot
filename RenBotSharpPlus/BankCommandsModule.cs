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

using DSharpPlus.Entities;
using DSharpPlus;
using DSharpPlus.SlashCommands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using System.Security.Cryptography;
using Newtonsoft.Json.Linq;

namespace RenBotSharp
{
    public class BankCommandsModule : ApplicationCommandModule
    {
        [SlashCommand("balance", "Checks your balance")]
        private async Task Balance(InteractionContext ctx, [Option("user", "Who's balance to check (Default: User who ran the command)")] DiscordUser user = null)
        {
            JObject json = JObject.Parse(File.ReadAllText("config.json"));

            if (user == null)
            {
                user = ctx.User;
            }

            if (ctx.User.Id == user.Id)
            {
                if (json["servers"][ctx.Guild.Id.ToString()]["bank"][user.Id.ToString()] == null)
                {
                    DiscordEmbedBuilder embed = new DiscordEmbedBuilder()
                    {
                        Title = ctx.Guild.Members[user.Id].DisplayName,
                        Description = "You don't have an account, you can create one though!",
                        Color = new DiscordColor(String.Format("#{0:X6}", RandomNumberGenerator.GetInt32(0, 0x1000000))),
                        Footer = new DiscordEmbedBuilder.EmbedFooter() { IconUrl = user.AvatarUrl }
                    };

                    var newAcc = new DiscordButtonComponent(ButtonStyle.Danger, "make_new_bank_account", "Create", false);

                    await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder().AddEmbed(embed).AddComponents(newAcc));
                    return;
                }
            }
            else
            {
                if (json["servers"][ctx.Guild.Id.ToString()]["bank"][user.Id.ToString()] == null)
                {
                    DiscordEmbedBuilder embed = new DiscordEmbedBuilder()
                    {
                        Title = ctx.Guild.Members[user.Id].DisplayName,
                        Description = $"{ctx.Guild.Members[user.Id].DisplayName} doesn't have an account, they can create one though!",
                        Color = new DiscordColor(String.Format("#{0:X6}", RandomNumberGenerator.GetInt32(0, 0x1000000))),
                        Footer = new DiscordEmbedBuilder.EmbedFooter() { IconUrl = user.AvatarUrl }
                    };

                    await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder().AddEmbed(embed));
                    return;
                }
            }

            decimal balance = Convert.ToDecimal(json["servers"][ctx.Guild.Id.ToString()]["bank"][user.Id.ToString()]["money"].ToString());

            decimal safe = Convert.ToDecimal(json["servers"][ctx.Guild.Id.ToString()]["bank"][user.Id.ToString()]["safe"].ToString());

            DiscordEmbedBuilder responseEmbed = new DiscordEmbedBuilder()
            {
                Title = ctx.Guild.Members[user.Id].DisplayName,
                Color = new DiscordColor(String.Format("#{0:X6}", RandomNumberGenerator.GetInt32(0, 0x1000000))),
                Footer = new DiscordEmbedBuilder.EmbedFooter() { IconUrl = user.AvatarUrl }
            };

            DateTime currentTime = DateTime.UtcNow;
            long unixTime = ((DateTimeOffset)currentTime).ToUnixTimeSeconds();

            decimal Stash = (unixTime - Convert.ToInt64(json["servers"][ctx.Guild.Id.ToString()]["bank"][user.Id.ToString()]["stash"].ToString())) * (Decimal.Round(BankService.GetCurrentValue(), 2) + 1);

            responseEmbed.AddField("Balance", "ℝ" + Decimal.Round(balance, 2).ToString());

            responseEmbed.AddField("In Stash", $"ℝ{Decimal.Round(Stash, 2).ToString()} ({Decimal.Round(unixTime - Convert.ToDecimal(json["servers"][ctx.Guild.Id.ToString()]["bank"][user.Id.ToString()]["stash"].ToString()), 2)}*{Decimal.Round(BankService.GetCurrentValue() + 1, 2)})");

            responseEmbed.AddField("In Safe", $"ℝ{Decimal.Round(safe, 2)}");

            await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder().AddEmbed(responseEmbed));
        }
        [SlashCommand("draw", "Draws unclaimed money accumulated money from your stash")]
        private async Task Draw(InteractionContext ctx)
        {
            JObject json = JObject.Parse(File.ReadAllText("config.json"));

            DateTime currentTime = DateTime.UtcNow;
            long unixTime = ((DateTimeOffset)currentTime).ToUnixTimeSeconds();

            decimal Stash = (unixTime - Convert.ToInt64(json["servers"][ctx.Guild.Id.ToString()]["bank"][ctx.User.Id.ToString()]["stash"].ToString())) * (Decimal.Round(BankService.GetCurrentValue(), 2) + 1);

            json["servers"][ctx.Guild.Id.ToString()]["bank"][ctx.User.Id.ToString()]["stash"] = unixTime.ToString();

            decimal balance = Convert.ToDecimal(json["servers"][ctx.Guild.Id.ToString()]["bank"][ctx.User.Id.ToString()]["money"].ToString());

            balance += Stash;

            balance = Decimal.Round(balance, 2);

            json["servers"][ctx.Guild.Id.ToString()]["bank"][ctx.User.Id.ToString()]["money"] = balance.ToString();

            File.WriteAllText("config.json", json.ToString());

            DiscordEmbedBuilder responseEmbed = new DiscordEmbedBuilder()
            {
                Title = ctx.Guild.Members[ctx.User.Id].DisplayName,
                Description = $"Added ℝ{Decimal.Round(Stash, 2)} to your balance!\nNew balance: ℝ{balance}",
                Color = new DiscordColor(String.Format("#{0:X6}", RandomNumberGenerator.GetInt32(0, 0x1000000))),
                Footer = new DiscordEmbedBuilder.EmbedFooter() { IconUrl = ctx.User.AvatarUrl }
            };

            await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder().AddEmbed(responseEmbed));
        }
        [SlashCommand("value", "Checks how much ℝ is currently worth")]
        private async Task Value(InteractionContext ctx)
        {
            decimal currentValue = Decimal.Round(BankService.GetCurrentValue(), 2);

            string valueString = string.Empty;

            if (currentValue >= 0)
            {
                valueString = "ℝ+" + currentValue.ToString();
            }
            else
            {
                valueString = "ℝ" + currentValue.ToString();
            }

            DiscordEmbedBuilder responseEmbed = new DiscordEmbedBuilder()
            {
                Title = "Current ℝ value",
                Description = valueString,
                Color = new DiscordColor(String.Format("#{0:X6}", RandomNumberGenerator.GetInt32(0, 0x1000000))),
            };

            await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder().AddEmbed(responseEmbed));
        }
        [SlashCommand("steal", "Lets you steal ℝ from someone. Less effective against poorer people. :3")]
        private async Task Steal(InteractionContext ctx, [Option("user", "Who to steal from")] DiscordUser user)
        {
            JObject json = JObject.Parse(File.ReadAllText("config.json"));

            if (json["servers"][ctx.Guild.Id.ToString()]["bank"][ctx.User.Id.ToString()] == null)
            {
                DiscordEmbedBuilder bembed = new DiscordEmbedBuilder()
                {
                    Title = ctx.Guild.Members[user.Id].DisplayName,
                    Description = $"You don't have an account, you can create one though!",
                    Color = new DiscordColor(String.Format("#{0:X6}", RandomNumberGenerator.GetInt32(0, 0x1000000))),
                    Footer = new DiscordEmbedBuilder.EmbedFooter() { IconUrl = ctx.User.AvatarUrl }
                };

                await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder().AddEmbed(bembed));
                return;
            }
            else if (json["servers"][ctx.Guild.Id.ToString()]["bank"][user.Id.ToString()] == null)
            {
                DiscordEmbedBuilder bembed = new DiscordEmbedBuilder()
                {
                    Title = ctx.Guild.Members[user.Id].DisplayName,
                    Description = $"{ctx.Guild.Members[user.Id].DisplayName} doesn't have an account, they can create one though!",
                    Color = new DiscordColor(String.Format("#{0:X6}", RandomNumberGenerator.GetInt32(0, 0x1000000))),
                    Footer = new DiscordEmbedBuilder.EmbedFooter() { IconUrl = user.AvatarUrl }
                };

                await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder().AddEmbed(bembed));
                return;
            }

            if (ctx.User.Id == user.Id) 
            {
                await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder().WithContent("You can't steal from yourself!"));
                return; 
            }

            DateTime currentTime = DateTime.UtcNow;
            long unixTime = ((DateTimeOffset)currentTime).ToUnixTimeSeconds();

            long cooldown = Convert.ToInt64(json["servers"][ctx.Guild.Id.ToString()]["bank"][ctx.User.Id.ToString()]["steal_cooldown"].ToString());

            if (cooldown > unixTime)
            {
                await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder().WithContent($"You're on cooldown for {cooldown - unixTime} more seconds"));
                return;
            }

            decimal VictimMoney = Convert.ToDecimal(json["servers"][ctx.Guild.Id.ToString()]["bank"][user.Id.ToString()]["money"].ToString());
            
            if (VictimMoney == 0)
            {
                await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder().WithContent($"{ctx.Guild.Members[user.Id].DisplayName} has no ℝ to steal!"));
                return;
            }

            if (BankService.SuccessfulSteal(VictimMoney))
            {
                decimal AmountToSteal = BankService.CalculateAmountToSteal(VictimMoney);

                VictimMoney -= AmountToSteal;

                decimal TheifMoney = Convert.ToDecimal(json["servers"][ctx.Guild.Id.ToString()]["bank"][ctx.User.Id.ToString()]["money"].ToString());

                TheifMoney += AmountToSteal;

                json["servers"][ctx.Guild.Id.ToString()]["bank"][user.Id.ToString()]["money"] = Decimal.Round(VictimMoney, 2).ToString();

                json["servers"][ctx.Guild.Id.ToString()]["bank"][ctx.User.Id.ToString()]["money"] = Decimal.Round(TheifMoney, 2).ToString();

                DiscordEmbedBuilder embed = new DiscordEmbedBuilder()
                {
                    Title = ctx.Guild.Members[ctx.User.Id].DisplayName,
                    Description = $"Successfully stole ℝ{Decimal.Round(AmountToSteal, 2)} from {ctx.Guild.Members[user.Id].DisplayName} ({Decimal.Round(BankService.StealChance(VictimMoney + AmountToSteal), 2)}% chance)",
                    Color = new DiscordColor(String.Format("#{0:X6}", RandomNumberGenerator.GetInt32(0, 0x1000000))),
                    Footer = new DiscordEmbedBuilder.EmbedFooter() { IconUrl = ctx.User.AvatarUrl }
                };

                embed.AddField($"{ctx.Guild.Members[ctx.User.Id].DisplayName}'s Balance", $"ℝ{Decimal.Round(TheifMoney, 2)} ({Decimal.Round(TheifMoney - AmountToSteal, 2)} + {Decimal.Round(AmountToSteal, 2)})");
                embed.AddField($"{ctx.Guild.Members[user.Id].DisplayName}'s Balance", $"ℝ{Decimal.Round(VictimMoney, 2)} ({Decimal.Round(VictimMoney + AmountToSteal, 2)} - {Decimal.Round(AmountToSteal, 2)})");

                embed.AddField("You're now on cooldown for", "300 seconds");

                json["servers"][ctx.Guild.Id.ToString()]["bank"][ctx.User.Id.ToString()]["steal_cooldown"] = (unixTime + 300).ToString();

                File.WriteAllText("config.json", json.ToString());

                await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder().AddEmbed(embed));
            }
            else
            {
                DiscordEmbedBuilder embed = new DiscordEmbedBuilder()
                {
                    Title = ctx.Guild.Members[ctx.User.Id].DisplayName,
                    Description = $"Your attempt at stealing was ℝ from {ctx.Guild.Members[user.Id].DisplayName} was unsuccessful! ({Decimal.Round(BankService.StealChance(VictimMoney), 2)}% chance)",
                    Color = new DiscordColor(String.Format("#{0:X6}", RandomNumberGenerator.GetInt32(0, 0x1000000))),
                    Footer = new DiscordEmbedBuilder.EmbedFooter() { IconUrl = ctx.User.AvatarUrl }
                };

                embed.AddField("You're now on cooldown for", "60 seconds");

                json["servers"][ctx.Guild.Id.ToString()]["bank"][ctx.User.Id.ToString()]["steal_cooldown"] = (unixTime + 60).ToString();

                File.WriteAllText("config.json", json.ToString());

                await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder().AddEmbed(embed));
            }
        }
        [SlashCommand("gamblecard", "Lets you gamble in a card game")]
        private async Task GambleCard(InteractionContext ctx)
        {

        }
        [SlashCommand("leaderboard", "Shows the top 10 richest people in the server")]
        private async Task Leaderboard(InteractionContext ctx)
        {
            JObject json = JObject.Parse(File.ReadAllText("config.json"));
            
            try
            {
                Dictionary<ulong, decimal> UserAndMoney = new Dictionary<ulong, decimal>();

                foreach (var account in (JObject)json["servers"][ctx.Guild.Id.ToString()]["bank"])
                {
                    if (ctx.Guild.Members.ContainsKey(ulong.Parse(account.Key)))
                    {
                        UserAndMoney[ulong.Parse(account.Key)] = Decimal.Round(Convert.ToDecimal(json["servers"][ctx.Guild.Id.ToString()]["bank"][account.Key]["money"].ToString()) + Convert.ToDecimal(json["servers"][ctx.Guild.Id.ToString()]["bank"][account.Key]["safe"].ToString()), 2);
                    }
                }

                UserAndMoney.OrderByDescending(x => x.Key);

                DiscordEmbedBuilder embed = new DiscordEmbedBuilder()
                {
                    Title = $"Leaderboard for {ctx.Guild.Name}",
                    Color = new DiscordColor(String.Format("#{0:X6}", RandomNumberGenerator.GetInt32(0, 0x1000000))),
                    Footer = new DiscordEmbedBuilder.EmbedFooter() { IconUrl = ctx.User.AvatarUrl }
                };

                Dictionary<ulong, decimal> TopTen = UserAndMoney.OrderByDescending(pair => pair.Value).Take(10).ToDictionary(pair => pair.Key, pair => pair.Value);

                foreach (KeyValuePair<ulong, decimal> user in TopTen)
                {
                    embed.AddField(ctx.Guild.Members[user.Key].DisplayName, $"ℝ{Decimal.Round(user.Value, 2)}");
                }

                await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder().AddEmbed(embed));
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
        }
        [SlashCommand("deposit", "Desposit all your ℝ into your Safe")]
        private async Task Deposit(InteractionContext ctx)
        {
            JObject json = JObject.Parse(File.ReadAllText("config.json"));

            if (json["servers"][ctx.Guild.Id.ToString()]["bank"][ctx.User.Id.ToString()] == null)
            {
                DiscordEmbedBuilder embed = new DiscordEmbedBuilder()
                {
                    Title = ctx.Guild.Members[ctx.User.Id].DisplayName,
                    Description = $"You don't have an account, you can create one though!",
                    Color = new DiscordColor(String.Format("#{0:X6}", RandomNumberGenerator.GetInt32(0, 0x1000000))),
                    Footer = new DiscordEmbedBuilder.EmbedFooter() { IconUrl = ctx.User.AvatarUrl }
                };

                await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder().AddEmbed(embed));
                return;
            }

            DateTime currentTime = DateTime.UtcNow;
            long unixTime = ((DateTimeOffset)currentTime).ToUnixTimeSeconds();

            long cooldown = Convert.ToInt64(json["servers"][ctx.Guild.Id.ToString()]["bank"][ctx.User.Id.ToString()]["safe_cooldown"].ToString());

            if (cooldown > unixTime)
            {
                await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder().WithContent($"You're on safe cooldown for {cooldown - unixTime} more seconds"));
                return;
            }

            decimal balance = Convert.ToDecimal(json["servers"][ctx.Guild.Id.ToString()]["bank"][ctx.User.Id.ToString()]["money"].ToString());
            decimal safe = Convert.ToDecimal(json["servers"][ctx.Guild.Id.ToString()]["bank"][ctx.User.Id.ToString()]["safe"].ToString());

            safe += balance;

            json["servers"][ctx.Guild.Id.ToString()]["bank"][ctx.User.Id.ToString()]["safe"] = Decimal.Round(safe, 2).ToString();
            json["servers"][ctx.Guild.Id.ToString()]["bank"][ctx.User.Id.ToString()]["money"] = "0.00";

            json["servers"][ctx.Guild.Id.ToString()]["bank"][ctx.User.Id.ToString()]["safe_cooldown"] = (unixTime + 60000).ToString();

            File.WriteAllText("config.json", json.ToString());

            DiscordEmbedBuilder responseEmbed = new DiscordEmbedBuilder()
            {
                Title = ctx.Guild.Members[ctx.User.Id].DisplayName,
                Description = $"Deposited {Decimal.Round(balance, 2)} into your safe",
                Color = new DiscordColor(String.Format("#{0:X6}", RandomNumberGenerator.GetInt32(0, 0x1000000))),
                Footer = new DiscordEmbedBuilder.EmbedFooter() { IconUrl = ctx.User.AvatarUrl }
            };

            responseEmbed.AddField("In Safe", Decimal.Round(safe, 2).ToString());

            await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder().AddEmbed(responseEmbed));
        }
        [SlashCommand("safedraw", "Draw all your ℝ from your Safe")]
        private async Task SafeDraw(InteractionContext ctx)
        {
            JObject json = JObject.Parse(File.ReadAllText("config.json"));

            if (json["servers"][ctx.Guild.Id.ToString()]["bank"][ctx.User.Id.ToString()] == null)
            {
                DiscordEmbedBuilder embed = new DiscordEmbedBuilder()
                {
                    Title = ctx.Guild.Members[ctx.User.Id].DisplayName,
                    Description = $"You don't have an account, you can create one though!",
                    Color = new DiscordColor(String.Format("#{0:X6}", RandomNumberGenerator.GetInt32(0, 0x1000000))),
                    Footer = new DiscordEmbedBuilder.EmbedFooter() { IconUrl = ctx.User.AvatarUrl }
                };

                await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder().AddEmbed(embed));
                return;
            }

            DateTime currentTime = DateTime.UtcNow;
            long unixTime = ((DateTimeOffset)currentTime).ToUnixTimeSeconds();

            long cooldown = Convert.ToInt64(json["servers"][ctx.Guild.Id.ToString()]["bank"][ctx.User.Id.ToString()]["safe_cooldown"].ToString());

            if (cooldown > unixTime)
            {
                await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder().WithContent($"You're on safe cooldown for {cooldown - unixTime} more seconds"));
                return;
            }

            decimal balance = Convert.ToDecimal(json["servers"][ctx.Guild.Id.ToString()]["bank"][ctx.User.Id.ToString()]["money"].ToString());
            decimal safe = Convert.ToDecimal(json["servers"][ctx.Guild.Id.ToString()]["bank"][ctx.User.Id.ToString()]["safe"].ToString());

            balance += safe;

            json["servers"][ctx.Guild.Id.ToString()]["bank"][ctx.User.Id.ToString()]["safe"] = "0.00";
            json["servers"][ctx.Guild.Id.ToString()]["bank"][ctx.User.Id.ToString()]["money"] = Decimal.Round(balance, 2).ToString();

            json["servers"][ctx.Guild.Id.ToString()]["bank"][ctx.User.Id.ToString()]["safe_cooldown"] = (unixTime + 60000).ToString();

            File.WriteAllText("config.json", json.ToString());

            DiscordEmbedBuilder responseEmbed = new DiscordEmbedBuilder()
            {
                Title = ctx.Guild.Members[ctx.User.Id].DisplayName,
                Description = $"Drew {Decimal.Round(safe, 2)} from your safe",
                Color = new DiscordColor(String.Format("#{0:X6}", RandomNumberGenerator.GetInt32(0, 0x1000000))),
                Footer = new DiscordEmbedBuilder.EmbedFooter() { IconUrl = ctx.User.AvatarUrl }
            };

            responseEmbed.AddField("In Balance", Decimal.Round(balance, 2).ToString());

            await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder().AddEmbed(responseEmbed));
        }
    }
}

/*[SlashCommand("example", "Example")]
private async Task Example(InteractionContext ctx, [Option("user", "Person")] DiscordUser user)
{
    if (!Directory.Exists($"{Environment.CurrentDirectory}/Bank/{ctx.User.Id}"))
    {
        DiscordEmbedBuilder embed = new DiscordEmbedBuilder()
        {
            Title = user.Username,
            Description = $"You don't have an account, you can create one though!",
            Color = new DiscordColor(String.Format("#{0:X6}", RandomNumberGenerator.GetInt32(0, 0x1000000))),
            Footer = new DiscordEmbedBuilder.EmbedFooter() { IconUrl = user.AvatarUrl }
        };

        await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder().AddEmbed(embed));
        return;
    }
    else if (!Directory.Exists($"{Environment.CurrentDirectory}/Bank/{user.Id}"))
    {
        DiscordEmbedBuilder embed = new DiscordEmbedBuilder()
        {
            Title = user.Username,
            Description = $"{user.Username} doesn't have an account, they can create one though!",
            Color = new DiscordColor(String.Format("#{0:X6}", RandomNumberGenerator.GetInt32(0, 0x1000000))),
            Footer = new DiscordEmbedBuilder.EmbedFooter() { IconUrl = user.AvatarUrl }
        };

        await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder().AddEmbed(embed));
        return;
    }

    if (ctx.User.Id == user.Id)
    {
        return;
    }
}*/