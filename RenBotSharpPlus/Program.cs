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
using System.Threading.Tasks;
using DSharpPlus;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.Json;
using DSharpPlus.SlashCommands;
using DSharpPlus.Net;
using DSharpPlus.Lavalink;
using DSharpPlus.Entities;
using Newtonsoft.Json;
using System.Data;
using System.Text.RegularExpressions;
using System.Web;
using System.Security.Cryptography;
using System.Timers;
using System.Net;

namespace RenBotSharp
{
    public static class Program
    {
        public static IConfiguration config;
        public static DiscordClient Discord;
        public static List<string> CleverBotContext = new List<string>();
        public static CleverBot clever = new CleverBot();
        static async Task Main(string[] args)
        {
            Console.Title = "Ren Bot :3";

            foreach (string i in File.ReadLines($"{Environment.CurrentDirectory}\\Talky.Ren"))
            {
                string[] pair = i.Split(' ');
                Settings.TalkyServers[ulong.Parse(pair[0])] = bool.Parse(pair[1]);
            }

            config = new ConfigurationBuilder()
                .SetBasePath(AppContext.BaseDirectory)
                .AddJsonFile(path: "config.json").Build();

            Discord = new DiscordClient(new DiscordConfiguration()
            {
                Token = config["Token"],
                TokenType = TokenType.Bot,
                Intents = DiscordIntents.All,
                MinimumLogLevel = Microsoft.Extensions.Logging.LogLevel.Debug,
                MessageCacheSize = 1024,
                AlwaysCacheMembers = true
            });

            var endpoint = new ConnectionEndpoint
            {
                Hostname = "127.0.0.1",
                Port = 2333
            };

            var lavalinkConfig = new LavalinkConfiguration
            {
                Password = "youshallnotpass",
                RestEndpoint = endpoint,
                SocketEndpoint = endpoint
            };

            var lavalink = Discord.UseLavalink();
            var slash = Discord.UseSlashCommands();

#if DEBUG
            slash.RegisterCommands<BasicCommandsModule>(864223405774602260);
            slash.RegisterCommands<AudioCommandsModule>(864223405774602260);
            slash.RegisterCommands<ImageCommandsModule>(864223405774602260);
            slash.RegisterCommands<BankCommandsModule>(864223405774602260);
#else
            slash.RegisterCommands<BasicCommandsModule>();
            slash.RegisterCommands<AudioCommandsModule>();
            slash.RegisterCommands<ImageCommandsModule>();
            slash.RegisterCommands<BankCommandsModule>();
#endif

            Discord.MessageDeleted += async (s, e) =>
            {
                Settings.LastDeletedMessage[e.Guild.Id] = e.Message;
            };

            Discord.MessageCreated += async (s, e) =>
            {
                if (e.Message.Author.Id != 1088269352542949436)
                {
                    if (Settings.TalkyServers[e.Guild.Id])
                    {
                        if (e.Message.ChannelId == 1009900125818208276 || e.Message.ChannelId == 891136812091838514 || e.Message.ChannelId == 817559757249314816  || e.Message.ChannelId == 1133010507712974858 || e.Message.ChannelId == 798294820568956940 || e.Message.ChannelId == 1010386646987980830 || e.Message.ChannelId == 853768477750722602)
                        {
                            if (!e.Message.Author.IsBot)
                            {
                                string CleverResponse = await clever.SendCleverbotMessage(e.Message.Content, CleverBotContext.ToArray());
                                CleverBotContext.Add(e.Message.Content);
                                CleverBotContext.Add(CleverResponse);
                                await e.Message.RespondAsync(CleverResponse);
                                if (CleverBotContext.Count > 100)
                                {
                                    CleverBotContext.RemoveAt(0);
                                    CleverBotContext.RemoveAt(0);
                                }
                            }
                        }
                    }
                }
            };

            Discord.ComponentInteractionCreated += async (s, e) =>
            {
                if (e.Id == "left_arrow_define")
                {
                    int currentpage = int.Parse(new Regex(@"\d+(?=\/)").Match(e.Message.Embeds.First().Footer.Text).Value);

                    currentpage--;

                    DataSet words = JsonConvert.DeserializeObject<DataSet>(await Settings.client.GetStringAsync($"https://api.urbandictionary.com/v0/{((string.IsNullOrEmpty(Settings.LastWord)) ? "random" : $"define?page=1&term={Settings.LastWord}")}"));

                    DataTable dataTable = words.Tables["list"];

                    DataRow result = dataTable.Rows[currentpage - 1];

                    DiscordEmbedBuilder embed = new DiscordEmbedBuilder()
                    {
                        Title = result["word"].ToString(),
                        Footer = new DiscordEmbedBuilder.EmbedFooter() { Text = $"Page {currentpage}/{dataTable.Rows.Count}  •  {DateTime.Now.ToString("t")}" },
                        Url = result["permalink"].ToString(),
                        Color = Settings.Rainbow[(currentpage - 1) % 7]
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

                    var left = new DiscordButtonComponent(ButtonStyle.Primary, "left_arrow_define", "←", (currentpage <= 1));

                    var right = new DiscordButtonComponent(ButtonStyle.Primary, "right_arrow_define", "→", (dataTable.Rows.Count <= currentpage));

                    await e.Interaction.CreateResponseAsync(InteractionResponseType.UpdateMessage, new DiscordInteractionResponseBuilder().AddEmbed(embed).AddComponents(new DiscordComponent[]
                    {
                    left, right
                    }));
                }
                else if (e.Id == "right_arrow_define")
                {
                    int currentpage = int.Parse(new Regex(@"\d+(?=\/)").Match(e.Message.Embeds.First().Footer.Text).Value);

                    currentpage++;

                    DataSet words = JsonConvert.DeserializeObject<DataSet>(await Settings.client.GetStringAsync($"https://api.urbandictionary.com/v0/{((string.IsNullOrEmpty(Settings.LastWord)) ? "random" : $"define?page=1&term={Settings.LastWord}")}"));

                    DataTable dataTable = words.Tables["list"];

                    DataRow result = dataTable.Rows[currentpage - 1];

                    DiscordEmbedBuilder embed = new DiscordEmbedBuilder()
                    {
                        Title = result["word"].ToString(),
                        Footer = new DiscordEmbedBuilder.EmbedFooter() { Text = $"Page {currentpage}/{dataTable.Rows.Count}  •  {DateTime.Now.ToString("t")}" },
                        Url = result["permalink"].ToString(),
                        Color = Settings.Rainbow[(currentpage - 1) % 7]
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

                    var left = new DiscordButtonComponent(ButtonStyle.Primary, "left_arrow_define", "←", (currentpage <= 1));

                    var right = new DiscordButtonComponent(ButtonStyle.Primary, "right_arrow_define", "→", (dataTable.Rows.Count <= currentpage));

                    await e.Interaction.CreateResponseAsync(InteractionResponseType.UpdateMessage, new DiscordInteractionResponseBuilder().AddEmbed(embed).AddComponents(new DiscordComponent[]
                    {
                    left, right
                    }));
                }
                else if (e.Id == "generate_new_color")
                {
                    string color = String.Format("#{0:X6}", RandomNumberGenerator.GetInt32(0, 0x1000000));

                    DiscordEmbedBuilder embed = new DiscordEmbedBuilder()
                    {
                        Title = color,
                        Color = new DiscordColor(color)
                    };

                    var newColor = new DiscordButtonComponent(ButtonStyle.Secondary, "generate_new_color", "New", false);

                    await e.Interaction.CreateResponseAsync(InteractionResponseType.UpdateMessage, new DiscordInteractionResponseBuilder().AddEmbed(embed).AddComponents(new DiscordComponent[] { newColor }));
                }
                else if (e.Id == "make_new_bank_account")
                {
                    DiscordEmbedBuilder embed = new DiscordEmbedBuilder()
                    {
                        Title = e.User.Username,
                        Description = $"Created an account!",
                        Color = new DiscordColor(String.Format("#{0:X6}", RandomNumberGenerator.GetInt32(0, 0x1000000))),
                        Footer = new DiscordEmbedBuilder.EmbedFooter() { IconUrl = e.User.AvatarUrl }
                    };

                    Directory.CreateDirectory($"{Environment.CurrentDirectory}\\Bank\\{e.User.Id}");

                    File.WriteAllText($"{Environment.CurrentDirectory}\\Bank\\{e.User.Id}\\Money.Ren", "0.00");

                    File.WriteAllText($"{Environment.CurrentDirectory}\\Bank\\{e.User.Id}\\Safe.Ren", "0.00");

                    File.WriteAllText($"{Environment.CurrentDirectory}\\Bank\\{e.User.Id}\\StealCooldown.Ren", "0");

                    File.WriteAllText($"{Environment.CurrentDirectory}\\Bank\\{e.User.Id}\\SafeCooldown.Ren", "0");

                    DateTime currentTime = DateTime.UtcNow;
                    long unixTime = ((DateTimeOffset)currentTime).ToUnixTimeSeconds();

                    File.WriteAllText($"{Environment.CurrentDirectory}\\Bank\\{e.User.Id}\\Stash.Ren", unixTime.ToString());

                    await e.Interaction.CreateResponseAsync(InteractionResponseType.UpdateMessage, new DiscordInteractionResponseBuilder().AddEmbed(embed));
                }
            };

            Discord.MessageReactionAdded += async (s, e) =>
            {
                try
                {
                    if (e.Guild.Id == 1064410879124320286)
                    {
                        if (e.Emoji.Id == 1165671822470168607 && e.Message.Reactions.Where(x => x.Emoji.Id == 1165671822470168607).Select(x => x).First().Count <= 1)
                        {
                            Uri uriResult;

                            if (Uri.TryCreate(e.Message.Content, UriKind.Absolute, out uriResult) && (uriResult.Scheme == Uri.UriSchemeHttp || uriResult.Scheme == Uri.UriSchemeHttps))
                            {
                                DiscordEmbedBuilder embed = new DiscordEmbedBuilder()
                                {
                                    Title = e.Message.Author.Username,
                                    Description = e.Message.Content,
                                    ImageUrl = (e.Message.Attachments.Count > 0) ? e.Message.Attachments[0].Url : e.Message.Content,
                                    Footer = new DiscordEmbedBuilder.EmbedFooter() { IconUrl = e.Message.Author.AvatarUrl, Text = $"Sent at {e.Message.Timestamp}" }
                                };

                                if (e.Message.Attachments.Count > 0)
                                {
                                    foreach (var attachment in e.Message.Attachments)
                                    {
                                        embed.AddField(attachment.FileName, attachment.Url, true);
                                    }
                                }

                                await s.SendMessageAsync(await Discord.GetChannelAsync(1166583996629667850), embed);
                                return;
                            }
                            else
                            {
                                DiscordEmbedBuilder embed = new DiscordEmbedBuilder()
                                {
                                    Title = e.Message.Author.Username,
                                    Description = e.Message.Content,
                                    ImageUrl = (e.Message.Attachments.Count > 0) ? e.Message.Attachments[0].Url : string.Empty,
                                    Footer = new DiscordEmbedBuilder.EmbedFooter() { IconUrl = e.Message.Author.AvatarUrl, Text = $"Sent at {e.Message.Timestamp}" }
                                };

                                if (e.Message.Attachments.Count > 0)
                                {
                                    foreach (var attachment in e.Message.Attachments)
                                    {
                                        embed.AddField(attachment.FileName, attachment.Url, true);
                                    }
                                }

                                await s.SendMessageAsync(await Discord.GetChannelAsync(1166583996629667850), embed);
                                return;
                            }
                        }
                    }
                }
                catch
                {
                    Console.WriteLine("Nuh Uh (Message not in cache [Fuck you])");
                }
            };

            await Discord.ConnectAsync();
            await lavalink.ConnectAsync(lavalinkConfig); // Make sure this is after Discord.ConnectAsync().

            DateTime currentTime = DateTime.UtcNow;

            File.WriteAllText($"{Environment.CurrentDirectory}\\Startup.Ren", ((DateTimeOffset)currentTime).ToUnixTimeSeconds().ToString());

            await Task.Delay(1000);

            await UpdateStatusAsync();
            System.Timers.Timer timer = new System.Timers.Timer(TimeSpan.FromMinutes(1));
            timer.Elapsed += async (s, e) => await UpdateStatusAsync();
            timer.Start();

            foreach (var i in Discord.Guilds)
            {
                Console.WriteLine($"{i.Key} | {i.Value.Name}");
            }

            await Task.Delay(-1);
        }
        public static async Task UpdateStatusAsync()
        {
            DiscordActivity activity = new DiscordActivity();

            int type = RandomNumberGenerator.GetInt32(0, 6);
            if (type == 4) { type = 5; }
            activity.ActivityType = (ActivityType)type;

            activity.Name = Settings.Statuses[RandomNumberGenerator.GetInt32(0, Settings.Statuses.Length)].Replace("${Mem}", RandomMemberName());

            await Discord.UpdateStatusAsync(activity);
        }
        private static string RandomMemberName()
        {
            return Discord.Guilds[817559120910614570].Members.ElementAt(RandomNumberGenerator.GetInt32(0, Discord.Guilds[817559120910614570].Members.Count)).Value.DisplayName;
        }
    }
}